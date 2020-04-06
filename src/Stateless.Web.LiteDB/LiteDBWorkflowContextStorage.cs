namespace Stateless.Web
{
    using System.Collections.Generic;
    using LiteDB;

    public class LiteDBWorkflowContextStorage : IWorkflowContextStorage
    {
        private readonly string connectionString;

        public LiteDBWorkflowContextStorage(string connectionString = null)
        {
            this.connectionString = connectionString ?? "workflow.db";
        }

        public IEnumerable<WorkflowContext> FindAll()
        {
            using (var db = new LiteDatabase(this.connectionString))
            {
                return db.GetCollection<WorkflowContext>().Query().ToList();
            }
        }

        public WorkflowContext FindById(string id)
        {
            using (var db = new LiteDatabase(this.connectionString))
            {
                return db.GetCollection<WorkflowContext>().FindById(id);
            }
        }

        public void Save(WorkflowContext entity)
        {
            using (var db = new LiteDatabase(this.connectionString))
            {
                db.GetCollection<WorkflowContext>().Upsert(entity);
            }
        }
    }
}
