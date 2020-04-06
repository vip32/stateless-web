namespace Stateless.Web
{
    using Microsoft.AspNetCore.Builder;

    public static class WorkflowMiddlewareExtensions
    {
        public static IApplicationBuilder UseStateless(
            this IApplicationBuilder builder,
            StateMachineMiddlewareOptions options = default)
        {
            return builder.UseMiddleware<StateMachineMiddleware>(options ?? new StateMachineMiddlewareOptions());
        }
    }
}
