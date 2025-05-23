using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum ObjectType
{
    Dongle,
    LevelUpEffect
}

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    [System.Serializable]
    public struct PoolInfo
    {
        public ObjectType type;
        public GameObject prefab;
        public Transform group;
        public int defaultCapacity;
        public int maxSize;
    }

    [SerializeField] private PoolInfo[] pools;

    private Dictionary<ObjectType, ObjectPool<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        poolDictionary = new Dictionary<ObjectType, ObjectPool<GameObject>>();

        foreach (var pool in pools)
        {
            ObjectType type = pool.type;
            GameObject prefab = pool.prefab;
            Transform group = pool.group;

            ObjectPool<GameObject> objectPool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject obj = Instantiate(prefab, group);
                    obj.SetActive(false);
                    return obj;
                },
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: obj =>
                {
                    if (Application.isPlaying)
                    {
                        Destroy(obj);
                    }
                    else
                    {
                        DestroyImmediate(obj);
                    }
                },
                collectionCheck: false,
                defaultCapacity: pool.defaultCapacity,
                maxSize: pool.maxSize
            );

            poolDictionary[type] = objectPool;
        }
    }

    public GameObject Get(ObjectType type)
    {
        return poolDictionary[type].Get();
    }

    public void Release(ObjectType type, GameObject obj)
    {
        poolDictionary[type].Release(obj);
    }
}
