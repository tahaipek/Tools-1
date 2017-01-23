using System;

namespace NetCoreStack.ComponentBinder.Tools
{
    public interface IFileSystemWatcher : IDisposable
    {
        event EventHandler<string> OnFileChange;

        event EventHandler OnError;

        bool EnableRaisingEvents { get; set; }
    }
}