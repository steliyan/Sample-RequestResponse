using System;
using System.Threading.Tasks;
using MassTransit.Courier;

namespace RequestService
{
    public class Activity2 : IExecuteActivity<Activity2Args>
    {
        public async Task<ExecutionResult> Execute(ExecuteContext<Activity2Args> context)
        {
            if (context.Arguments.Name.Contains("test"))
            {
                Console.WriteLine("Faulting Activity2...");
                throw new CustomException() { Data = "Something went wrong..." };
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
    }
}
