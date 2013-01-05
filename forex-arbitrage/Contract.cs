using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TWSLib;

namespace forex_arbitrage
{
    //("USD", "EUR", "JPY", "GBP", "AUD", "CHF", "CAD"
    public enum Currencies
    {
       /* USD = 0,
        EUR = 1,
        GBP = 2,
        JPY = 3,
        HKD = 4,
        SGD = 5*/

        
        USD = 0,
        EUR = 1,
        JPY = 2,
        CHF = 3,

            /*
        USD = 0,
        EUR = 1,
        JPY = 2,
        GBP = 3,
        AUD = 4,
        CHF = 5,
        CAD = 6*/
    }

    public class Contract : IContract
    {
        #region Fields

        private string m_comboLegsDescrip = String.Empty;
        private dynamic m_comboLegs;
        private int m_conId = 0;
        private string m_currency = "USD";
        private string m_exchange = "SMART";
        private string m_expiry = String.Empty;
        private int m_includeExpired = 0;
        private string m_localSymbol = String.Empty;
        private string m_multiplier = String.Empty;
        private string m_primaryExchange = "ISLAND";
        private string m_right = String.Empty;
        private string m_secId = String.Empty;
        private string m_secIdType = String.Empty;
        private string m_secType = "STK";
        private double m_strike = 0;
        private string m_symbol = "AAPL";
        private dynamic m_underComp;

        public const string GENERIC_TICK_TAGS = "100,101,104,105,106,107,165,221,225,233,236,258,293,294,295,318";

        #endregion

        #region Properties

        public string comboLegsDescrip
        {
            get { return m_comboLegsDescrip; }
        }

        public dynamic comboLegs
        {
            //get { return m_comboLegs; }
            //set { m_comboLegs = value; }
            get { return null; }
            set {  }
        }

        public int conId
        {
            get { return m_conId; }
            set { m_conId = value; }
        }

        public string currency
        {
            get { return m_currency; }
            set { m_currency = value; }
        }

        public string exchange
        {
            get { return m_exchange; }
            set { m_exchange = value; }
        }

        public string expiry
        {
            get { return m_expiry; }
            set { m_expiry = value; }
        }

        public int includeExpired
        {
            get { return m_includeExpired; }
            set { m_includeExpired = value; }
        }

        public string localSymbol
        {
            get { return m_localSymbol; }
            set { m_localSymbol = value; }
        }

        public string multiplier
        {
            get { return m_multiplier; }
            set { m_multiplier = value; }
        }

        public string primaryExchange
        {
            get { return m_primaryExchange; }
            set { m_primaryExchange = value; }
        }

        public string right
        {
            get { return m_right; }
            set { m_right = value; }
        }

        public string secId
        {
            get { return m_secId; }
            set { m_secId = value; }
        }

        public string secIdType
        {
            get { return m_secIdType; }
            set { m_secIdType = value; }
        }

        public string secType
        {
            get { return m_secType; }
            set { m_secType = value; }
        }

        public double strike
        {
            get { return m_strike; }
            set { m_strike = value; }
        }

        public string symbol
        {
            get { return m_symbol; }
            set { m_symbol = value; }
        }

        public dynamic underComp
        {
            //get { return m_underComp; }
            //set { m_underComp = value; }
            get { return null; }
            set {  }
        }

        #endregion

        #region Constructors

        public Contract(String symbol)
        {
            m_symbol = symbol;
        }

        public Contract(String symbol, String secType)
        {
            m_symbol = symbol;
            m_secType = secType;
        }

        #endregion

        #region Builders

        /*public static IContract BuildContract(String symbol)
        {
            IContract contract = new IContract();
            contract.conId = 0;
            contract.currency = String.Empty;

            return contract;
        //     private string m_comboLegsDescrip = String.Empty;
        //private dynamic m_comboLegs;
        //private int m_conId = 0;
        //private string m_currency = "USD";
        //private string m_exchange = "SMART";
        //private string m_expiry = String.Empty;
        //private int m_includeExpired = 0;
        //private string m_localSymbol = String.Empty;
        //private string m_multiplier = String.Empty;
        //private string m_primaryExchange = "ISLAND";
        //private string m_right = String.Empty;
        //private string m_secId = String.Empty;
        //private string m_secIdType = String.Empty;
        //private string m_secType = "STK";
        //private double m_strike = 0;
        //private string m_symbol = "AAPL";
        //private dynamic m_underComp;
        }*/

        #endregion
    }
}
