namespace Stateless.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class StateMachineContext
    {
        private List<StateMachineContent> content = new List<StateMachineContent>();

        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Name { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public string State { get; set; }

        public string Trigger { get; set; }

        public TimeSpan? Ttl { get; set; }

        public IEnumerable<StateMachineContent> Content
        {
            get { return this.content; }
            set { this.content = value?.ToList(); }
        }

        public DataDictionary Properties { get; set; } = new DataDictionary();

        public bool IsExpired()
        {
            if(!this.Ttl.HasValue)
            {
                return false;
            }

            return this.Created.AddMilliseconds(this.Ttl.Value.TotalMilliseconds) <= DateTime.Now;
        }

        public void AddContent(string key, string contentType, long size)
        {
            var now = DateTime.UtcNow;
            var content = this.content.Find(c => c.Key.SafeEquals(key));

            if (content != null)
            {
                content.ContentType = contentType;
                content.Size = size;
                content.Updated = now;
            }
            else
            {
                this.content.Add(new StateMachineContent
                {
                    Key = key,
                    ContentType = contentType,
                    Size = size,
                    Created = now,
                    Updated = now
                });
            }
        }
    }
}
