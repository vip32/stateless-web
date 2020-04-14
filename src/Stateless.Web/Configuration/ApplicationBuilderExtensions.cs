namespace Stateless.Web
{
    using Microsoft.AspNetCore.Builder;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseStateless(
            this IApplicationBuilder builder,
            StatelessMiddlewareOptions options = default)
        {
            return builder.UseMiddleware<StatelessMiddleware>(options ?? new StatelessMiddlewareOptions());
        }
    }
}
