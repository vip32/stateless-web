namespace Stateless.Web
{
    using System.Threading.Tasks;

    public interface IStateTransitionHandler
    {
        bool CanHandle(Workflow workflow);

        Task OnEntryAsync(Workflow workflow);

        Task OnExitAsync(Workflow workflow);
    }
}