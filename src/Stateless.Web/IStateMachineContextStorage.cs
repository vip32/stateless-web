namespace Stateless.Web
{
    using System.Collections.Generic;

    public interface IStateMachineContextStorage
    {
        IEnumerable<StateMachineContext> FindAll();

        StateMachineContext FindById(string id);

        void Save(StateMachineContext entity);
    }
}
