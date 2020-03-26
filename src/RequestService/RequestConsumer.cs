﻿namespace RequestService
{
    using System.Threading.Tasks;
    using MassTransit;
    using Sample.MessageTypes;

    public class RequestConsumer :
        IConsumer<ISimpleRequest>
    {
        public async Task Consume(ConsumeContext<ISimpleRequest> context)
        {
            System.Console.WriteLine("Returning name for {0}", context.Message.CustomerId);

            context.Respond(new SimpleResponse
            {
                CusomerName = string.Format("Customer Number {0}", context.Message.CustomerId)
            });
        }


        class SimpleResponse :
            ISimpleResponse
        {
            public string CusomerName { get; set; }
        }
    }
}