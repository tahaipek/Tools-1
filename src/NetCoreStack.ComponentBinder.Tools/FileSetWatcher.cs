using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.ComponentBinder.Tools
{
    public class FileSetWatcher : IDisposable
    {
        private readonly IFileWatcher _fileWatcher;
        private readonly List<string> _fileSet;

        public FileSetWatcher(List<string> fileSet)
        {
            _fileSet = fileSet;
            _fileWatcher = new FileWatcher();
        }

        public async Task<string> GetChangedFileAsync(CancellationToken cancellationToken)
        {
            foreach (var file in _fileSet)
            {
                _fileWatcher.WatchDirectory(Path.GetDirectoryName(file));
            }

            var tcs = new TaskCompletionSource<string>();
            cancellationToken.Register(() => tcs.TrySetResult(null));

            Action<string> callback = path =>
            {
                if (_fileSet.Contains(path))
                {
                    tcs.TrySetResult(path);
                }
            };

            _fileWatcher.OnFileChange += callback;
            var changedFile = await tcs.Task;
            _fileWatcher.OnFileChange -= callback;

            return changedFile;
        }

        public void Dispose()
        {
            _fileWatcher.Dispose();
        }
    }
}