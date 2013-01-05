using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace forex_arbitrage
{
    /// <summary>
    /// Interaction logic for ForexTicker.xaml
    /// </summary>
    public partial class ForexTicker : UserControl
    {
        #region Fields

        private bool m_isNormalized = true;
        private Stopwatch m_stopwatch = new Stopwatch();

        public const bool TRACE_TICKS = false;

        #endregion

        #region Properties

        public bool IsNormalized
        {
            get { return m_isNormalized; }
        }

        public int I
        {
            get
            {
                return Id & 0xFFFF;
            }
        }

        public int J
        {
            get
            {
                return (Id >> 16) & 0xFFFF;
            }
        }

        public string LocalSymbol
        {
            get
            {
                return IsNormalized ? Underlying + "." + Currency : Currency + "." + Underlying;
            }
        }

        public string LocalCurrency
        {
            get
            {
                return IsNormalized ? Currency : Underlying;
            }
        }

        public Currencies CurrencyId
        {
            get
            {
                return (Currencies)Enum.Parse(typeof(Currencies), Currency);
            }
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(int),
            typeof(ForexTicker));

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register("Price", typeof(double),
            typeof(ForexTicker));

        public double Price
        {
            get { return (double)GetValue(PriceProperty); }
            set 
            {
                if (TRACE_TICKS)
                {
                    m_stopwatch.Stop();
                    Log.Info("i(" + I + ") j(" + J + ") = " + m_stopwatch.ElapsedMilliseconds);
                }
                
                double fildered = value != 0 && !IsNormalized ? 1 / value : value;
                fildered = Math.Round(fildered, 5);
                SetValue(PriceProperty, fildered);

                if (TRACE_TICKS) m_stopwatch.Restart();
            }
        }

        public static readonly DependencyProperty UnderlyingProperty =
            DependencyProperty.Register("Underlying", typeof(String),
            typeof(ForexTicker));

        public String Underlying
        {
            get { return (String)GetValue(UnderlyingProperty); }
            set { SetValue(UnderlyingProperty, value); }
        }

        public static readonly DependencyProperty CurrencyProperty =
            DependencyProperty.Register("Currency", typeof(String),
            typeof(ForexTicker));

        public String Currency
        {
            get { return (String)GetValue(CurrencyProperty); }
            set { SetValue(CurrencyProperty, value); }
        }

        #endregion

        #region Constructor

        public ForexTicker()
        {
            InitializeComponent();
        }

        public ForexTicker(int i, int j, String underlying, String currency)
            : this()
        {
            InitializeComponent();
            Underlying = underlying;
            Currency = currency;
            Id = (i << 0) | (j << 16);
            m_isNormalized = CalculateNormalized();
            
            if (TRACE_TICKS) m_stopwatch.Start();
        }

        #endregion

        #region Members

        private bool CalculateNormalized()
        {
            if (Underlying == "USD")
            {
                switch (Currency)
                {
                    case "EUR":
                        return false;
                    case "CHF":
                    case "JPY":
                    case "HKD":
                    case "CAD":
                    case "CNH":
                    case "CZK":
                    case "DKK":
                    case "HUF":
                    case "ILS":
                    case "MXN":
                    case "NOK":
                    case "PLN":
                    case "RUB":
                    case "SEK":
                    case "SGD":
                        return true;
                }
            }
            else if (Underlying == "EUR")
            {
                switch (Currency)
                {
                    case "USD":
                    case "JPY":
                    case "CHF":
                        return true;
                }
            }
            else if (Underlying == "JPY")
            {
                return false;
            }
            else if (Underlying == "CHF")
            {
                switch (Currency)
                {
                    case "USD":
                    case "EUR":
                        return false;
                    case "JPY":
                        return true;
                }
            }

            return true;
        }

        #endregion
    }
}
