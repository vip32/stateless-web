namespace Stateless.Web
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class OnOffBrokenStateTransitionHandler : IStateTransitionHandler
    {
        private readonly ILogger<EchoStateTransitionHandler> logger;

        public OnOffBrokenStateTransitionHandler(ILogger<EchoStateTransitionHandler> logger)
        {
            this.logger = logger;
        }

        public bool CanHandle(StateMachine stateMachine)
        {
            return stateMachine.Context.State.SafeEquals(OnOffStateMachine.States.Broken);
        }

        public Task OnEntryAsync(StateMachine stateMachine)
        {
            this.logger?.LogInformation($"{stateMachine.Context.Trigger} > PLEASE CALL A HANDYMAN");

            return Task.CompletedTask;
        }

        public Task OnExitAsync(StateMachine stateMachine)
        {
            if (stateMachine.Context.Trigger.SafeEquals(OnOffStateMachine.Triggers.Repair))
            {
                this.logger?.LogInformation($"{stateMachine.Context.Trigger} > PRAISE TO THE HANDYMAN");
            }

            return Task.CompletedTask;
        }
    }
}
