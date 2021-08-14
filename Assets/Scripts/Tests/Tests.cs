using Rails.Collections;
using Rails.Data;
using Rails.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rails
{
    public class Tests : TesterBase
    {
        private NodeId nodeMid = new NodeId(1, 1);
        private NodeId nodeN = new NodeId(1, 2);
        private NodeId nodeNE = new NodeId(2, 2);
        private NodeId nodeSE = new NodeId(2, 1);
        private NodeId nodeS = new NodeId(1, 0);
        private NodeId nodeSW = new NodeId(0, 1);
        private NodeId nodeNW = new NodeId(0, 2);

        private void Start()
        {
            // PriorityQueue
            TestMethod(TestPriorityQueueOrder);
            TestMethod(TestPriorityQueuePeek);

            // TrackGraph
            TestMethod(TestTrackGraphInsertion);
            TestMethod(TestTrackGraphTryGetValues);
            TestMethod(TestTrackGraphDFS);
            
            // Utilities
            TestMethod(TestReflectCardinal);
            TestMethod(TestCardinalBetween);
            TestMethod(TestPointTowards);
            TestThrowsException(TestCardinalBetweenException, typeof(ArgumentException));

            // Deck
            TestMethod(TestNoCityRepeats);
        }

        #region PriorityQueueTests 
        private void TestPriorityQueueOrder()
        {
            var queue = new PriorityQueue<int>();
            queue.Insert(10);
            queue.Insert(4);
            queue.Insert(6);
            queue.Insert(1);
            queue.Insert(6);

            Assert(queue.Pop() == 1);
            Assert(queue.Pop() == 4);
            Assert(queue.Pop() == 6);
            Assert(queue.Pop() == 6);
            Assert(queue.Pop() == 10);
        }
        private void TestPriorityQueuePeek()
        {
            var queue = new PriorityQueue<string>();
            queue.Insert("x");
            queue.Insert("d");
            queue.Insert("p");
            queue.Insert("z");

            Assert(queue.Peek() == "d");
            Assert(queue.Pop() == "d");
            Assert(queue.Pop() == "p");
            Assert(queue.Pop() == "x");
            Assert(queue.Pop() == "z");

            Assert(queue.Peek() == null);
            Assert(queue.Pop() == null);
        }
        #endregion

        #region TrackGraph Tests 
        private void TestTrackGraphInsertion()
        {
            var graph = new TrackGraph<int>();

            var nodeId = new NodeId(10, 10);
            var nodeIdToward = Utilities.PointTowards(new NodeId(10, 10), Cardinal.NW);

            graph[nodeId, Cardinal.NW] = 20;
            Assert(graph[nodeId, Cardinal.NW] == 20);
            Assert(graph[nodeId, nodeIdToward] == 20);
            Assert(graph[nodeIdToward, Cardinal.SE] == 20);
            Assert(graph[nodeIdToward, nodeId] == 20);
        }
        private void TestTrackGraphTryGetValues()
        {
            var graph = new TrackGraph<string>();
            graph[new NodeId(1, 1), Cardinal.SE] = "Apple";
            graph[new NodeId(4, 8), Cardinal.S] = "Banana";

            graph[new NodeId(10, 10), Cardinal.NW] = "Orange";
            graph[new NodeId(10, 10), Cardinal.NE] = "Peach";
            graph[new NodeId(10, 10), Cardinal.N] = "Strawberry";

            var testPoint = Utilities.PointTowards(new NodeId(10, 10), Cardinal.NW);

            Assert(graph.TryGetEdgeValue(new NodeId(1, 1), Cardinal.SE, out var apple) && apple == "Apple");
            Assert(graph.TryGetEdgeValue(new NodeId(4, 8), Cardinal.S, out var banana) && banana == "Banana");

            var values = graph.TryGetEdges(new NodeId(10, 10), out var strs);
            Assert(strs.Any(s => s == "Orange"));
            Assert(strs.Any(s => s == "Peach"));
            Assert(strs.Any(s => s == "Strawberry"));

            Assert(graph.TryGetEdgeValue(testPoint, new NodeId(10, 10), out var orange) && orange == "Orange");

            Assert(graph.TryGetEdgeValue(testPoint, Cardinal.NW, out var nullVal) && nullVal == null);

            Assert(!graph.TryGetEdgeValue(new NodeId(20, 20), Cardinal.NW, out var _));
            Assert(!graph.TryGetEdges(new NodeId(20, 20), out var _));
        }        
        private void TestTrackGraphDFS()
        {
            var graph = new TrackGraph<int>();
            graph[new NodeId(3, 8), Cardinal.N] = 8;
            graph[new NodeId(3, 8), Cardinal.S] = 9;
            graph[new NodeId(3, 8), Cardinal.NW] = 10;
            graph[new NodeId(3, 8), Cardinal.SE] = 10;
            graph[new NodeId(3, 8), Cardinal.NE] = 10;

            graph[new NodeId(8, 8), Cardinal.N] = 8;
            graph[new NodeId(8, 8), Cardinal.S] = 9;
            graph[new NodeId(8, 8), Cardinal.NW] = 10;
            graph[new NodeId(8, 8), Cardinal.SE] = 10;
            graph[new NodeId(8, 8), Cardinal.NE] = 10;

            graph[new NodeId(13, 8), Cardinal.N] = 7;
            graph[new NodeId(13, 8), Cardinal.S] = 9;
            graph[new NodeId(13, 8), Cardinal.NW] = 10;
            graph[new NodeId(13, 8), Cardinal.SE] = 10;
            graph[new NodeId(13, 8), Cardinal.NE] = 10;

            graph[new NodeId(18, 8), Cardinal.NE] = 10;

            Assert(graph.GetConnected(7, id => id).Length == 1);
            Assert(graph.GetConnected(8, id => id).Length == 2);
            Assert(graph.GetConnected(9, id => id).Length == 3);
            Assert(graph.GetConnected(10, id => id).Length == 4);

            Assert(graph.GetConnected(7, id => id).Sum(hs => hs.Count) == 2);
            Assert(graph.GetConnected(8, id => id).Sum(hs => hs.Count) == 4);
            Assert(graph.GetConnected(9, id => id).Sum(hs => hs.Count) == 6);
            Assert(graph.GetConnected(10, id => id).Sum(hs => hs.Count) == 14);
        }
        #endregion

        #region UtilitiesTests
        private void TestReflectCardinal()
        {
            Assert(Utilities.ReflectCardinal(Cardinal.N) == Cardinal.S);
            Assert(Utilities.ReflectCardinal(Cardinal.NE) == Cardinal.SW);
            Assert(Utilities.ReflectCardinal(Cardinal.NW) == Cardinal.SE);
            Assert(Utilities.ReflectCardinal(Cardinal.S) == Cardinal.N);
            Assert(Utilities.ReflectCardinal(Cardinal.SW) == Cardinal.NE);
            Assert(Utilities.ReflectCardinal(Cardinal.SE) == Cardinal.NW);
        }

        private void TestCardinalBetween()
        {
            Assert(Utilities.CardinalBetween(nodeMid, nodeN) == Cardinal.N);
            Assert(Utilities.CardinalBetween(nodeMid, nodeNE) == Cardinal.NE);
            Assert(Utilities.CardinalBetween(nodeMid, nodeSE) == Cardinal.SE);
            Assert(Utilities.CardinalBetween(nodeMid, nodeS) == Cardinal.S);
            Assert(Utilities.CardinalBetween(nodeMid, nodeSW) == Cardinal.SW);
            Assert(Utilities.CardinalBetween(nodeMid, nodeNW) == Cardinal.NW);
        }

        private void TestCardinalBetweenException() => Utilities.CardinalBetween(nodeMid, new NodeId(20, 20));

        private void TestPointTowards()
        {
            Assert(Utilities.PointTowards(nodeMid, Cardinal.N) == nodeN);
            Assert(Utilities.PointTowards(nodeMid, Cardinal.NE) == nodeNE);
            Assert(Utilities.PointTowards(nodeMid, Cardinal.SE) == nodeSE);
            Assert(Utilities.PointTowards(nodeMid, Cardinal.S) == nodeS);
            Assert(Utilities.PointTowards(nodeMid, Cardinal.SW) == nodeSW);
            Assert(Utilities.PointTowards(nodeMid, Cardinal.NW) == nodeNW);
        }
        #endregion

        #region DeckTests
        private void TestNoCityRepeats()
        {
            Deck.Initialize();

            List<DemandCard> demandCards = new List<DemandCard>();
            for (int i = 0; i < Deck.DemandCardCount; ++i)
                demandCards.Add(Deck.DrawOne());

            foreach (var demandCard in demandCards)
            {
                foreach (var demand in demandCard)
                    Assert(!demandCard.Any(d => d != demand && d.City == demand.City));

                Deck.Discard(demandCard);
            }
        }
        #endregion
    }
}
