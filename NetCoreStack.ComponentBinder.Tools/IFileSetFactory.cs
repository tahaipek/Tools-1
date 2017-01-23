using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.ComponentBinder.Tools
{
    public interface IFileSetFactory
    {
        Task<IFileSet> CreateAsync(CancellationToken cancellationToken);
    }
}