namespace RequestService
{
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;
    using Sample.MessageTypes;

    public class RequestResponseConsumer :
        RoutingSlipResponseProxy<ISimpleRequest, ISimpleResponse>
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