using System.Collections.Generic;


namespace GameFramework
{
    public sealed class Context : GObject
    {
        public int tag;
        private List<IEntity> entities;
        private int current = 0;

        public int Count
        {
            get
            {
                return entities.Count;
            }
        }

        public Context(int tag)
        {
            this.tag = tag;
            entities = new List<IEntity>();
        }

        public void Dispose()
        {
            entities.Clear();
        }

        public IEntity NextEntity()
        {
            if (current > entities.Count - 1)
            {
                current = 0;
                return default;
            }
            return entities[current++];
        }

        public bool Contains(int tag)
        {
            if (tag <= 0)
            {
                return false;
            }
            return (tag & this.tag) != 0;
        }

        internal void Remove(IEntity entity)
        {
            entities.Remove(entity);
        }

        internal void Add(IEntity entity)
        {
            entities.Add(entity);
        }
    }
}
