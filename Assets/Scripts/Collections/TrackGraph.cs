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
        
        /// <summary>
        /// Creates a new TrackGraph
        /// </summary>
        public TrackGraph()
            => _adjacencyList = new Dictionary<NodeId, T[]>();        
        
        /// <summary>
        /// Creates a new TrackGraph, supplying a method which gives
        /// default values upon instantiating a new vertex.
        /// </summary>
        /// <param name="defaultEdgesFactory">The method which supplies edge default values.</param>
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
                // Throw an exception if the vertex doesn't exist
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
                // Throw exception if either vertex doesn't exist
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
                    // Throw an exception if the vertices aren't adjacent
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
        
        /// <summary>
        /// Tests if the `TrackGraph` contains the given `NodeId` vertex
        /// </summary>
        /// <param name="nodeId">The vertex position to check</param>
        /// <returns>True if a vertex is found. False if not</returns>
        public bool ContainsVertex(NodeId nodeId) => _adjacencyList.ContainsKey(nodeId);

        /// <summary>
        /// Attempts to retrieve all `Cardinal` values at the given vertex.
        /// </summary>
        /// <param name="nodeId">The vertex position to check</param>
        /// <param name="edgeValues">The array to write the values to</param>
        /// <returns>True is a vertex is found. False if not</returns>
        public bool TryGetValue(NodeId nodeId, out T[] edgeValues) => _adjacencyList.TryGetValue(nodeId, out edgeValues);

        /// <summary>
        /// Clones the given TrackGraph. While the vertices will be automatically deep cloned,
        /// the values will not. `cloneEdgeFactory` supplies a method to clone the values.
        /// </summary>
        /// <param name="cloneEdgeFactory">The method by which the edge `T` values are cloned.</param>
        /// <returns>The new, cloned TrackGraph</returns>
        public TrackGraph<T> Clone(Func<KeyValuePair<NodeId, T[]>, T[]> cloneEdgeFactory) 
            => new TrackGraph<T>
            {
                _adjacencyList = this._adjacencyList.ToDictionary(
                    entry => entry.Key,
                    entry => cloneEdgeFactory(entry)
                ),
                _defaultEdgesFactory = this._defaultEdgesFactory,
            };        
       
        
        // Inserts a vertex (if one doesn't exist) and Cardinal edge value
        // with the given arguments.
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
