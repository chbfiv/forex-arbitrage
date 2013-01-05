using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forex_arbitrage
{
    [DebuggerDisplay("{Path} = {Profit}")]
    public class ArbitrageCycle : IComparable<ArbitrageCycle>, IComparer<ArbitrageCycle>, IEqualityComparer<ArbitrageCycle>, IEquatable<ArbitrageCycle>, ICloneable
    {
        #region Fields

        private List<DirectedEdge> m_edges = new List<DirectedEdge>();

        #endregion

        #region Properties

        public List<DirectedEdge> Edges
        {
            get { return m_edges; }
        }

        public bool HasMinimumDepth
        {
            get { return m_edges.Count > 2; }
        }

        public bool HasMaximumDepth
        {
            get { return m_edges.Count > 5; }
        }

        public bool IsCompleteCycle
        {
            get
            {
                if (m_edges.Count > 2)
                {
                    DirectedEdge f = m_edges[0];
                    DirectedEdge l = m_edges[m_edges.Count - 1];
                    return f != null && l != null ? f.From == l.To : false;
                }
                return false;
            }
        }


        public double Profit
        {
            get
            {
                double profit = 1;
                foreach (DirectedEdge edge in m_edges)
                {
                    profit *= edge.Weight;
                }
                return profit;
            }
        }

        public string Path
        {
            get
            {
                string path = string.Empty;
                foreach (DirectedEdge edge in m_edges)
                {
                    path += edge.Pair + ">";
                }
                return path;
            }
        }

        #endregion

        #region Constructors

        public ArbitrageCycle(params DirectedEdge[] edges)
        {
            foreach (DirectedEdge edge in edges)
            {
                m_edges.Add(edge);
            }
        }

        public ArbitrageCycle(ArbitrageCycle cycle)
        {
            DirectedEdge[] temp = cycle.Edges.Select(item => (DirectedEdge)item.Clone()).ToArray();
            m_edges = new List<DirectedEdge>(temp);
        }

        #endregion

        #region Members}

        public void Add(DirectedEdge edge)
        {
            if (!m_edges.Contains(edge))
            {
                m_edges.Add(edge);
            }
        }

        public int CompareTo(ArbitrageCycle other)
        {
            double from = 1;
            double to = 1;
            foreach (DirectedEdge edge in Edges)
            {
                from *= edge.Weight;
            }

            foreach (DirectedEdge edge in other.Edges)
            {
                to *= edge.Weight;
            }

            if (from < to) return -1;
            else if (from > to) return 1;
            else return 0;
        }

        public int Compare(ArbitrageCycle x, ArbitrageCycle y)
        {
            return x.CompareTo(y);
        }

        public bool Equals(ArbitrageCycle x, ArbitrageCycle y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(ArbitrageCycle obj)
        {
            return m_edges.GetHashCode();
        }

        public bool Equals(ArbitrageCycle other)
        {
            return m_edges.Equals(other);
        }

        public object Clone()
        {
            return new ArbitrageCycle(this);
        }

        #endregion
    }
}
