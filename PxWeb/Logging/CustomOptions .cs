using Microsoft.Extensions.Logging.Console;

namespace PxWeb.Logging
{
    public sealed class CustomOptions : ConsoleFormatterOptions
    {
        public string? CustomPrefix { get; set; }
    }
}
