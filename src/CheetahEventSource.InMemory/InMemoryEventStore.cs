using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheetahEventSource.Core;

namespace CheetahEventSource.InMemory
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly Dictionary<int, Event> _events = new Dictionary<int, Event>();

        public Task SaveAsync(CheetahAggregate entity)
        {
            foreach (Event e in entity.Events.Where(e => !e.Persisted))
            {
                _events.Add(e.Version, e);
                e.Persist();
            }

            return Task.CompletedTask;
        }
    }
}