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

        public async Task OnEntryAsync(Workflow workflow)
        {
            foreach (var handler in this.handlers.Safe())
            {
                if (handler.CanHandle(workflow))
                {
                    await handler.OnEntryAsync(workflow).ConfigureAwait(false);
                }
            }
        }

        public async Task OnExitAsync(Workflow workflow)
        {
            foreach (var handler in this.handlers.Safe())
            {
                if (handler.CanHandle(workflow))
                {
                    await handler.OnExitAsync(workflow).ConfigureAwait(false);
                }
            }
        }
    }
}