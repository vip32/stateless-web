namespace Stateless.Web
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public class WorkflowBuilder
    {
        public WorkflowBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        public IServiceCollection Services { get; }

        public WorkflowBuilder Register(string name, string initialState, Action<Workflow> configurationAction)
        {
            if (this.Services == null)
            {
                return this;
            }

            this.Services.RegisterWorkflow(name, initialState, configurationAction);
            return this;
        }
    }
}
