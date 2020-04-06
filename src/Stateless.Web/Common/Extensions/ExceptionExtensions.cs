namespace Stateless.Web
{
    using System;
    using System.Diagnostics;

    public static class ExceptionExtensions
    {
        [DebuggerStepThrough]
        public static string GetFullMessage(this Exception source)
        {
            if (source == null)
            {
                return null;
            }

            return source?.InnerException == null
                 ? $"[{source.GetType().Name}] {source?.Message}".Replace(Environment.NewLine, Environment.NewLine + " ")
                 : $"[{source.GetType().Name}] {source.Message}  --> {source.InnerException.GetFullMessage()}".Replace(Environment.NewLine, Environment.NewLine + " ");
        }
    }
}
