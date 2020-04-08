namespace Stateless.Web
{
    using System.Threading.Tasks;

    public interface IStateTransitionHandler
    {
        bool CanHandle(StateMachine stateMachine);

        Task OnEntryAsync(StateMachine stateMachine);

        Task OnExitAsync(StateMachine stateMachine);
    }
}