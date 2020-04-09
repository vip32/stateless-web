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

        public bool CanHandle(StateMachine stateMachine)
        {
            return true;
        }

        public Task OnEntryAsync(StateMachine stateMachine)
        {
            this.logger?.LogInformation($"state entry: {stateMachine.Context.State} (handler={this.GetType().Name}, trigger={stateMachine.Context.Trigger})");
            return Task.CompletedTask;
        }

        public Task OnExitAsync(StateMachine stateMachine)
        {
            this.logger?.LogInformation($"state exit: {stateMachine.Context.State} (handler={this.GetType().Name}, trigger={stateMachine.Context.Trigger})");
            return Task.CompletedTask;
        }
    }
}
