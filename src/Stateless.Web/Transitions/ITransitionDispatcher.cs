namespace Stateless.Web
{
    using System.Threading.Tasks;

    public interface ITransitionDispatcher
    {
        Task OnEntryAsync(Workflow workflow);

        Task OnExitAsync(Workflow workflow);
    }
}