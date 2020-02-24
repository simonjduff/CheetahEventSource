using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CheetahEventSource.Core.Attributes;
using CheetahEventSource.Core.Events;
using CheetahEventSource.Core.Services;
using Newtonsoft.Json;

namespace CheetahEventSource.Core
{
    public abstract class CheetahAggregate
    {
        internal readonly Dictionary<int, CheetahEvent> _uncommittedEvents = new Dictionary<int, CheetahEvent>();
        private int _version = 0;
        
        public string Id { get; private set; }
        internal IAttributeService AttributeService { get; set; }

        internal async Task LoadAsync(IEnumerable<CheetahEvent> events)
        {
            foreach (var e in events.OrderBy(e => e.Version))
            {
                if (e.EventType.Equals("Created"))
                {
                    await TransitionAsync(e.EventType, e.SchemaVersion,
                        JsonConvert.DeserializeObject<EntityCreated>(e.Payload ?? string.Empty));
                    continue;
                }

                Type type = AttributeService.FindEvent(e.EventType, e.SchemaVersion);
                object payload = JsonConvert.DeserializeObject(e.Payload, type);
                
                await TransitionAsync(e.EventType, e.SchemaVersion, payload);
                _version = e.Version;
            }
        }
        
        public int Version => _version;

        internal Task ApplyAsync(EntityCreated e)
        {
            var attribute = e.GetType().GetCustomAttribute(typeof(CheetahEventAttribute)) as CheetahEventAttribute;

            var cheetahEvent = new CheetahEvent
            {
                Id = e.Id,
                Version = 1,
                EventType = attribute.EventName,
                SchemaVersion = attribute.SchemaVersion
            };
            
            _uncommittedEvents.Add(cheetahEvent.Version, cheetahEvent);
            _version = cheetahEvent.Version;

            return TransitionAsync(attribute.EventName, attribute.SchemaVersion, e);
        }
        
        protected Task ApplyAsync(object e)
        {
            var attribute = e.GetType().GetCustomAttribute(typeof(CheetahEventAttribute)) as CheetahEventAttribute;

            // Todo: test this
            if (attribute == null)
            {
                // Todo: make a specific exception type
                throw new Exception("No attribute found for event");
            }
            
            var cheetahEvent = new CheetahEvent
            {
                Id = Id,
                Version = _version + 1,
                EventType = attribute.EventName,
                SchemaVersion = attribute.SchemaVersion,
                Payload = JsonConvert.SerializeObject(e)
            };

            _version = cheetahEvent.Version;

            return TransitionAsync(attribute.EventName, attribute.SchemaVersion, e);
        }

        // Todo: types for eventName, and schemaVersion
        protected abstract Task TransitionAsync(string eventName, string schemaVersion, object e);
        
        private Task TransitionAsync(string eventName, string schemaVersion, EntityCreated created)
        {
            Id = created.Id;
            return Task.CompletedTask;
        }

        internal void FlagEventSaved(CheetahEvent e)
        {
            _uncommittedEvents.Remove(e.Version);
        }
        
    }

    public class CheetahEvent
    {
        public string Id { get; set; }
        public int Version { get; set; }
        public string Payload { get; set; }
        public string EventType { get; set; }
        public string SchemaVersion { get; set; }
    }
}