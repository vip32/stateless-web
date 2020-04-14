namespace Stateless.Web
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public static class OnOffStateMachine
    {
        public const string Name = "onoff";

        public struct States
        {
            public const string On = "on";
            public const string Off = "off";
            public const string Broken = "broken";
        }

        public struct Triggers
        {
            public const string Switch = "switch";
            public const string Break = "break";
            public const string Repair = "repair";
        }

        public class BrokenStateTransitionHandler : IStateTransitionHandler
        {
            private readonly ILogger<EchoStateTransitionHandler> logger;

            public BrokenStateTransitionHandler(ILogger<EchoStateTransitionHandler> logger)
            {
                this.logger = logger;
            }

            public bool CanHandle(StateMachine stateMachine)
            {
                return stateMachine.Context.State.SafeEquals(States.Broken);
            }

            public Task OnEntryAsync(StateMachine stateMachine)
            {
                this.logger?.LogInformation($"{stateMachine.Context.Trigger} > PLEASE CALL A HANDYMAN");

                return Task.CompletedTask;
            }

            public Task OnExitAsync(StateMachine stateMachine)
            {
                if (stateMachine.Context.Trigger.SafeEquals(Triggers.Repair))
                {
                    this.logger?.LogInformation($"{stateMachine.Context.Trigger} > PRAISE TO THE HANDYMAN");
                }

                return Task.CompletedTask;
            }
        }
    }
}
