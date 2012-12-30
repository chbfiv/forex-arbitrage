using System;
using System.Collections.Generic;
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
        public ForexTicker()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TickerPriceProperty =
            DependencyProperty.Register("TickerPrice", typeof(String),
            typeof(ForexTicker));

        public String TickerPrice
        {
            get { return (String)GetValue(TickerPriceProperty); }
            set { SetValue(TickerPriceProperty, value); }
        }

        public static readonly DependencyProperty TickerPriceDeltaProperty =
            DependencyProperty.Register("TickerPriceDelta", typeof(String),
            typeof(ForexTicker));

        public String TickerPriceDelta
        {
            get { return (String)GetValue(TickerPriceDeltaProperty); }
            set { SetValue(TickerPriceDeltaProperty, value); }
        }
    }
}
