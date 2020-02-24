using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheetahEventSource.Core;

namespace CheetahEventSource.InMemory
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly Dictionary<int, CheetahEvent> _events = new Dictionary<int, CheetahEvent>();

        public Task SaveAsync(CheetahAggregate entity)
        {
            // Todo: this will need to clear the uncommitted events somehow
            foreach (CheetahEvent e in entity._uncommittedEvents.Values)
            {
                _events.Add(e.Version, e);
            }

            return Task.CompletedTask;
        }
    }
}