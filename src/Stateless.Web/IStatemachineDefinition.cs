namespace Stateless.Web
{
    public interface IStatemachineDefinition
    {
        string Name { get; }

        string InitialState { get; }

        StateMachine CreateInstance(StateMachineContext instance, ITransitionDispatcher dispatcher);
    }
}
