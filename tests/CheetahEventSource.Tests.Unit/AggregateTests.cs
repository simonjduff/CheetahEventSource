using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheetahEventSource.Core;
using CheetahEventSource.Core.Attributes;
using CheetahEventSource.Core.Services;
using CheetahEventSource.InMemory;
using Newtonsoft.Json;
using Xunit;

namespace CheetahEventSource.Tests.Unit
{
    public class AggregateTests
    {
        private ICheetahAggregates<TestAggregate> _service;

        public AggregateTests()
        {
            _service = new CheetahAggregates<TestAggregate>(() => Guid.NewGuid().ToString(), 
                new AttributeService(this.GetType().Assembly));
        }
        
        [Fact]
        public async Task CreateAggregate()
        {
            // Given a created entity
            TestAggregate entity = await _service.Create();

            // And the version is set to 1
            Assert.Equal(1, entity.Version);

            // And the event is a created event
            Assert.Single(entity._uncommittedEvents);
            Assert.Equal("Created", entity._uncommittedEvents.Single().Value.EventType);
        }

        [Fact]
        public async Task Can_store_events()
        {
            // Given a created entity
            TestAggregate entity = await _service.Create();

            // When I store the entity
            IEventStore store = new InMemoryEventStore();
            
            // Then the uncommitted event is the create
            Assert.Single(entity._uncommittedEvents);
            Assert.Equal("Created", entity._uncommittedEvents.Single().Value.EventType);
            
            // When I save the entity
            await store.SaveAsync(entity);
            
            // And I apply another event
            entity.Change("Change");
            
            // Then the uncommitted event is the change
            Assert.Single(entity._uncommittedEvents);
            Assert.Equal("Created", entity._uncommittedEvents.Single().Value.EventType);
        }

        [Fact]
        public async Task Can_transition()
        {
            // Given a created entity
            TestAggregate entity = await _service.Create();

            // When I store the entity
            IEventStore store = new InMemoryEventStore();
            
            // Then the uncommitted event is the create
            Assert.Single(entity._uncommittedEvents);
            Assert.Equal("Created", entity._uncommittedEvents.Single().Value.EventType);
            
            // When I save the entity
            await store.SaveAsync(entity);
            
            // And I apply another event
            entity.Change("12345");
            
            // Then the uncommitted event is the change
            Assert.Single(entity._uncommittedEvents);
            Assert.Equal("Created", entity._uncommittedEvents.Single().Value.EventType);
            Assert.Equal("12345", entity.ChangeValue);
        }

        [Fact]
        public async Task Can_load_aggregate_from_events()
        {
            // Given events
            List<CheetahEvent> events = new List<CheetahEvent>();
            events.Add(new CheetahEvent
            {
                Id = "entity1",
                EventType = "Created",
                SchemaVersion = "1.0.0",
                Version = 1,
                Payload = "{\"id\":\"entity1\"}"
            });
            events.Add(new CheetahEvent
            {
                Id = "entity1",
                EventType = "TestChanged",
                SchemaVersion = "0.0.1",
                Version = 2,
                Payload = JsonConvert.SerializeObject(new TestAggregate.TestChanged{Value = "Loaded"})
            });
            
            // When I hydrate the entity
            TestAggregate aggregate = await _service.Hydrate(events);
            
            // Then the entity is loaded
            Assert.Equal("entity1", aggregate.Id);
            Assert.Equal("Loaded", aggregate.ChangeValue);
        }
    }

    public class TestAggregate : CheetahAggregate
    {
        public void Change(string value)
        {
            ApplyAsync(new TestChanged{Value = value});
        }

        [CheetahEvent("TestChanged", "0.0.1")]
        public class TestChanged
        {
            public string Value { get; set; }
        }

        protected override Task TransitionAsync(string eventName, string schemaVersion, object payload)
        {
            switch (eventName)
            {
                case "TestChanged":
                    Transition(payload as TestChanged);
                    return Task.CompletedTask;
            }
            
            throw new Exception($"Unknown event {eventName} {schemaVersion}");
        }

        private void Transition(TestChanged e)
        {
            ChangeValue = e.Value;
        }

        public string ChangeValue { get; set; }
    }

}
