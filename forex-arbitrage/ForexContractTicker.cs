using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forex_arbitrage
{
    public class ForexContractTicker
    {
        #region Fields

        private double m_price = 0;
        private String m_underlying = String.Empty;
        private String m_currency = String.Empty;

        #endregion 

        #region Properties

        public double Price
        {
            get { return m_price; }
            set { m_price = value; }
        }

        #endregion

        #region Contrustors

        public ForexContractTicker(String underlying, String currency)
        {
            m_underlying = underlying;
            m_currency = currency;
        }

        #endregion

        #region Members

        public String ResolveSymbol()
        {
            String result = String.Empty;
            if (m_underlying == "USD")
            {

            }
        }

        #endregion
    }
}
