using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forex_arbitrage
{
    public class DirectedEdge : Comparer<DirectedEdge>, IComparable<DirectedEdge>
    {
        #region Fields

        private int m_from;
        private int m_to;
        private double m_weight;

        #endregion

        #region Properties

        public int From
        {
            get { return m_from; }
            set { m_from = value; }
        }

        public int To
        {
            get { return m_to; }
            set { m_to = value; }
        }

        public double Weight
        {
            get { return m_weight; }
            set { m_weight = value; }
        }

        #endregion

        #region Constructor

        public DirectedEdge(int from, int to, double weight)
        {
            m_from = from;
            m_to = to;
            m_weight = weight;
        }

        #endregion

        #region Comparer

        public override int Compare(DirectedEdge x, DirectedEdge y)
        {
            if (x.Weight < y.Weight) return -1;
            else if (x.Weight > y.Weight) return 1;
            else return 0;
        }

        public int CompareTo(DirectedEdge other)
        {
            if (Weight < other.Weight) return -1;
            else if (Weight > other.Weight) return 1;
            else return 0;
        }

        #endregion
    }
}
