using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace NetCoreStack.ComponentBinder.Tools
{
    internal class CommandLineOptions
    {
        public CommandLineApplication App { get; private set; }
        public bool IsHelp { get; private set; }
        public bool IsVerbose { get; private set; }
        public CommandArgument WatchPath { get; private set; }
        public CommandArgument DestinationPath { get; private set; }
        public IList<string> RemainingArguments { get; private set; }
        public static CommandLineOptions Parse(string[] args, TextWriter stdout, TextWriter stderr)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "dotnet cbind",
                FullName = "NetCoreStack Component Binder - Watcher",
                Out = stdout,
                Error = stderr,
                AllowArgumentSeparator = true,
                ExtendedHelpText = @"
Examples:
  dotnet cbind --watch <component.dll> --destination
"
            };

            app.HelpOption("-?|-h|--help");
            var watchPath = app.Argument("-w|--watch", "Watch sources to handle changes and copy to destination");
            var destinationPath = app.Argument("-d|--destination", "The destination path for changed files");
            var optVerbose = app.Option("-v|--verbose", "Show verbose output", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                if (app.RemainingArguments.Count == 0)
                {
                    app.ShowHelp();
                }

                return 0;
            });

            if (app.Execute(args) != 0)
            {
                return null;
            }

            return new CommandLineOptions
            {
                App = app,
                IsVerbose = optVerbose.HasValue(),
                RemainingArguments = app.RemainingArguments,
                IsHelp = app.IsShowingInformation,
                WatchPath = watchPath,
                DestinationPath = destinationPath
            };
        }
    }
}
