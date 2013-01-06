using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forex_arbitrage
{
    public class ArbitrageCycleComparer : IEqualityComparer<ArbitrageCycle>
    {
        public bool Equals(ArbitrageCycle x, ArbitrageCycle y)
        {
            int xCount = x.Edges.Count;
            int yCount = y.Edges.Count;
            return xCount == yCount && x.Edges.Union(y.Edges).Count() == xCount;
        }

        public int GetHashCode(ArbitrageCycle obj)
        {
            return obj.Edges.GetHashCode();
        }
    }

    [DebuggerDisplay("{Path} = {Profit}")]
    public class ArbitrageCycle : IComparable<ArbitrageCycle>, IComparer<ArbitrageCycle>, ICloneable
    {
        #region Fields

        private List<DirectedEdge> m_edges = new List<DirectedEdge>();
        private DateTime m_origin = DateTime.Now;        
        private DateTime m_current = DateTime.Now;        

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
                    return f.IsSet && l.IsSet ? f.From == l.To : false;
                }
                return false;
            }
        }

        public DateTime Origin
        {
            get { return m_origin; }
        }

        public DateTime Current
        {
            get { return m_current; }
            set { m_current = value; }
        }

        public TimeSpan LifeTime
        {
            get { return m_current - m_origin; }
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
                string path = "\n";
                foreach (DirectedEdge edge in m_edges)
                {
                    path += edge.Pair + " " + edge.Weight + "\n";
                }
                return path;
            }
        }

        public string Summary
        {
            get
            {
                return Path + "profit(" + Profit + ")";
            }
        }

        #endregion

        #region Constructors

        public ArbitrageCycle(DateTime origin, params DirectedEdge[] edges)
        {
            m_origin = origin;
            foreach (DirectedEdge edge in edges)
            {
                m_edges.Add(edge);
            }
        }

        public ArbitrageCycle(ArbitrageCycle cycle)
        {
            m_origin = cycle.Origin;
            DirectedEdge[] temp = cycle.Edges.Select(item => (DirectedEdge)item).ToArray();
            m_edges = new List<DirectedEdge>(temp);
        }

        #endregion

        #region Members

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

        public int GetHashCode(ArbitrageCycle obj)
        {
            return obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return m_edges.GetHashCode();
        }

        public bool Equals(ArbitrageCycle x, ArbitrageCycle y)
        {
            return x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            return m_edges.Equals(obj);
        }

        public object Clone()
        {
            return new ArbitrageCycle(this);
        }

        #endregion
    }
}
