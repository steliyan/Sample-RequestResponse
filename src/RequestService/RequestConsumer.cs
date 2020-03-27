namespace RequestService
{
    using System;
    using MassTransit;
    using MassTransit.Courier;
    using Sample.MessageTypes;

    public class RequestConsumer :
        RoutingSlipRequestProxy<ISimpleRequest>
    {
        protected override void BuildRoutingSlip(RoutingSlipBuilder builder, ConsumeContext<ISimpleRequest> request)
        {
            builder.AddActivity("Activity", new Uri("exchange:Activity_execute"));
            builder.AddActivity("Activity2", new Uri("exchange:Activity2_execute"), new {
                Name = request.Message.CustomerId,
            });
        }

        class SimpleResponse :
            ISimpleResponse
        {
            public string CusomerName { get; set; }
        }
    }
}