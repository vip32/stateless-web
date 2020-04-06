namespace Stateless.Web
{
    using System.IO;

    public interface IWorkflowContentStorage
    {
        Stream Load(WorkflowContext context, string key, Stream stream);

        void Save(WorkflowContext context, string key, Stream stream, string contentType = null);
    }
}
