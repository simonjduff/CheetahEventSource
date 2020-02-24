using System;
using System.Data;

namespace CheetahEventSource.Core.Attributes
{
    public class CheetahEventAttribute : Attribute
    {
        public CheetahEventAttribute(string eventName, string schemaVersion)
        {
            EventName = eventName;
            SchemaVersion = schemaVersion;
        }
        
        public string EventName { get; }
        public string SchemaVersion { get; }
    }
}