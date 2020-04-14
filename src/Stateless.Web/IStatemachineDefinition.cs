namespace Stateless.Web
{
    using System;

    public interface IStatemachineDefinition
    {
        string Name { get; }

        string InitialState { get; }

        TimeSpan? Ttl { get; }

        StateMachine CreateInstance(StateMachineContext instance, ITransitionDispatcher dispatcher);
    }
}
