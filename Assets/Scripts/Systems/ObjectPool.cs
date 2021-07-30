using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private T _template;

    private List<T> _inactiveObjects;
    private HashSet<T> _activeObjects;

    private int _addToPoolCount;
     
    public ObjectPool(T template, int baseCount, int addToPoolCount)
    {
        _inactiveObjects = new List<T>();
        _activeObjects = new HashSet<T>();

        _template = template;
        _template.gameObject.SetActive(false);

        _addToPoolCount = addToPoolCount;

        for (int i = 0; i < baseCount; ++i)
            GenerateNewT();
    }

    public T Retrieve()
    {
        if (_inactiveObjects.Count == 0)
        {
            for (int i = 0; i < _addToPoolCount; ++i)
                GenerateNewT();
        }
        T returnT = _inactiveObjects[_inactiveObjects.Count - 1];
        returnT.gameObject.SetActive(true);
    
        _inactiveObjects.RemoveAt(_inactiveObjects.Count - 1);
      
        _activeObjects.Add(returnT);
        return returnT;
    }

    public void Return(T tObject)
    {
        if (_activeObjects.Contains(tObject))
        {
            _activeObjects.Remove(tObject);
            tObject.gameObject.SetActive(false);
            _inactiveObjects.Add(tObject);
        }
        else
            throw new ArgumentException("Object attempted return to ObjectPool that is not owned by pool.");
    }

    private void GenerateNewT()
    {
        var newT = GameObject.Instantiate(_template).GetComponent<T>();
        _inactiveObjects.Add(newT);
        newT.gameObject.SetActive(false); 
    }
}
