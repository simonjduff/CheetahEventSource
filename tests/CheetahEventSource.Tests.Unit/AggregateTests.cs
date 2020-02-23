using System.Linq;
using System.Threading.Tasks;
using CheetahEventSource.Core;
using CheetahEventSource.InMemory;
using Xunit;

namespace CheetahEventSource.Tests.Unit
{
    public class AggregateTests
    {
        [Fact]
        public void CreateAggregate()
        {
            // When I create a new entity
            CheetahAggregate entity = new TestAggregate();

            // Then there is a Created event on the log
            Assert.Single(entity.Events);

            // And the version is set to 1
            Assert.Equal(1, entity.Version);

            // And the event is a created event
            Assert.Equal(typeof(TestAggregate.TestCreated), entity.Events.Single().GetType());
        }

        [Fact]
        public async Task Can_store_events()
        {
            // Given a created entity
            TestAggregate entity = new TestAggregate();

            // When I store the entity
            IEventStore store = new InMemoryEventStore();
            await store.SaveAsync(entity);

            // And I apply another event
            entity.Change();

            // Then the create event is persisted
            Assert.True(entity.Events.First().Persisted);

            // And the second event is not
            Assert.False(entity.Events.Skip(1).First().Persisted);
        }

        [Fact]
        public void Can_load_aggregate_from_events()
        {
            
        }
}

    public class TestAggregate : CheetahAggregate
    {
        public TestAggregate()
        {
            Apply(new TestCreated());
        }

        private void Apply(TestCreated created)
        {
            
            base.Apply(created);
        }

        public void Change()
        {
            Apply(new TestChanged());
        }

        public class TestCreated : Event
        {
        }

        public class TestChanged : Event
        {
        }
    }

}
