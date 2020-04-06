namespace Stateless.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Stateless;

    public class Workflow
    {
        private readonly StateMachine<string, string> machine;

        internal Workflow(WorkflowContext context, ITransitionDispatcher dispatcher, Action<Workflow> configuration = null)
        {
            this.machine = new StateMachine<string, string>(context.State);
            configuration?.Invoke(this);
            this.Context = context;
            this.Dispatcher = dispatcher;
        }

        public IEnumerable<string> PermittedTriggers => this.machine.PermittedTriggers;

        public WorkflowContext Context { get; }

        public ITransitionDispatcher Dispatcher { get; }

        public void Activate()
        {
            this.machine.Activate();
        }

        public void Deactivate()
        {
            this.machine.Deactivate();
        }

        public async Task<bool> FireAsync(string trigger)
        {
            if (this.machine.CanFire(trigger))
            {
                await this.machine.DeactivateAsync().ConfigureAwait(false);
                await this.machine.FireAsync(trigger).ConfigureAwait(false);
                await this.machine.ActivateAsync().ConfigureAwait(false);

                this.Context.State = this.machine.State;

                return true;
            }
            else
            {
                return false;
            }
        }

        internal StateMachine<string, string>.StateConfiguration Configure(string state) // leaky?
        {
            return this.machine.Configure(state);
        }
    }
}
