namespace Stateless.Web
{
    using System;
    using System.Collections.Generic;

    public class WorkflowContext
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Name { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public string State { get; set; }

        public IDictionary<string, WorkflowContent> Content { get; set; } = new Dictionary<string, WorkflowContent>();
    }
}
