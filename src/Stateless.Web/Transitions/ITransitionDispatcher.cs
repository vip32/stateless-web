namespace Stateless.Web
{
    using System.Threading.Tasks;

    public interface ITransitionDispatcher
    {
        Task OnEntryAsync(StateMachine stateMachine);

        Task OnExitAsync(StateMachine stateMachine);
    }
}