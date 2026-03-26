using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 10;
    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab, this.transform);
            pool.Enqueue(obj);
            obj.SetActive(false);
        }
    }

    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            //Create new object if all pooled objects are in use
            GameObject obj = Instantiate(prefab, this.transform);
            return obj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        pool.Enqueue(obj);
        obj.SetActive(false);
    }
}
