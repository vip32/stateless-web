namespace Stateless.Web
{
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
}
