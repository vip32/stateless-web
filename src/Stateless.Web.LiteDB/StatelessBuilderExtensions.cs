namespace Stateless.Web
{
    using Microsoft.Extensions.DependencyInjection;

    public static class StatelessBuilderExtensions
    {
        public static StatelessBuilder UseLiteDBStorage(this StatelessBuilder source, string connectionString = null)
        {
            if (source.Services == null)
            {
                return source;
            }

            source.Services.AddScoped<IStateMachineContextStorage>(sp => new LiteDBStateMachineContextStorage(connectionString));
            source.Services.AddScoped<IStateMachineContentStorage>(sp => new LiteDBStateMachineContentStorage(connectionString));

            return source;
        }
    }
}
