/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

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
        private T _defaultEdgeValue;

        /// <summary>
        /// Creates a new TrackGraph
        /// </summary>
        public TrackGraph()
        {
            _adjacencyList = new Dictionary<NodeId, T[]>();
            _defaultEdgeValue = default(T);
        }
        
        /// <summary>
        /// Creates a new TrackGraph, supplying a method which gives
        /// default values upon instantiating a new vertex.
        /// </summary>
        /// <param name="defaultEdgesFactory">The method which supplies edge default values.</param>
        public TrackGraph(T defaultEdgeValue)
        {
            _adjacencyList = new Dictionary<NodeId, T[]>();
            _defaultEdgeValue = defaultEdgeValue;
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
        /// default edges variable given at construction.
        /// </summary>
        /// <param name="nodeId">The vertex position to check</param>
        /// <param name="edgeValues">The array to write the values to</param>
        /// <returns>True if a vertex is found. False if not.</returns>
        public bool TryGetEdges(NodeId nodeId, out T[] edgeValues) => _adjacencyList.TryGetValue(nodeId, out edgeValues);
        
        /// <summary>
        /// Attempts to retrieve a specific edge value at the given vertex and direction.
        /// Returns false if the value doesn't exist.
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
        /// Clones the given TrackGraph. Does not deep clone the edge values,
        /// but it does deep clone the arrays holding the values.
        /// </summary>
        /// <returns>The new, cloned TrackGraph</returns>
        public TrackGraph<T> Clone() 
            => new TrackGraph<T>
            {
                _adjacencyList = this._adjacencyList.ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value.ToArray()
                ),
                _defaultEdgeValue = this._defaultEdgeValue,
            };

        // The DFS performed in the next two methods was adapted from the description of
        // DFS on https://www.geeksforgeeks.org/difference-between-bfs-and-dfs/

        /// <summary>
        /// Finds all subgraphs with the given edge values,
        /// performing a transformational function on their vertices
        /// and returning the set of those values.
        /// </summary>
        /// <typeparam name="U">The vertex tranform type to return, upon finding a match.</typeparam>
        /// <param name="function">The function used to determine the resulting HashSet values per subgraph.</param>
        /// <param name="matchEdgeValue">The edge value the subgraph must match to transform it's connected vertices.</param>
        /// <param name="connectEdgeValues">Edge values that connect subgraphs, but do not transform their vertices, or add them to the final sets</param>
        /// <returns>A collection of HashSets, each with vertex values transformed by the given function.</returns>
        public HashSet<U>[] GetConnected<U>(Func<NodeId, U> function, T matchEdgeValue, params T[] connectEdgeValues)
        {
            var groups = new List<HashSet<U>>();

            // All edges that have already been visited
            var visitedEdges = new HashSet<GraphEdge>();
            foreach(var pair in _adjacencyList)
            { 
                (var id, var edges) = (pair.Key, pair.Value);
                for(int i = 0; i < edges.Length; ++i)
                {
                    // Ensure the edges don't equal the default value, and that they match
                    // either the match value, or the connection values
                    if (edges[i].Equals(_defaultEdgeValue))
                        continue;
                    if (!edges[i].Equals(matchEdgeValue) && !connectEdgeValues.Any(v => edges[i].Equals(v)))
                        continue;
                    
                    var rootEdge = new GraphEdge(id, Utilities.PointTowards(id, (Cardinal)i));
                    if (visitedEdges.Contains(rootEdge))
                        continue;

                    // A new, unique group has been found 
                    groups.Add(GenerateSubgraphVertices(function, rootEdge, visitedEdges, matchEdgeValue, connectEdgeValues));
                }
            }
            return groups.ToArray();
        }

        /// <summary>
        /// Generates a vertex set of a single subgraph in the GraphTrack
        /// given a root edge to start on.
        /// </summary>
        private HashSet<U> GenerateSubgraphVertices<U>(
            Func<NodeId, U> query, 
            GraphEdge rootEdge, 
            HashSet<GraphEdge> visitedEdges,
            T matchEdgeValue, 
            params T[] connectEdgeValues
        ) {
            // The returned set
            var group = new HashSet<U>();
            // The stack to push matching edges on
            var edgeStack = new Stack<GraphEdge>();

            // Push the root edge onto the stack
            edgeStack.Push(rootEdge);
            
            while (edgeStack.Count > 0)
            {
                // Add the popped edge to the visited edges, so it doesn't
                // get visited again
                var edge = edgeStack.Pop();
                visitedEdges.Add(edge);

                // If the edge value equals the match value, transform the
                // vertices and add to the set
                if (this[edge.First, edge.Second].Equals(matchEdgeValue))
                {
                    group.Add(query(edge.First));
                    group.Add(query(edge.Second));
                }
                
                // Check all surrounding edges and see if there are any more
                // connections to the current subgraph
                for (Cardinal c = Cardinal.N; c < Cardinal.MAX_CARDINAL; ++c)
                {
                    var adjacents = new GraphEdge[]
                    {
                        new GraphEdge(edge.First, Utilities.PointTowards(edge.First, c)),
                        new GraphEdge(edge.Second, Utilities.PointTowards(edge.Second, c)),
                    };

                    foreach (var adjacent in adjacents)
                    {
                        // If this edge matches the requirements, and hasn't been visited
                        // add it to the stack
                        if (visitedEdges.Contains(adjacent))
                            continue;
                        if (!TryGetEdgeValue(adjacent.First, adjacent.Second, out var adjEdgeValue))
                            continue; 
                        if (adjEdgeValue.Equals(_defaultEdgeValue))
                            continue;
                        if (!adjEdgeValue.Equals(matchEdgeValue) && !connectEdgeValues.Any(v => adjEdgeValue.Equals(v)))
                            continue;

                        edgeStack.Push(adjacent);
                    }
                }
            }
            return group;
        }

        // Inserts a vertex (if one doesn't exist) and Cardinal edge value
        // with the given arguments.
        private void InsertAt(NodeId id, Cardinal card, T value)
        {
            if (!_adjacencyList.ContainsKey(id))
                _adjacencyList[id] =
                    Enumerable.Repeat(_defaultEdgeValue, (int)Cardinal.MAX_CARDINAL).ToArray();

            _adjacencyList[id][(int)card] = value;
        }
    }

    class GraphEdge
    {
        public NodeId First { get; set; }
        public NodeId Second { get; set; }

        public GraphEdge(NodeId first, NodeId second)
        {
            First = first;
            Second = second;
        }

        public override bool Equals(object obj) =>
            obj is GraphEdge ge && (
                (First == ge.First && Second == ge.Second) ||
                (Second == ge.First && First == ge.Second)
            );

        public override int GetHashCode()
        {
            unchecked
            {
                return 37 * (First.GetHashCode() + Second.GetHashCode());
            }
        }
    }
}
