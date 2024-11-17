using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoolSystem
{
    public class Pool<T> where T : MonoBehaviour, IPoolable
    {
        private List<T> members = new List<T>();
        private HashSet<T> unavailable = new HashSet<T>();
        private IFactory<T> factory;

        public IReadOnlyCollection<T> ReleasedItems => unavailable;

        private Pool(IFactory<T> factory) : this(factory, 5) { }

        public Pool(IFactory<T> factory, int poolSize)
        {
            this.factory = factory;

            for (int i = 0; i < poolSize; i++)
            {
                Create();
            }
        }

        public int GetReleasedCount() => unavailable.Count;

        public T GetItem()
        {
            T allocatedObj = Allocate();
            return allocatedObj;
        }
        public T GetItem(Vector3 position = default, Quaternion rotation = default)
        {
            T allocatedObj = Allocate();
            allocatedObj.transform.position = position;
            allocatedObj.transform.rotation = rotation;
            allocatedObj.gameObject.SetActive(true);
            return allocatedObj;
        }

        public void Recycle(T item)
        {
            item.Reset();
            item.gameObject.SetActive(false);
            Release(item);
        }

        private T Allocate()
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (!unavailable.Contains(members[i]))
                {
                    unavailable.Add(members[i]);
                    return members[i];
                }
            }
            T newMember = Create();
            unavailable.Add(newMember);
            return newMember;
        }

        private void Release(T member)
        {
            unavailable.Remove(member);
        }

        private T Create()
        {
            T member = factory.Create();
            members.Add(member);
            return member;
        }
    }

    public class PrefabFactory<T> : IFactory<T> where T : MonoBehaviour
    {
        GameObject prefab;
        string name;
        int index = 0;

        public PrefabFactory(GameObject prefab) : this(prefab, prefab.name) { }

        public PrefabFactory(GameObject prefab, string name)
        {
            this.prefab = prefab;
            this.name = name;
        }

        public T Create()
        {
            GameObject tempGameObject = GameObject.Instantiate(prefab) as GameObject;
            tempGameObject.name = name + index.ToString();
            T objectOfType = tempGameObject.GetComponent<T>();
            tempGameObject.SetActive(false);
            index++;
            return objectOfType;
        }
    }

    public interface IFactory<T>
    {
        T Create();
    }

    public interface IPoolable
    {
        void Reset();
    }
}