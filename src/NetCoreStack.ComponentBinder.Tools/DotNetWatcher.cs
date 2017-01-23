using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace NetCoreStack.ComponentBinder.Tools
{
    public class DotNetWatcher
    {
        private readonly ILogger _logger;

        public DotNetWatcher(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;
        }

        public async Task WatchAsync(List<string> fileSet, CancellationToken cancellationToken)
        {
            var cancelledTaskSource = new TaskCompletionSource<object>();
            cancellationToken.Register(state => ((TaskCompletionSource<object>)state).TrySetResult(null), cancelledTaskSource);

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                using (var currentRunCancellationSource = new CancellationTokenSource())
                using (var combinedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    currentRunCancellationSource.Token))
                using (var fileSetWatcher = new FileSetWatcher(fileSet))
                {
                    var fileSetTask = fileSetWatcher.GetChangedFileAsync(combinedCancellationSource.Token);
                    var finishedTask = await Task.WhenAny(fileSetTask, cancelledTaskSource.Task);

                    // Regardless of the which task finished first, make sure everything is cancelled
                    // and wait for dotnet to exit. We don't want orphan processes
                    currentRunCancellationSource.Cancel();

                    await Task.WhenAll(fileSetTask);

                    if (finishedTask == cancelledTaskSource.Task || cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (!string.IsNullOrEmpty(fileSetTask.Result))
                    {
                        _logger.LogInformation($"File changed: {fileSetTask.Result}");
                    }
                }
            }
        }
    }
}
