using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheetahEventSource.Core.Events;

namespace CheetahEventSource.Core.Services
{
    public class CheetahAggregates<T> : ICheetahAggregates<T>
    where T : CheetahAggregate, new()
    {
        private readonly Func<string> _idGenerationStrategy;
        private readonly IAttributeService _attributeService;

        public CheetahAggregates(Func<string> idGenerationStrategy,
            IAttributeService attributeService)
        {
            _attributeService = attributeService;
            _idGenerationStrategy = idGenerationStrategy;
        }
        
        public Task<T> Create()
        {
            T entity = new T();
            entity.AttributeService = _attributeService;
            entity.ApplyAsync(new EntityCreated(_idGenerationStrategy()));
            return Task.FromResult(entity);
        }

        public async Task<T> Hydrate(IEnumerable<CheetahEvent> events)
        {
            var entity = new T();
            entity.AttributeService = _attributeService;
            await entity.LoadAsync(events);
            return entity;
        }
    }

    public interface ICheetahAggregates<T>
    where T : CheetahAggregate, new()
    {
        Task<T> Create();
        Task<T> Hydrate(IEnumerable<CheetahEvent> events);
    }
}