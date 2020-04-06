namespace Stateless.Web
{
    using System;
    using System.IO;
    using LiteDB;

    public class LiteDBWorkflowContentStorage : IStateMachineContentStorage
    {
        // https://github.com/mbdavid/LiteDB/wiki/FileStorage
        private readonly string connectionString;

        public LiteDBWorkflowContentStorage(string connectionString = null)
        {
            this.connectionString = connectionString ?? "workflow.db";
        }

        public Stream Load(StateMachineContext context, string key, Stream stream)
        {
            using (var db = new LiteDatabase(this.connectionString))
            {
                if (db.FileStorage.Exists($"{context.Id}/{key}"))
                {
                    var fileInfo = db.FileStorage.Download($"{context.Id}/{key}", stream);
                }

                return stream;
            }
        }

        public void Save(StateMachineContext context, string key, Stream stream, string contentType)
        {
            using (var db = new LiteDatabase(this.connectionString))
            {
                if (db.FileStorage.Exists($"{context.Id}/{key}"))
                {
                    db.FileStorage.Delete($"{context.Id}/{key}");
                }

                db.FileStorage.Upload($"{context.Id}/{key}", key, stream);

                context.Content.Add(key, new StateMachineContent
                {
                    ContentType = contentType,
                    Size = stream.Length,
                    Created = DateTime.UtcNow
                });
            }
        }
    }
}
