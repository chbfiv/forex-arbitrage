using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forex_arbitrage
{
    public class BellmanFordSP
    {
        #region Fields

        private double[] m_distTo;
        private DirectedEdge[] m_edgeTo;
        private bool[] m_onQueue;
        private Queue<int> m_queue;
        private int m_cost;
        private IEnumerable<DirectedEdge> m_cycle;

        #endregion

        #region Properties

        public IEnumerable<DirectedEdge> negativeCycle
        {
            get { return m_cycle; }
        }

        public int negativeCycleCount
        {
            get
            {
                return m_cycle != null ? m_cycle.Count() : 0; 
            }
        }

        public Queue<int> Queue
        {
            get { return m_queue; }
        }

        public bool hasNegativeCycle
        {
            get
            {
                return m_cycle != null;
            }
        }

        #endregion

        #region Constructor

        public BellmanFordSP(EdgeWeightedDigraph G, int s)
        {
            m_distTo = new double[G.V];
            m_edgeTo = new DirectedEdge[G.V];
            m_onQueue = new bool[G.V];

            for (int v = 0; v < G.V; v++)
                m_distTo[v] = double.PositiveInfinity;

            m_distTo[s] = 0.0;

            m_queue = new Queue<int>();
            m_queue.Enqueue(s);

            int negativeCycleCheck = 1000;
            while (m_queue.Count > 0 && !hasNegativeCycle)
            {
                int v = m_queue.Dequeue();
                m_onQueue[v] = false;
                relax(G, v);

                if (m_cost++ % negativeCycleCheck == 0)
                   findNegativeCycle();
            } 
            
            //TODO
            /*if (!check(G, s))
            {
                //throw new Exception("found cycle in digraph");
                Log.Info("failed to assert a cycle in digraph");
            }*/
        }

        #endregion

        #region Members

        private void relax(EdgeWeightedDigraph G, int v)
        {
            foreach (DirectedEdge e in G.adj(v))
            {
                int w = e.To;
                if (m_distTo[w] > m_distTo[v] + e.Weight)
                {
                    m_distTo[w] = m_distTo[v] + e.Weight;
                    m_edgeTo[w] = e;
                    if (!m_onQueue[w])
                    {
                        m_queue.Enqueue(w);
                        m_onQueue[w] = true;
                    }
                }
               // if (m_cost++ % G.V == 0)
                 //   findNegativeCycle();
            }
        }

        public IEnumerable<DirectedEdge> findNegativeCycle()
        {
            int V = m_edgeTo.Length;
            EdgeWeightedDigraph spt = new EdgeWeightedDigraph(V);
            for (int v = 0; v < V; v++)
            {
                if (m_edgeTo[v].IsSet)
                    spt.addEdge(m_edgeTo[v]);
            }

            EdgeWeightedDirectedCycle finder = new EdgeWeightedDirectedCycle(spt);
            return m_cycle = finder.cycle;
        }

        #endregion        
    }
}
