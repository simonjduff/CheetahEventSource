using System.Collections.Generic;
using System.Linq;

namespace CheetahEventSource.Core
{
    public abstract class CheetahAggregate
    {
        private SortedList<int, Event> _events = new SortedList<int, Event>();
        private readonly object _synlock = new object();

        public void Apply(Event e)
        {
            lock (_synlock)
            {
                e.Version = Version + 1;
                _events.Add(e.Version, e);
            }
        }
        public int Version => _events.Any() ? _events.Last().Value.Version : 0;
        public IEnumerable<Event> Events => _events.Values;
    }
}