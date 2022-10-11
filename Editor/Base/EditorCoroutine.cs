using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEditor
{
    public class EditorCoroutine
    {
        public int coustomId;
        private List<CorEntity> corList;
        private List<CorEntity> removeCorList;

        public EditorCoroutine()
        {
            corList = new List<CorEntity>();
            removeCorList = new List<CorEntity>();
        }

        public void Update()
        {
            if (corList.Count == 0) return;
            foreach (var cor in corList)
            {
                if (cor.IsDone())
                {
                    removeCorList.Add(cor);
                }
            }

            if (removeCorList.Count > 0)
            {
                foreach (var v in removeCorList)
                {
                    corList.Remove(v);
                }
                removeCorList.Clear();
            }
        }

        public int StartCor(IEnumerator enumerator)
        {
            coustomId++;
            CorEntity corEntity = new CorEntity(coustomId, enumerator);
            corList.Add(corEntity);
            return coustomId;
        }

        public void StopCor(int id)
        {
            CorEntity corEntity = null;
            foreach (var v in corList)
            {
                if (v.Id == id)
                {
                    corEntity = v;
                    break;
                }
            }
            if (corEntity == null) return;
            corEntity.Stop();
        }

        public void StopAll()
        {
            foreach (var v in corList)
            {
                v.Stop();
            }
        }


        private class CorEntity
        {
            public int Id { get; private set; }
            public IEnumerator enumerator;
            private CoustomYield wait;
            private bool stop = false;

            public CorEntity(int id, IEnumerator enumerator)
            {
                Id = id;
                this.enumerator = enumerator;
            }

            public bool IsDone()
            {
                if (stop) return true;
                if (wait == null)
                {
                    if (!enumerator.MoveNext()) return true;
                    wait = enumerator.Current as CoustomYield;
                }
                if (wait.IsDone())
                    wait = null;
                return false;
            }

            public void Stop()
            {
                stop = true;
            }
        }
    }

    public interface CoustomYield
    {
        bool IsDone();
    }

    public class Wait : CoustomYield
    {
        private float endTime;

        public Wait(float time)
        {
            endTime = Time.realtimeSinceStartup + time;
        }

        public bool IsDone()
        {
            return endTime<Time.realtimeSinceStartup;
        }
    }
}
