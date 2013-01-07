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
                return IsNormalized ? Currency.ToString() : Underlying.ToString();
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
            DependencyProperty.Register("Underlying", typeof(Currencies),
            typeof(ForexTicker));

        public Currencies Underlying
        {
            get { return (Currencies)GetValue(UnderlyingProperty); }
            set { SetValue(UnderlyingProperty, value); }
        }

        public static readonly DependencyProperty CurrencyProperty =
            DependencyProperty.Register("Currency", typeof(Currencies),
            typeof(ForexTicker));

        public Currencies Currency
        {
            get { return (Currencies)GetValue(CurrencyProperty); }
            set { SetValue(CurrencyProperty, value); }
        }

        #endregion

        #region Constructor

        public ForexTicker()
        {
            InitializeComponent();
        }

        public ForexTicker(int i, int j, Currencies underlying, Currencies currency)
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
            if (Underlying == Currencies.USD)
            {
                switch (Currency)
                {
                    case Currencies.EUR:
                    case Currencies.GBP:
                    case Currencies.AUD:
                        return false;
                    case Currencies.JPY:
                    case Currencies.CHF:
                    case Currencies.CAD:
                        return true;
                }
            }
            else if (Underlying == Currencies.EUR)
            {
                switch (Currency)
                {
                    case Currencies.USD:
                    case Currencies.JPY:
                    case Currencies.GBP:
                    case Currencies.AUD:
                    case Currencies.CHF:
                    case Currencies.CAD:
                        return true;
                }
            }
            else if (Underlying == Currencies.JPY)
            {
                switch (Currency)
                {
                    case Currencies.USD:
                    case Currencies.EUR:
                    case Currencies.GBP:
                    case Currencies.AUD:
                    case Currencies.CHF:
                    case Currencies.CAD:
                        return false;
                }
            }
            else if (Underlying == Currencies.GBP)
            {
                switch (Currency)
                {
                    case Currencies.EUR:
                        return false;
                    case Currencies.USD:
                    case Currencies.JPY:
                    case Currencies.AUD:
                    case Currencies.CHF:
                    case Currencies.CAD:
                        return true;
                }
            }
            else if (Underlying == Currencies.AUD)
            {
                switch (Currency)
                {
                    case Currencies.EUR:
                    case Currencies.GBP:
                        return false;
                    case Currencies.USD:
                    case Currencies.JPY:
                    case Currencies.CHF:
                    case Currencies.CAD:
                        return true;
                }
            }
            else if (Underlying == Currencies.CHF)
            {
                switch (Currency)
                {
                    case Currencies.USD:
                    case Currencies.EUR:
                    case Currencies.GBP:
                    case Currencies.AUD:
                    case Currencies.CAD:
                        return false;
                    case Currencies.JPY:
                        return true;
                }
            }
            else if (Underlying == Currencies.CAD)
            {
                switch (Currency)
                {
                    case Currencies.USD:
                    case Currencies.EUR:
                    case Currencies.GBP:
                    case Currencies.AUD:
                        return false;
                    case Currencies.JPY:
                    case Currencies.CHF:
                        return true;
                }
            }

            return false;
        }

        #endregion
    }
}
