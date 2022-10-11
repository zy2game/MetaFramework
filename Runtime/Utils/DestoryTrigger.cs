using UnityEngine;
namespace GameFramework
{
    public sealed class EntityContentGameObject : MonoBehaviour
    {
        public IEntity entity;
        private void OnDestroy()
        {
            if (entity == null)
            {
                return;
            }
            entity.Dispose();
        }
    }
}