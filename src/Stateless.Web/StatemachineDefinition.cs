namespace Stateless.Web
{
    using System;

    public class StatemachineDefinition : IStatemachineDefinition
    {
        private readonly Action<Workflow> configuration;

        public StatemachineDefinition(
            string name,
            string initialState,
            Action<Workflow> configuration = null)
        {
            this.Name = name;
            this.InitialState = initialState;
            this.configuration = configuration;
        }

        public string Name { get; }

        public string InitialState { get; }

        public Workflow Create(StateMachineContext context, ITransitionDispatcher dispatcher)
        {
            context.Name = this.Name;
            context.State ??= this.InitialState;
            return new Workflow(context, dispatcher, this.configuration);
        }
    }
}
