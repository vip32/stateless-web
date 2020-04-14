namespace Stateless.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Stateless;

    public class StateMachine
    {
        private readonly StateMachine<string, string> machine;

        private StateMachine(string state)
        {
            this.machine = new StateMachine<string, string>(state);
        }

        public IEnumerable<string> PermittedTriggers => this.machine.PermittedTriggers;

        public StateMachineContext Context { get; private set; }

        public ITransitionDispatcher Dispatcher { get; private set; }

        public static StateMachine Create(
            StateMachineContext context,
            ITransitionDispatcher dispatcher,
            Action<StateMachine> configuration = null)
        {
            var result = new StateMachine(context.State)
            {
                Context = context,
                Dispatcher = dispatcher
            };

            configuration?.Invoke(result);
            return result;
        }

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
            if (this.Context.IsExpired())
            {
                throw new Exception("statemachine: cannot fire trigger on expired state machines");
            }

            if (this.machine.CanFire(trigger))
            {
                this.Context.Trigger = trigger;
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

        internal StateMachine<string, string>.StateConfiguration Configure(string state)
        {
            return this.machine.Configure(state);
        }
    }
}
