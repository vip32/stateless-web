namespace Stateless.Web
{
    public interface IStatemachineDefinition
    {
        string Name { get; }

        string InitialState { get; }

        Workflow Create(StateMachineContext instance, ITransitionDispatcher dispatcher);
    }
}
