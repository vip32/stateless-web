namespace Stateless.Web
{
    using System.IO;

    public interface IStateMachineContentStorage
    {
        Stream Load(StateMachineContext context, string key, Stream stream);

        void Save(StateMachineContext context, string key, Stream stream, string contentType = null);
    }
}
