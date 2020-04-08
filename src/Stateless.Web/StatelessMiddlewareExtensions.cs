namespace Stateless.Web
{
    using Microsoft.AspNetCore.Builder;

    public static class StatelessMiddlewareExtensions
    {
        public static IApplicationBuilder UseStateless(
            this IApplicationBuilder builder,
            StatelessMiddlewareOptions options = default)
        {
            return builder.UseMiddleware<StatelessMiddleware>(options ?? new StatelessMiddlewareOptions());
        }
    }
}
