using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.ComponentBinder.Tools
{
    public class Program
    {
        private const string Prefix = "NetCoreStack";
        private static readonly string LoggerName = $"{Prefix}DotNetWatcher";
        private readonly CancellationToken _cancellationToken;
        private readonly TextWriter _stdout;
        private readonly TextWriter _stderr;

        public Program(TextWriter consoleOutput, TextWriter consoleError, CancellationToken cancellationToken)
        {
            if (consoleOutput == null)
            {
                throw new ArgumentNullException(nameof(consoleOutput));
            }

            if (consoleError == null)
            {
                throw new ArgumentNullException(nameof(consoleError));
            }

            _cancellationToken = cancellationToken;
            _stdout = consoleOutput;
            _stderr = consoleError;
        }

        public static int Main(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            using (CancellationTokenSource ctrlCTokenSource = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (sender, ev) =>
                {
                    if (!ctrlCTokenSource.IsCancellationRequested)
                    {
                        Console.WriteLine($"[{LoggerName}] Shutdown requested. Press CTRL+C again to force exit.");
                        ev.Cancel = true;
                    }
                    else
                    {
                        ev.Cancel = false;
                    }
                    ctrlCTokenSource.Cancel();
                };

                try
                {
                    return new Program(Console.Out, Console.Error, ctrlCTokenSource.Token)
                        .MainInternalAsync(args)
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException || ex is OperationCanceledException)
                    {
                        // swallow when only exception is the CTRL+C forced an exit
                        return 0;
                    }

                    Console.Error.WriteLine(ex.ToString());
                    Console.Error.WriteLine($"[{LoggerName}] An unexpected error occurred".Bold().Red());
                    return 1;
                }
            }
        }

        private async Task<int> MainInternalAsync(string[] args)
        {
            var options = CommandLineOptions.Parse(args, _stdout, _stderr);

            var loggerFactory = new LoggerFactory();
            var commandProvider = new CommandOutputProvider
            {
                LogLevel = ResolveLogLevel(options)
            };
            loggerFactory.AddProvider(commandProvider);
            var logger = loggerFactory.CreateLogger(LoggerName);

            if (string.IsNullOrEmpty(options.WatchPath.Value))
            {
                // invalid args syntax
                return 1;
            }

            var projectFile = Path.Combine(Directory.GetCurrentDirectory(), Project.FileName);
            await new DotNetWatcher(logger).WatchAsync(new List<string> { options.WatchPath.Value }, _cancellationToken);
            return 0;
        }

        private LogLevel ResolveLogLevel(CommandLineOptions options)
        {
            bool globalVerbose;
            bool.TryParse(Environment.GetEnvironmentVariable(CommandContext.Variables.Verbose), out globalVerbose);

            if (options.IsVerbose || globalVerbose)
            {
                return LogLevel.Debug;
            }

            return LogLevel.Information;
        }
    }
}