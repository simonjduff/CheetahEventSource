using System.Threading.Tasks;
using CheetahEventSource.Core;

namespace CheetahEventSource.InMemory
{
    public interface IEventStore
    {
        Task SaveAsync(CheetahAggregate entity);
    }
}