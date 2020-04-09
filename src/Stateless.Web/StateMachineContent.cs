namespace Stateless.Web
{
    using System;

    public class StateMachineContent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Key { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
    }
}
