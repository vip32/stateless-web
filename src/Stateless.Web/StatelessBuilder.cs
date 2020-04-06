namespace Stateless.Web
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public class StatelessBuilder
    {
        public StatelessBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        public IServiceCollection Services { get; }

        public StatelessBuilder AddStateMachine(string name, string initialState, Action<Workflow> configurationAction)
        {
            if (this.Services == null)
            {
                return this;
            }

            this.Services.RegisterStateMachine(name, initialState, configurationAction);
            return this;
        }
    }
}
