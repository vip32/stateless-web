namespace Stateless.Web
{
    public interface IStatemachineDefinition
    {
        string Name { get; }

        string InitialState { get; }

        StateMachine Create(StateMachineContext instance, ITransitionDispatcher dispatcher);
    }
}
