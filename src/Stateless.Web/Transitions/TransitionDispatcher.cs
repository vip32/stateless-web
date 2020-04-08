namespace Stateless.Web
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class TransitionDispatcher : ITransitionDispatcher
    {
        private readonly IEnumerable<IStateTransitionHandler> handlers;

        public TransitionDispatcher(IEnumerable<IStateTransitionHandler> handlers = null)
        {
            this.handlers = handlers;
        }

        public async Task OnEntryAsync(StateMachine stateMachine)
        {
            foreach (var handler in this.handlers.Safe())
            {
                if (handler.CanHandle(stateMachine))
                {
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
                    await handler.OnExitAsync(stateMachine).ConfigureAwait(false);
                }
            }
        }
    }
}