using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit.Courier;

namespace RequestService
{
    public class Activity2 : IActivity<Activity2Args, Activity2Log>
    {
        public async Task<ExecutionResult> Execute(ExecuteContext<Activity2Args> context)
        {
            if (context.Arguments.Name.Contains("error"))
            {
                throw new Exception("Unknown error...");
            }

            if (context.Arguments.Name.Contains("test"))
            {
                var variables = new Dictionary<string, object>() {
                    { "ValidationErrors", new List<string>() { "FOO", "BAR" } }
                };
                return context.ReviseItinerary(new { }, variables,
                    builder =>
                    {
                        // Adding this as a workaround in order to be able to add variables to a faulted routing slip
                        builder.AddActivity("FaultyActivity", new Uri("exchange:FaultyActivity_execute"));
                        builder.AddActivitiesFromSourceItinerary();
                    });
            }

            Console.WriteLine("Completing Activity2...");
            return context.CompletedWithVariables(
                new
                {
                    CustomerDetails = new CustomerDetails() { Name = "Hello " + context.Arguments.Name }
                });
        }

        public async Task<CompensationResult> Compensate(CompensateContext<Activity2Log> context)
        {
            Console.WriteLine("Complensatiing Activity2...");
            return context.Compensated();
        }
    }

    public interface Activity2Args
    {
        string Name { get; }

        Uri ResponseAddress { get; }
    }

    public interface Activity2Log { }
}

