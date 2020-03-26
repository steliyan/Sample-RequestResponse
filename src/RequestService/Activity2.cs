using System;
using System.Threading.Tasks;
using MassTransit.Courier;
using Sample.MessageTypes;

namespace RequestService
{
    public class Activity2 : IExecuteActivity<Activity2Args>
    {
        public async Task<ExecutionResult> Execute(ExecuteContext<Activity2Args> context)
        {
            if (context.Arguments.Name.Contains("error"))
            {
                throw new Exception("Unknown error...");
            }

            if (context.Arguments.Name.Contains("test"))
            {
                var endpoint = await context.GetSendEndpoint(context.Arguments.ResponseAddress);
                await endpoint.Send<ISimpleFailResponse>(new { Reason = "Something went wrong..." });

                throw new Exception("Something went wrong error...");
            }

            Console.WriteLine("Completing Activity2...");
            return context.CompletedWithVariables(
                new
                {
                    CustomerDetails = new CustomerDetails() { Name = "Hello " + context.Arguments.Name }
                });
        }
    }

    public interface Activity2Args
    {
        string Name { get; }

        Uri ResponseAddress { get; }
    }
}
