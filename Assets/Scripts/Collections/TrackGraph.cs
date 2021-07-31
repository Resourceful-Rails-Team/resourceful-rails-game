using Rails.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rails.Collections
{
    /// <summary>
    /// An undirected graph representing tracks on the Map.
    /// </summary>
    /// <typeparam name="T">The type designated for edge values (Cardinal direction)</typeparam>
    public class TrackGraph<T>
    {
        private Dictionary<NodeId, T[]> _adjacencyList;
        private Func<T[]> _defaultEdgesFactory;

        public TrackGraph()
            => _adjacencyList = new Dictionary<NodeId, T[]>();        

        public TrackGraph(Func<T[]> defaultEdgesFactory)
        {
            _adjacencyList = new Dictionary<NodeId, T[]>();
            _defaultEdgesFactory = defaultEdgesFactory;
        }

        /// <summary>
        /// Inserts a given `T` edge value at the `NodeId` and `Cardinal`.
        /// 
        ///     get: Returns the value at the given location, or throws an `ArgumentException`, if there
        ///          is nothing at the given `NodeId`.
        ///      
        ///     set: Sets the position and direction value to the one given. Automatically
        ///          inserts a new `NodeId` key with the given cloneEdgeFunction, or `T`s
        ///          default values, if there is no `NodeId` vertex present.
        ///          
        /// </summary>
        /// <param name="id">The `NodeId` to write/read the given `T` value</param>
        /// <param name="card">The `Cardinal` to write/read the given `T` value</param>
        /// <returns>The given `T` value at the position and direction</returns>
        public T this[NodeId id, Cardinal card]
        {
            get
            {
                if (!_adjacencyList.ContainsKey(id))
                    throw new ArgumentException($"TrackGraph does not contain Vertex {id}");

                return _adjacencyList[id][(int)card];
            }
            set
            {
                InsertAt(id, card, value);

                var reflectId = Utilities.PointTowards(id, card);
                var reflectCard = Utilities.ReflectCardinal(card);

                InsertAt(reflectId, reflectCard, value);
            }
        }

        /// <summary>
        /// Inserts a given `T` edge value at the two adjacent `NodeId`s.
        /// 
        ///     get: Returns the value at the given location, or throws an `ArgumentException`, if there
        ///          is nothing at either `NodeId`, or if the `NodeId`s aren't adjacent.
        ///      
        ///     set: Sets the position and direction value to the one given. Automatically
        ///          inserts two new `NodeId` key with the given cloneEdgeFunction, or `T`s
        ///          default values, if there is no `NodeId` vertex present.
        ///          
        /// </summary>
        /// <param name="id">The `NodeId` to write/read the given `T` value</param>
        /// <param name="card">The `Cardinal` to write/read the given `T` value</param>
        /// <returns>The given `T` value at the position and direction</returns>
        public T this[NodeId id, NodeId idAdj]
        {
            get
            {
                if (!_adjacencyList.ContainsKey(id))
                    throw new ArgumentException($"TrackGraph does not contain Vertex {id}");
                if (!_adjacencyList.ContainsKey(idAdj))
                    throw new ArgumentException($"TrackGraph does not contain Vertex {idAdj}");

                try
                {
                    var card = Utilities.CardinalBetween(id, idAdj);
                    return this[id, card];
                }
                catch(ArgumentException)
                {
                    throw new ArgumentException($"Attempted to retrieve edge between two non-adjacent nodes: {id} and {idAdj}");
                }
            }
            set
            {
                try
                {
                    var card = Utilities.CardinalBetween(id, idAdj);
                    this[id, card] = value;
                }
                catch(ArgumentException)
                {
                    throw new ArgumentException($"Attempted to create edge between two non-adjacent nodes: {id} and {idAdj}");
                }
            }
        }


        public TrackGraph<T> Clone(Func<KeyValuePair<NodeId, T[]>, T[]> cloneEdgeFunction) 
            => new TrackGraph<T>
            {
                _adjacencyList = this._adjacencyList.ToDictionary(
                    entry => entry.Key,
                    entry => cloneEdgeFunction(entry)
                )
            };        

        private void InsertAt(NodeId id, Cardinal card, T value)
        {
            if (!_adjacencyList.ContainsKey(id))
                _adjacencyList[id] = 
                    _defaultEdgesFactory?.Invoke() ?? 
                    Enumerable.Repeat(default(T), (int)Cardinal.MAX_CARDINAL).ToArray();

            _adjacencyList[id][(int)card] = value;
        }
    }
}
