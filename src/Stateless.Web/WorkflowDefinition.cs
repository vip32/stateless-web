namespace Stateless.Web
{
    using System;

    public class WorkflowDefinition : IWorkflowDefinition
    {
        private readonly Action<Workflow> configuration;

        public WorkflowDefinition(
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

        public Workflow Create(WorkflowContext context, ITransitionDispatcher dispatcher)
        {
            context.Name = this.Name;
            context.State ??= this.InitialState;
            return new Workflow(context, dispatcher, this.configuration);
        }
    }
}
