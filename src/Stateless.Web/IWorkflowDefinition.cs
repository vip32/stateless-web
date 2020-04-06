namespace Stateless.Web
{
    public interface IWorkflowDefinition
    {
        string Name { get; }

        string InitialState { get; }

        Workflow Create(WorkflowContext instance, ITransitionDispatcher stateHandler);
    }
}
