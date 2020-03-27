using System.Threading.Tasks;
using MassTransit.Courier;

namespace RequestService
{
    public class FaultyActivity : IExecuteActivity<FaultyArgs>
    {
        public async Task<ExecutionResult> Execute(ExecuteContext<FaultyArgs> context)
        {
            return context.Faulted();
        }
    }

    public interface FaultyArgs
    {
    }
}
