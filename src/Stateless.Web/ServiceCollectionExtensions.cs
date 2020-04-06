namespace Stateless.Web
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWorkflow(
            this IServiceCollection services,
            Action<WorkflowBuilder> optionsAction = null)
        {
            if (services == null)
            {
                return services;
            }

            services.AddScoped<ITransitionDispatcher, TransitionDispatcher>();
            AddWorkflowHandlers(services);

            var options = new WorkflowBuilder(services);
            optionsAction?.Invoke(options);

            return services;
        }

        /// <summary>
        /// Add a workflow registration
        /// </summary>
        /// <param name="services">The services</param>
        internal static IServiceCollection RegisterWorkflow(
            this IServiceCollection services,
            string name,
            string initialState,
            Action<Workflow> configuration)
        {
            if (services == null)
            {
                return services;
            }

            return services.AddScoped<IWorkflowDefinition>(sp =>
            {
                return new WorkflowDefinition(
                        name,
                        initialState,
                        configuration);
            });
        }

        private static void AddWorkflowHandlers(IServiceCollection services)
        {
            services
                .Scan(scan => scan
                    .FromApplicationDependencies(a => !a.FullName.StartsWithAny(new[] { "Microsoft", "System", "Scrutor" }))
                    .AddClasses(classes => classes.AssignableTo(typeof(IStateTransitionHandler)), true)
                    .AsImplementedInterfaces().WithScopedLifetime());
        }
    }
}
