using System.Collections.Generic;
using UnityEngine;

namespace Arekntt.Core
{
    public class ObjectPool<T> where T : Component
    {
        private T prefab;
        private Queue<T> pool = new();

        public ObjectPool(T prefab, int initialSize = 10)
        {
            this.prefab = prefab;

            for (int i = 0; i < initialSize; i++)
            {
                var obj = GameObject.Instantiate(prefab);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            if (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                obj.gameObject.SetActive(true);
                return obj;
            }

            return GameObject.Instantiate(prefab);
        }

        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}
