namespace Stateless.Web
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class TransitionDispatcher : ITransitionDispatcher
    {
        private readonly ILogger<TransitionDispatcher> logger;
        private readonly IEnumerable<IStateTransitionHandler> handlers;

        public TransitionDispatcher(ILogger<TransitionDispatcher> logger, IEnumerable<IStateTransitionHandler> handlers = null)
        {
            this.logger = logger;
            this.handlers = handlers;
        }

        public async Task OnEntryAsync(StateMachine stateMachine)
        {
            foreach (var handler in this.handlers.Safe())
            {
                if (handler.CanHandle(stateMachine))
                {
                    this.logger?.LogInformation($"statemachine: transition handler entry {stateMachine.Context.State} (handler={handler.GetType().Name}, trigger={stateMachine.Context.Trigger})");
                    await handler.OnEntryAsync(stateMachine).ConfigureAwait(false);
                }
            }
        }

        public async Task OnExitAsync(StateMachine stateMachine)
        {
            foreach (var handler in this.handlers.Safe())
            {
                if (handler.CanHandle(stateMachine))
                {
                    this.logger?.LogInformation($"statemachine: transition handler exit {stateMachine.Context.State} (handler={handler.GetType().Name}, trigger={stateMachine.Context.Trigger})");
                    await handler.OnExitAsync(stateMachine).ConfigureAwait(false);
                }
            }
        }
    }
}