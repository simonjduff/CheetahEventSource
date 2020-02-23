namespace CheetahEventSource.Core
{
    public abstract class Event
    {
        private bool _persisted;

        public int Version { get; internal set; }

        public bool Persisted => _persisted;

        internal void Persist()
        {
            _persisted = true;
        }
    }
}