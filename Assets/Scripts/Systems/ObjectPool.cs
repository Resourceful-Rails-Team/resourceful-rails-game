/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Systems
{
    /// <summary>
    /// A pool of objects that can be retrieved and returned, automatically
    /// enabling and disabling the objects.
    /// </summary>
    /// <typeparam name="T">Any MonoBehaviour</typeparam>
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private T _template;

        private List<T> _inactiveObjects;
        private HashSet<T> _activeObjects;

        private int _addToPoolCount;

        /// <summary>
        /// Constructs the ObjectPool with the given template and base info.
        /// </summary>
        /// <param name="template">The template for the pool</param>
        /// <param name="baseCount">The number of objects to generate upon construction</param>
        /// <param name="addToPoolCount">The number of objects to generate when the pool is empty</param>
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

        /// <summary>
        /// Retrieves a single object from the pool
        /// </summary>
        /// <returns>The object requested</returns>
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

        /// <summary>
        /// Returns an object to the pool. Raises exception if the object does not
        /// belong to the pool.
        /// </summary>
        /// <param name="tObject">The object to deactivate / return.</param>
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

        // Generates a new T object
        private void GenerateNewT()
        {
            var newT = GameObject.Instantiate(_template).GetComponent<T>();
            _inactiveObjects.Add(newT);
            newT.gameObject.SetActive(false);
        }
    }
}
