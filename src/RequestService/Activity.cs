using System;
using System.Threading.Tasks;
using MassTransit.Courier;

namespace RequestService
{
    public class Activity : IActivity<ActivityArgs, ActivityLog>
    {
        public async Task<ExecutionResult> Execute(ExecuteContext<ActivityArgs> context)
        {
            Console.WriteLine("Completining Activity...");
            return context.Completed(new { });
        }

        public async Task<CompensationResult> Compensate(CompensateContext<ActivityLog> context)
        {
            Console.WriteLine("Compensating Activity...");
            return context.Compensated();
        }
    }

    public interface ActivityArgs
    {
        string Name { get; }
    }

    public interface ActivityLog
    {
    }
}
