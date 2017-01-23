using System;

namespace NetCoreStack.ComponentBinder.Tools
{
    public interface IFileWatcher : IDisposable
    {
        event Action<string> OnFileChange;
        void WatchDirectory(string directory);
    }
}