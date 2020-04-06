namespace Stateless.Web
{
    using Microsoft.AspNetCore.Builder;

    public static class WorkflowMiddlewareExtensions
    {
        public static IApplicationBuilder UseStateless(
            this IApplicationBuilder builder,
            WorkflowMiddlewareOptions options = default)
        {
            return builder.UseMiddleware<WorkflowMiddleware>(options ?? new WorkflowMiddlewareOptions());
        }
    }
}
