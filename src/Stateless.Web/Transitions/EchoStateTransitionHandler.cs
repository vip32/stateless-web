namespace Stateless.Web
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class EchoStateTransitionHandler : IStateTransitionHandler
    {
        private readonly ILogger<EchoStateTransitionHandler> logger;

        public EchoStateTransitionHandler(ILogger<EchoStateTransitionHandler> logger)
        {
            this.logger = logger;
        }

        public bool CanHandle(Workflow workflow)
        {
            return true;
        }

        public Task OnEntryAsync(Workflow workflow)
        {
            this.logger?.LogInformation($"state entry: {workflow.Context.State} (handler={this.GetType().Name})");
            return Task.CompletedTask;
        }

        public Task OnExitAsync(Workflow workflow)
        {
            this.logger?.LogInformation($"state exit: {workflow.Context.State} (handler={this.GetType().Name})");
            return Task.CompletedTask;
        }
    }
}
