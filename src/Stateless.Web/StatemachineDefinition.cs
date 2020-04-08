namespace Stateless.Web
{
    using System;

    public class StatemachineDefinition : IStatemachineDefinition
    {
        private readonly Action<StateMachine> configuration;

        public StatemachineDefinition(
            string name,
            string initialState,
            Action<StateMachine> configuration = null)
        {
            this.Name = name;
            this.InitialState = initialState;
            this.configuration = configuration;
        }

        public string Name { get; }

        public string InitialState { get; }

        public StateMachine Create(StateMachineContext context, ITransitionDispatcher dispatcher)
        {
            context.Name = this.Name;
            context.State ??= this.InitialState;
            return new StateMachine(context, dispatcher, this.configuration);
        }
    }
}
