namespace Stateless.Web
{
    using static Stateless.StateMachine<string, string>;

    public static class StatelessExtensions
    {
        public static StateConfiguration Configure(this StateMachine source, string state)
        {
            return source.Configure(state)
                .OnEntryAsync(async t =>
                {
                    source.Context.State = t.Destination;
                    await source.Dispatcher.OnEntryAsync(source).ConfigureAwait(false);
                })
                .OnExitAsync(async _ => await source.Dispatcher.OnExitAsync(source).ConfigureAwait(false));
        }
    }
}
