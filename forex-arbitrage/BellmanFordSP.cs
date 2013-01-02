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
        private int cost;
        private IEnumerable<DirectedEdge> m_cycle;

        #endregion

        #region Properties

        public IEnumerable<DirectedEdge> negativeCycle
        {
            get { return m_cycle; }
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

            while (m_queue.Count > 0 && !hasNegativeCycle)
            {
                int v = m_queue.Dequeue();
                m_onQueue[v] = false;
                relax(G, v);
            }

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
                if (cost++ % G.V == 0)
                    findNegativeCycle();
            }
        }

        private void findNegativeCycle()
        {
            int V = m_edgeTo.Length;
            EdgeWeightedDigraph spt = new EdgeWeightedDigraph(V);
            for (int v = 0; v < V; v++)
            {
                if (m_edgeTo[v] != null)
                    spt.addEdge(m_edgeTo[v]);
            }

            EdgeWeightedDirectedCycle finder = new EdgeWeightedDirectedCycle(spt);
            m_cycle = finder.cycle;
        }

        #endregion        
    }
}
