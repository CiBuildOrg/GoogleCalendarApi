using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mvc.Server.Contracts
{
    public interface IAsyncLock
    {
        IDisposable Lock();
        Task<IDisposable> LockAsync();
        Task<IDisposable> LockAsync(CancellationToken ct);
    }
}
