using Microsoft.Extensions.Logging;

namespace NetCoreStack.ComponentBinder.Tools
{
    public class CommandOutputProvider : ILoggerProvider
    {
        public CommandOutputProvider()
        {
        }

        public ILogger CreateLogger(string name)
        {
            return new CommandOutputLogger(this, name);
        }

        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        public void Dispose()
        {
        }
    }
}