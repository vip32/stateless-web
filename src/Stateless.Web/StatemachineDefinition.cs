namespace Stateless.Web
{
    using System;

    public class StatemachineDefinition : IStatemachineDefinition
    {
        private readonly Action<StateMachine> configuration;

        public StatemachineDefinition(
            string name,
            string initialState,
            Action<StateMachine> configuration = null,
            TimeSpan? ttl = null)
        {
            this.Name = name;
            this.InitialState = initialState;
            this.configuration = configuration;
            this.Ttl = ttl;
        }

        public string Name { get; }

        public string InitialState { get; }

        public TimeSpan? Ttl { get; }

        public StateMachine CreateInstance(StateMachineContext context, ITransitionDispatcher dispatcher)
        {
            context.Name = this.Name;
            context.State ??= this.InitialState;
            context.Ttl = this.Ttl;
            return StateMachine.Create(context, dispatcher, this.configuration);
        }
    }
}
