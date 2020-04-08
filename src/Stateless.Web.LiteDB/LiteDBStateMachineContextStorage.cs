namespace Stateless.Web
{
    using System.Collections.Generic;
    using LiteDB;

    public class LiteDBStateMachineContextStorage : IStateMachineContextStorage
    {
        private readonly string connectionString;

        public LiteDBStateMachineContextStorage(string connectionString = null)
        {
            this.connectionString = connectionString ?? "stateless.db";
        }

        public IEnumerable<StateMachineContext> FindAll()
        {
            using (var db = new LiteDatabase(this.connectionString))
            {
                return db.GetCollection<StateMachineContext>().Query().ToList();
            }
        }

        public StateMachineContext FindById(string id)
        {
            using (var db = new LiteDatabase(this.connectionString))
            {
                return db.GetCollection<StateMachineContext>().FindById(id);
            }
        }

        public void Save(StateMachineContext entity)
        {
            using (var db = new LiteDatabase(this.connectionString))
            {
                db.GetCollection<StateMachineContext>().Upsert(entity);
            }
        }
    }
}
