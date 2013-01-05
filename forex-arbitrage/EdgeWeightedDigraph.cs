using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forex_arbitrage
{
    public class EdgeWeightedDigraph
    {
        #region Fields

        private int m_v = 0;
        private int m_e = 0;
        private HashSet<DirectedEdge>[] m_adj;

        #endregion

        #region Properties

        public int V
        {
              get { return m_v; }
        }

        public int E
        {
            get { return m_e; }
        }

        #endregion

        #region Constructor

        public EdgeWeightedDigraph(int v)
        {
            m_v = v;
            m_adj = new HashSet<DirectedEdge>[v];
            for (int i = 0; i < m_v; i++)
                m_adj[i] = new HashSet<DirectedEdge>();
        }

        #endregion

        #region Members

        public void addEdge(DirectedEdge e)
        {
            int v = e.From;
            if (m_adj[v].Add(e))
            {
                m_e++;
            }
            /*if (!m_adj[v].Contains(e))
            {
            }
            else
            {

            }*/
        }

        public IEnumerable<DirectedEdge> adj(int v)
        {
            return m_adj[v];
        }

        public IEnumerable<DirectedEdge> edges()
        {
            List<DirectedEdge> list = new List<DirectedEdge>();
            for (int v =0; v < V; v++)
            {
                foreach(DirectedEdge e in adj(v))
                {
                    list.Add(e);
                }
            }
            return list;
        }

        public int outdegree(int v)
        {
            return m_adj[v].Count;
        }

        #endregion
    }
}
