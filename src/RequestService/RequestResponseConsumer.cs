namespace RequestService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;
    using MassTransit.Events;
    using Sample.MessageTypes;

    // Copy/pasted from the original RoutingSlipResponseProxy with added logic for handling validation errors
    public abstract class RoutingSlipResponseProxy2<TRequest, TResponse> :
        IConsumer<RoutingSlipCompleted>,
        IConsumer<RoutingSlipFaulted>
        where TRequest : class
        where TResponse : class
    {
        public async Task Consume(ConsumeContext<RoutingSlipCompleted> context)
        {
            var request = context.Message.GetVariable<TRequest>("Request");
            var requestId = context.Message.GetVariable<Guid>("RequestId");

            Uri responseAddress = null;
            if (context.Message.Variables.ContainsKey("FaultAddress"))
                responseAddress = context.Message.GetVariable<Uri>("FaultAddress");
            if (responseAddress == null && context.Message.Variables.ContainsKey("ResponseAddress"))
                responseAddress = context.Message.GetVariable<Uri>("ResponseAddress");

            if (responseAddress == null)
                throw new ArgumentException($"The response address could not be found for the faulted routing slip: {context.Message.TrackingNumber}");

            var endpoint = await context.GetSendEndpoint(responseAddress).ConfigureAwait(false);

            var response = CreateResponseMessage(context, request);

            await endpoint.Send(response, x => x.RequestId = requestId)
                .ConfigureAwait(false);
        }

        public async Task Consume(ConsumeContext<RoutingSlipFaulted> context)
        {
            var request = context.Message.GetVariable<TRequest>("Request");
            var requestId = context.Message.GetVariable<Guid>("RequestId");

            Uri responseAddress = null;
            if (context.Message.Variables.ContainsKey("FaultAddress"))
                responseAddress = context.Message.GetVariable<Uri>("FaultAddress");
            if (responseAddress == null && context.Message.Variables.ContainsKey("ResponseAddress"))
                responseAddress = context.Message.GetVariable<Uri>("ResponseAddress");

            if (responseAddress == null)
                throw new ArgumentException($"The response address could not be found for the faulted routing slip: {context.Message.TrackingNumber}");

            var endpoint = await context.GetSendEndpoint(responseAddress).ConfigureAwait(false);

            ActivityException[] exceptions = context.Message.ActivityExceptions;

            if (context.Message.Variables.ContainsKey("ValidationErrors"))
            {
                var errors = context.Message.GetVariable<List<string>>("ValidationErrors");

                await endpoint.Send<ISimpleFailResponse>(new { Reason = string.Join(",", errors) }, x => x.RequestId = requestId)
                    .ConfigureAwait(false);
            }
            else
            {
                await endpoint.Send<Fault<TRequest>>(
                        new FaultEvent<TRequest>(request, requestId, context.Host, exceptions.Select(x => x.ExceptionInfo),
                            context.SupportedMessageTypes.ToArray()), x => x.RequestId = requestId)
                    .ConfigureAwait(false);
            }
        }

        protected abstract TResponse CreateResponseMessage(ConsumeContext<RoutingSlipCompleted> context, TRequest request);
    }


    public class RequestResponseConsumer :
        RoutingSlipResponseProxy2<ISimpleRequest, ISimpleResponse>
    {
        protected override ISimpleResponse CreateResponseMessage(ConsumeContext<RoutingSlipCompleted> context, ISimpleRequest request)
        {
            var details = context.Message.GetVariable<CustomerDetails>("CustomerDetails");
            return new SimpleResponse() { CusomerName = details.Name };
        }

        class SimpleResponse :
            ISimpleResponse
        {
            public string CusomerName { get; set; }
        }
    }
}