namespace RequestService
{
    using System;
    using System.Configuration;
    using MassTransit;
    using MassTransit.RabbitMqTransport;
    using MassTransit.Util;
    using Topshelf;
    using Topshelf.Logging;


    class RequestService :
        ServiceControl
    {
        readonly LogWriter _log = HostLogger.Get<RequestService>();

        IBusControl _busControl;

        public bool Start(HostControl hostControl)
        {
            _log.Info("Creating bus...");

            _busControl = Bus.Factory.CreateUsingRabbitMq(x =>
            {
                x.Host(new Uri(ConfigurationManager.AppSettings["RabbitMQHost"]), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                x.ReceiveEndpoint(ConfigurationManager.AppSettings["ServiceQueueName"], e =>
                {
                    e.Instance(new RequestResponseConsumer());
                    e.Consumer<RequestConsumer>();
                });

                x.ReceiveEndpoint("Activity_compensate", e => e.CompensateActivityHost<Activity, ActivityLog>());
                x.ReceiveEndpoint("Activity_execute", e => e.ExecuteActivityHost<Activity, ActivityArgs>(new Uri("exchange:Activity_compensate")));
                x.ReceiveEndpoint("Activity2_execute", e => e.ExecuteActivityHost<Activity2, Activity2Args>());
            });

            _log.Info("Starting bus...");

            TaskUtil.Await(() => _busControl.StartAsync());

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _log.Info("Stopping bus...");

            _busControl?.Stop();

            return true;
        }
    }
}