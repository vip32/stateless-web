namespace Stateless.Web
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public static class WorkflowBuilderExtensions
    {
        public static WorkflowBuilder UseLiteDBStorage(this WorkflowBuilder source, string connectionString = null)
        {
            if (source.Services == null)
            {
                return source;
            }

            source.Services.AddScoped<IWorkflowContextStorage>(sp => new LiteDBWorkflowContextStorage(connectionString));
            source.Services.AddScoped<IWorkflowContentStorage>(sp => new LiteDBWorkflowContentStorage(connectionString));

            return source;
        }
    }
}
