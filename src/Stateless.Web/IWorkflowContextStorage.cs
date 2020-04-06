namespace Stateless.Web
{
    using System.Collections.Generic;

    public interface IWorkflowContextStorage
    {
        IEnumerable<WorkflowContext> FindAll();

        WorkflowContext FindById(string id);

        void Save(WorkflowContext entity);
    }
}
