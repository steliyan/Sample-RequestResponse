﻿namespace Client
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using MassTransit;
    using MassTransit.Util;
    using Sample.MessageTypes;

    class Program
    {
        static void Main()
        {
            IBusControl busControl = CreateBus();

            TaskUtil.Await(() => busControl.StartAsync());

            try
            {
                IRequestClient<ISimpleRequest> client = CreateRequestClient(busControl);

                for (;;)
                {
                    Console.Write("Enter customer id (quit exits): ");
                    string customerId = Console.ReadLine();
                    if (customerId == "quit")
                        break;

                    // this is run as a Task to avoid weird console application issues
                    Task.Run(async () =>
                    {
                        var (response, error) = await client.GetResponse<ISimpleResponse, ISimpleFailResponse>(new SimpleRequest(customerId));

                        if (response.IsCompleted && response.Status == TaskStatus.RanToCompletion)
                        {
                            Console.WriteLine("Customer Name: {0}", response.Result.Message.CusomerName);
                        }

                        if (error.IsCompleted)
                        {
                            if (error.Status == TaskStatus.RanToCompletion)
                            {
                                Console.WriteLine("Fail: {0}", error.Result.Message.Reason);
                            }
                            else if (error.Status == TaskStatus.Faulted)
                            {
                                Console.WriteLine("Error occurred: {0}", error.Exception);
                            }
                        }
                    }).Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!!! OMG!!! {0}", ex);
            }
            finally
            {
                busControl.Stop();
            }
        }


        static IRequestClient<ISimpleRequest> CreateRequestClient(IBusControl busControl)
        {
            var serviceAddress = new Uri(ConfigurationManager.AppSettings["ServiceAddress"]);
            IRequestClient<ISimpleRequest> client =
                busControl.CreateRequestClient<ISimpleRequest>(serviceAddress, TimeSpan.FromSeconds(10));

            return client;
        }

        static IBusControl CreateBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(x => x.Host(new Uri(ConfigurationManager.AppSettings["RabbitMQHost"]), h =>
            {
                h.Username("guest");
                h.Password("guest");
            }));
        }


        class SimpleRequest :
            ISimpleRequest
        {
            readonly string _customerId;
            readonly DateTime _timestamp;

            public SimpleRequest(string customerId)
            {
                _customerId = customerId;
                _timestamp = DateTime.UtcNow;
            }

            public DateTime Timestamp
            {
                get { return _timestamp; }
            }

            public string CustomerId
            {
                get { return _customerId; }
            }
        }
    }
}