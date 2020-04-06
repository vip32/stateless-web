namespace Stateless.Web
{
    using Microsoft.Extensions.DependencyInjection;

    public static class WorkflowBuilderExtensions
    {
        public static StatelessBuilder UseLiteDBStorage(this StatelessBuilder source, string connectionString = null)
        {
            if (source.Services == null)
            {
                return source;
            }

            source.Services.AddScoped<IStateMachineContextStorage>(sp => new LiteDBWorkflowContextStorage(connectionString));
            source.Services.AddScoped<IStateMachineContentStorage>(sp => new LiteDBWorkflowContentStorage(connectionString));

            return source;
        }
    }
}
