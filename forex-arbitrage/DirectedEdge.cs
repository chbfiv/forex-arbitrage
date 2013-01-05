using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forex_arbitrage
{
    [DebuggerDisplay("{Pair} = {Weight}")]
    public class DirectedEdge : ICloneable //, IEqualityComparer<DirectedEdge> //, IEquatable<DirectedEdge> //, IComparable<DirectedEdge>, IComparer<DirectedEdge>, IEqualityComparer<DirectedEdge>, IEquatable<DirectedEdge>
    {
        #region Fields

        private int m_v;
        private int m_w;
        private double m_weight;

        #endregion

        #region Properties

        public int From
        {
            get { return m_v; }
            set { m_v = value; }
        }

        public int To
        {
            get { return m_w; }
            set { m_w = value; }
        }

        public double Weight
        {
            get { return m_weight; }
            set { m_weight = value; }
        }

        public string Pair
        {
            get
            {                
                return ((Currencies)From).ToString() + ":" + ((Currencies)To).ToString();
            }
        }

        #endregion

        #region Constructor

        public DirectedEdge(int v, int w, double weight)
        {
            m_v = v;
            m_w = w;
            m_weight = weight;
        }

        public DirectedEdge(DirectedEdge edge)
        {
            From = edge.From;
            To = edge.To;
            Weight = edge.Weight;
        }

        #endregion

        #region Members
        /*
        public int Compare(DirectedEdge x, DirectedEdge y)
        {
            return x.CompareTo(y);
        }

        public int CompareTo(DirectedEdge other)
        {
            //if (From < other.From)
            //{
            //    return -1;
            //}
            //else if (From > other.From)
            //{
            //    return 1;
            //}
            //else if (To < other.To)
            //{
            //    return -1;
            //}
            //else if (To > other.To)
            //{
            //    return 1;
            //}
            //else 
            //{
            //    return 0;
            //}
             
            if (Weight < other.Weight) return -1;
            else if (Weight > other.Weight) return 1;
            else return 0;
        }
        */
        /*
        public bool Equals(DirectedEdge x, DirectedEdge y)
        {

            //Check whether the compared objects reference the same data. 
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null. 
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) return false;

            return x.From == y.From && x.To == y.To;
        }

        public int GetHashCode(DirectedEdge obj)
        {
            //Check whether the object is null 
            if (Object.ReferenceEquals(obj, null)) return 0;

            return obj.From.GetHashCode() ^ obj.To.GetHashCode();
        }*/

        public override bool Equals(object obj)
        {
            DirectedEdge x = this;
            DirectedEdge y = obj as DirectedEdge;

            //Check whether the compared objects reference the same data. 
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null. 
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) return false;
            
            return x.From == y.From && x.To == y.To;
        }

        public override int GetHashCode()
        {
            //Check whether the object is null 
            //if (Object.ReferenceEquals(this, null)) return 0;

            return From.GetHashCode() ^ To.GetHashCode();
        }

        /*public bool Equals(DirectedEdge other)
        {
            return From == other.From && To == other.To;
        }*/

        public object Clone()
        {
            return new DirectedEdge(this);
        }

        #endregion

    }
}
