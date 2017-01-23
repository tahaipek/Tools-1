using System.Collections.Generic;

namespace NetCoreStack.ComponentBinder.Tools
{
    public interface IFileSet : IEnumerable<string>
    {
        bool Contains(string filePath);
    }
}