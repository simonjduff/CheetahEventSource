using CheetahEventSource.Core.Attributes;

namespace CheetahEventSource.Core.Events
{
    [CheetahEvent("Created", "1.0.0")]
    public class EntityCreated
    {
        public EntityCreated()
        {
        }

        public EntityCreated(string id)
        {
            Id = id;
        }
        
        public string Id { get; set; }
    }
}