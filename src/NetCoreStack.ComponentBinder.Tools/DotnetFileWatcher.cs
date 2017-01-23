﻿using System;
using System.IO;

namespace NetCoreStack.ComponentBinder.Tools
{
    internal class DotnetFileWatcher : IFileSystemWatcher
    {
        private readonly Func<string, FileSystemWatcher> _watcherFactory;
        private readonly string _watchedDirectory;

        private FileSystemWatcher _fileSystemWatcher;

        private readonly object _createLock = new object();

        public DotnetFileWatcher(string watchedDirectory)
            : this(watchedDirectory, DefaultWatcherFactory)
        {
        }

        internal DotnetFileWatcher(string watchedDirectory, Func<string, FileSystemWatcher> fileSystemWatcherFactory)
        {
            if (string.IsNullOrEmpty(watchedDirectory))
            {
                throw new ArgumentNullException(nameof(watchedDirectory));
            }

            if (fileSystemWatcherFactory == null)
            {
                throw new ArgumentNullException(nameof(fileSystemWatcherFactory));
            }

            _watchedDirectory = watchedDirectory;
            _watcherFactory = fileSystemWatcherFactory;
            CreateFileSystemWatcher();
        }

        public event EventHandler<string> OnFileChange;

        public event EventHandler OnError;

        private static FileSystemWatcher DefaultWatcherFactory(string watchedDirectory)
        {
            if (string.IsNullOrEmpty(watchedDirectory))
            {
                throw new ArgumentNullException(nameof(watchedDirectory));
            }

            return new FileSystemWatcher(watchedDirectory);
        }

        private void WatcherErrorHandler(object sender, ErrorEventArgs e)
        {
            CreateFileSystemWatcher();

            OnError?.Invoke(this, null);
        }

        private void WatcherRenameHandler(object sender, RenamedEventArgs e)
        {
            NotifyChange(e.OldFullPath);
            NotifyChange(e.FullPath);

            if (Directory.Exists(e.FullPath))
            {
                foreach (var newLocation in Directory.EnumerateFileSystemEntries(e.FullPath, "*", SearchOption.AllDirectories))
                {
                    // Calculated previous path of this moved item.
                    var oldLocation = Path.Combine(e.OldFullPath, newLocation.Substring(e.FullPath.Length + 1));
                    NotifyChange(oldLocation);
                    NotifyChange(newLocation);
                }
            }
        }

        private void WatcherChangeHandler(object sender, FileSystemEventArgs e)
        {
            NotifyChange(e.FullPath);
        }

        private void NotifyChange(string fullPath)
        {
            // Only report file changes
            OnFileChange?.Invoke(this, fullPath);
        }

        private void CreateFileSystemWatcher()
        {
            lock (_createLock)
            {
                bool enableEvents = false;

                if (_fileSystemWatcher != null)
                {
                    enableEvents = _fileSystemWatcher.EnableRaisingEvents;

                    _fileSystemWatcher.EnableRaisingEvents = false;

                    _fileSystemWatcher.Created -= WatcherChangeHandler;
                    _fileSystemWatcher.Deleted -= WatcherChangeHandler;
                    _fileSystemWatcher.Changed -= WatcherChangeHandler;
                    _fileSystemWatcher.Renamed -= WatcherRenameHandler;
                    _fileSystemWatcher.Error -= WatcherErrorHandler;

                    _fileSystemWatcher.Dispose();
                }

                _fileSystemWatcher = _watcherFactory(_watchedDirectory);
                _fileSystemWatcher.IncludeSubdirectories = true;

                _fileSystemWatcher.Created += WatcherChangeHandler;
                _fileSystemWatcher.Deleted += WatcherChangeHandler;
                _fileSystemWatcher.Changed += WatcherChangeHandler;
                _fileSystemWatcher.Renamed += WatcherRenameHandler;
                _fileSystemWatcher.Error += WatcherErrorHandler;

                _fileSystemWatcher.EnableRaisingEvents = enableEvents;
            }
        }

        public bool EnableRaisingEvents
        {
            get { return _fileSystemWatcher.EnableRaisingEvents; }
            set { _fileSystemWatcher.EnableRaisingEvents = value; }
        }

        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }
    }
}
