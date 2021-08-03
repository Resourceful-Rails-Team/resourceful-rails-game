using Rails.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rails.Collections
{
    /// <summary>
    /// An undirected graph representing tracks on the Map.
    /// </summary>
    /// <typeparam name="T">The type designated for edge values (stored by Cardinal direction)</typeparam>
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
        /// <para>
        /// Selects a given `T` edge value at the `NodeId` and `Cardinal`.
        /// </para>
        /// <para>
        /// get: Returns the value at the given location, or throws an `ArgumentException`, if there
        /// is nothing at the given `NodeId`.
        /// </para>
        /// <para>
        /// set: Sets the position and direction value to the one given. Automatically
        /// inserts a new `NodeId` key with the given cloneEdgeFunction, or `T`s
        /// default values, if there is no `NodeId` vertex present.
        /// </para>
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
        /// <para>
        /// Selects a given `T` edge value at the two adjacent `NodeId`s.
        /// </para>
        /// <para>
        /// get: Returns the value at the given location, or throws an `ArgumentException`, if there
        ///      is nothing at either `NodeId`, or if the `NodeId`s aren't adjacent.
        /// </para>
        /// <para>
        /// set: Sets the position and direction value to the one given. Automatically
        ///      inserts two new `NodeId` key with the given cloneEdgeFunction, or `T`s
        ///      default values, if there is no `NodeId` vertex present.
        /// </para>   
        /// </summary>
        /// <param name="id">The `NodeId` to write/read the given `T` value</param>
        /// <param name="idAdj">The `NodeId` adjacent to `id`</param>
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
        /// Attempts to retrieve all T edge-values at the given vertex.
        /// Will retrieve all Cardinal edges regardless of number of values set.
        /// All other edges will be default T values, or values defined by
        /// default edges factory.
        /// </summary>
        /// <param name="nodeId">The vertex position to check</param>
        /// <param name="edgeValues">The array to write the values to</param>
        /// <returns>True if a vertex is found. False if not.</returns>
        public bool TryGetEdges(NodeId nodeId, out T[] edgeValues) => _adjacencyList.TryGetValue(nodeId, out edgeValues);
        
        /// <summary>
        /// Attempts to retrieve a specific edge value at the given vertex and direction.
        /// </summary>
        /// <param name="id">The NodeId vertex to check</param>
        /// <param name="card">The Cardinal to check</param>
        /// <param name="edgeValue">The outputted value, if it exists</param>
        /// <returns>True if a vertex is found. False if not.</returns>
        public bool TryGetEdgeValue(NodeId id, Cardinal card, out T edgeValue)
        {
            if (_adjacencyList.TryGetValue(id, out var edges))
            {
                edgeValue = edges[(int)card];
                return true;
            }

            edgeValue = default(T);
            return false;
        }

        /// <summary>
        /// Attempts to retrieve a specific edge value at the given adjacent vertices.
        /// </summary>
        /// <param name="id">The NodeId vertex to check</param>
        /// <param name="idAdj">The adjacent NodeId vertex to check</param>
        /// <param name="edgeValue">The outputted value, if it exists</param>
        /// <returns>True if a vertex is found. False if not.</returns>
        public bool TryGetEdgeValue(NodeId id, NodeId idAdj, out T edgeValue)
        {
            try
            {
                if (_adjacencyList.TryGetValue(id, out var edges))
                {
                    var direction = Utilities.CardinalBetween(id, idAdj);
                    edgeValue = edges[(int)direction];
                    return true;
                }
                edgeValue = default(T);
                return false;
            }
            catch(ArgumentException)
            {
                throw new ArgumentException($"Attempted to retrieve edge between two non-adjacent nodes: {id} and {idAdj}");
            }
        }

        /// <summary>
        /// Clones the given TrackGraph. While the vertices do not need to be deep cloned,
        /// the values may. `cloneEdgeFactory` supplies a method to clone the values.
        /// </summary>
        /// <param name="cloneEdgeFactory">The method by which the edge `T` values are cloned.</param>
        /// <returns>The new, cloned TrackGraph</returns>
        public TrackGraph<T> Clone(Func<T[], T[]> cloneEdgeFactory) 
            => new TrackGraph<T>
            {
                _adjacencyList = this._adjacencyList.ToDictionary(
                    entry => entry.Key,
                    entry => cloneEdgeFactory(entry.Value)
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
