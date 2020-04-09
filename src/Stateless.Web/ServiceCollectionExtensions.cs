namespace Stateless.Web
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStateless(
            this IServiceCollection services,
            Action<StatelessBuilder> builderAction = null)
        {
            if (services == null)
            {
                return services;
            }

            services.AddScoped<ITransitionDispatcher, TransitionDispatcher>();
            AddStatemachineHandlers(services);

            var builder = new StatelessBuilder(services);
            builderAction?.Invoke(builder);

            return services;
        }

        /// <summary>
        /// Add a state machine registration
        /// </summary>
        /// <param name="services">The services</param>
        internal static IServiceCollection RegisterStateMachine(
            this IServiceCollection services,
            string name,
            string initialState,
            Action<StateMachine> configuration,
            TimeSpan? ttl)
        {
            if (services == null)
            {
                return services;
            }

            return services.AddScoped<IStatemachineDefinition>(sp =>
            {
                return new StatemachineDefinition(
                        name,
                        initialState,
                        configuration,
                        ttl);
            });
        }

        private static void AddStatemachineHandlers(IServiceCollection services)
        {
            services
                .Scan(scan => scan
                    .FromApplicationDependencies(a => !a.FullName.StartsWithAny(new[] { "Microsoft", "System", "Scrutor" }))
                    .AddClasses(classes => classes.AssignableTo(typeof(IStateTransitionHandler)), true)
                    .AsImplementedInterfaces().WithScopedLifetime());
        }
    }
}
