using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forex_arbitrage
{
    public class EdgeWeightedDirectedCycle
    {
        #region Fields

        private bool[] m_marked;
        private DirectedEdge[] m_edgeTo;
        private bool[] m_onStack;
        private Stack<DirectedEdge> m_cycle;

        #endregion

        #region Properties
        
        public IEnumerable<DirectedEdge> cycle
        {
            get { return m_cycle; }
        }

        public bool hasCycle
        {
            get
            {
                return m_cycle != null;
            }
        }
        #endregion

        #region Constructor

        public EdgeWeightedDirectedCycle(EdgeWeightedDigraph G)
        {
            m_marked = new bool[G.V];
            m_onStack = new bool[G.V];
            m_edgeTo = new DirectedEdge[G.V];

            for (int v = 0; v < G.V; v++)
            {
                if (!m_marked[v]) dfs(G, v);
            }

            if (!check(G))
            {
                //throw new Exception("found cycle in digraph");
                Log.Error("failed to assert a cycle in digraph");
            }
        }

        #endregion

        #region Members

        private void dfs(EdgeWeightedDigraph G, int v)
        {
            m_onStack[v] = true;
            m_marked[v] = true;

            foreach (DirectedEdge e in G.adj(v))
            {
                int w = e.To;

                if (m_cycle != null)
                {
                    return;
                }
                else if (!m_marked[w])
                {
                    m_edgeTo[w] = e;
                    dfs(G, w);
                }
                else if (m_onStack[w])
                {
                    m_cycle = new Stack<DirectedEdge>();
                    DirectedEdge ee = e;
                    while (ee.From != w)
                    {
                        m_cycle.Push(ee);
                        ee = m_edgeTo[ee.From];
                    }
                    m_cycle.Push(ee);
                }
            }

            m_onStack[v] = false;
        }

        private bool check(EdgeWeightedDigraph G)
        {

            if (hasCycle)
            {
                DirectedEdge first = null;
                DirectedEdge last = null;

                foreach (DirectedEdge e in cycle)
                {
                    if (first == null) first = e;
                    if (last != null)
                    {
                        if (last.To != e.From)
                        {
                            Log.Info("cycle edges " + last + " and " + e + " not incident");
                            return false;
                        }
                    }
                    last = e;
                }

                if (last.To != first.From)
                {
                    Log.Info("cycle edges " + last + " and " + first + " not incident");
                    return false;
                }
            }

            return true;
        }

        #endregion        
    }
}
