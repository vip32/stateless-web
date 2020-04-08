[![NuGet](https://img.shields.io/nuget/v/Stateless.Web.svg)](https://www.nuget.org/packages/Stateless.Web/)

# stateless-web
A simple web extension for the great [stateless](https://github.com/dotnet-state-machine/stateless) library.

## Usage

Startup.cs::ConfigureServices()
```
services.AddStateless(o =>
    {
        o.UseLiteDBStorage();
        o.AddStateMachine(
            OnOffStateMachine.Name,
            OnOffStateMachine.States.Off, c =>
            {
                c.Configure(OnOffStateMachine.States.Off)
                    .OnEntry((t) => Console.WriteLine($"the switch is turned {t.Destination}"))
                    .Permit(OnOffStateMachine.Triggers.Break, OnOffStateMachine.States.Broken)
                    .Permit(OnOffStateMachine.Triggers.Switch, OnOffStateMachine.States.On);
                c.Configure(OnOffStateMachine.States.On)
                    .OnEntry((t) => Console.WriteLine($"the switch is turned {t.Destination}"))
                    .Permit(OnOffStateMachine.Triggers.Break, OnOffStateMachine.States.Broken)
                    .Permit(OnOffStateMachine.Triggers.Switch, OnOffStateMachine.States.Off);
                c.Configure(OnOffStateMachine.States.Broken)
                    .OnEntry((t) => Console.WriteLine($"the switch is broken {t.Destination}"))
                    .Permit(OnOffStateMachine.Triggers.Repair, OnOffStateMachine.States.Off);
            });
    });
```

Startup.cs::Configure()
```
app.UseStateless();
```

#### State machine
```
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
}
```

#### Example requests
[here](REST.http)