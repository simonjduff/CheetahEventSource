using System;
using System.Linq;
using System.Reflection;
using CheetahEventSource.Core.Attributes;

namespace CheetahEventSource.Core.Services
{
    public class AttributeService : IAttributeService
    {
        private readonly Assembly[] _assemblies;

        public AttributeService(params Assembly[] assemblies)
        {
            var copy = new Assembly[assemblies.Length + 1];
            Array.Copy(assemblies, copy, assemblies.Length);
            copy[assemblies.Length] = this.GetType().Assembly;
            _assemblies = copy;
        }
        
        public Type FindEvent(string eventName, string schemaVersion)
        {
            var matchingTypes = _assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t
                    .GetCustomAttributes<CheetahEventAttribute>()
                    .Count(a => 
                        a.EventName.Equals(eventName, StringComparison.OrdinalIgnoreCase)
                        && a.SchemaVersion.Equals(schemaVersion, StringComparison.OrdinalIgnoreCase)) == 1);

            if (!matchingTypes.Any())
            {
                throw new Exception($"No matching types for {eventName} {schemaVersion}");
            }
            
            if (matchingTypes.Count() > 1)
            {
                throw new Exception($"Multiple matching types for {eventName} {schemaVersion}");
            }

            return matchingTypes.Single();
        }
    }

    public interface IAttributeService
    {
        Type FindEvent(string eventName, string schemaVersion);
    }
}