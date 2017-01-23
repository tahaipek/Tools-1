namespace NetCoreStack.ComponentBinder.Tools
{
    public static class FileWatcherFactory
    {
        public static IFileSystemWatcher CreateWatcher(string watchedDirectory)
        {
            return new DotnetFileWatcher(watchedDirectory) as IFileSystemWatcher;
        }
    }
}