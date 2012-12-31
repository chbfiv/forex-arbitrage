using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using TWSLib;


namespace forex_arbitrage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Tws m_tws = new Tws();
        private Task m_task;
        private CancellationTokenSource m_taskToken = new CancellationTokenSource();
        private ForexTicker[,] m_tickers;

        public const int NO_SECURITY_DEF_FOUND = 200;

        public MainWindow()
        {
            InitializeComponent();

            m_tws.tickPrice += m_tws_tickPrice;
            m_tws.errMsg += m_tws_errMsg;

            //TODO: hack grid lines colors
            var T = Type.GetType("System.Windows.Controls.Grid+GridLinesRenderer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            var GLR = Activator.CreateInstance(T);
            GLR.GetType().GetField("s_oddDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.Gray, 1.0));
            GLR.GetType().GetField("s_evenDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.Gray, 1.0));

            myGrid.ShowGridLines = true;

            myGrid.Visibility = Visibility.Hidden;
        }

        private void m_tws_errMsg(int id, int errorCode, string errorMsg)
        {
            StatusError(errorMsg + " [" + id + ":" + errorCode + "]");

            if (errorCode == NO_SECURITY_DEF_FOUND)
            {
                int i = id & 0xFFFF;
                int j = (id >> 16) & 0xFFFF;

                ForexTicker ticker = m_tickers[i, j];
                if (ticker != null)
                {
                    myGrid.Children.Remove(ticker);
                    m_tickers[i, j] = null;
                }
            }
        }

        private void File_Connect_Clicked(object sender, RoutedEventArgs e)
        {
            ConnectWindow win = new ConnectWindow();
            if (win.Prompt())
            {
                Status("Connecting to TWS using clientId " + win.ClientId + " ...");
                
                m_tws.connect(win.Host, win.Port, win.ClientId);
                if (m_tws.serverVersion > 0)
                {
                    Connected();
                }
                else
                {
                    StatusError("Failed to connect to TWS server.");
                }
            }
        }

        private void RequestMarketData(params string[] symbols)
        {
            m_tickers = new ForexTicker[symbols.Length, symbols.Length];

            myGrid.Children.Clear();
            myGrid.RowDefinitions.Clear();
            myGrid.ColumnDefinitions.Clear();

            myGrid.RowDefinitions.Add(new RowDefinition());
            myGrid.ColumnDefinitions.Add(new ColumnDefinition());
            int OFFSET = 1;

            for (int i = 0; i < symbols.Length; i++)
            {
                for (int j = 0; j < symbols.Length; j++)
                {   
                    if (i == 0)
                    {
                        myGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        Label lbl = new Label();
                        lbl.Content = symbols[j];
                        Grid.SetRow(lbl, 0);
                        Grid.SetColumn(lbl, j + OFFSET);
                        myGrid.Children.Add(lbl);
                    }

                    if (j == 0)
                    {
                        myGrid.RowDefinitions.Add(new RowDefinition());
                        Label lbl = new Label();
                        lbl.Content = symbols[i];
                        Grid.SetRow(lbl, i + OFFSET);
                        Grid.SetColumn(lbl, 0);
                        myGrid.Children.Add(lbl);
                    }

                    if (i != j)
                    {
                        ForexTicker ticker = new ForexTicker(i, j, symbols[i], symbols[j]);
                        Grid.SetRow(ticker, i + OFFSET);
                        Grid.SetColumn(ticker, j + OFFSET);
                        myGrid.Children.Add(ticker);                        
                        m_tickers[i,j] = ticker;

                        m_tws.reqMktData2(ticker.Id, ticker.LocalSymbol, "CASH", "SMART", "IDEALPRO", ticker.LocalCurrency, Contract.GENERIC_TICK_TAGS, 0);
                    }
                }
            }

            /*This is the security type. Valid values are:
STK
OPT
FUT
IND
FOP
CASH
BAG*/
            
            //m_tws.reqMktData(++i, symbol, "STK", String.Empty, 0, String.Empty, String.Empty, "SMART", "ISLAND", "USD", Contract.GENERIC_TICK_TAGS, 0);
            
        }

        private void m_tws_tickPrice(int id, int tickType, double price, int canAutoExecute)
        {
            int i = id & 0xFFFF;
            int j = (id >> 16) & 0xFFFF;

            ForexTicker ticker = m_tickers[i, j];
            if (ticker != null)
            {
                ticker.Price = price;
            }
        }

        private int k = 0;

        private void WhileConnectedTick()
        {
            while (m_tws.serverVersion > 0)
            {

                Thread.Sleep(50);
            }

            m_taskToken.Cancel();
            Disconnected(false);
        }

        private void File_Disconnect_Clicked(object sender, RoutedEventArgs e)
        {
            if (m_tws.serverVersion > 0)
            {
                m_tws.disconnect();
            }
            else
            {
                StatusError("Already disconnected from TWS server.");
            }
        }

        private void File_Exit_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Disconnected(bool error)
        {
            Dispatcher.Invoke(() =>
            {
                myGrid.Visibility = Visibility.Hidden;
                networkStatus.Text = "Offline";
                if (!error) Status("Disconnected from TWS server.");
                else StatusError("Disconnected from TWS server.");
            });
        }

        private void Connected()
        {
            Dispatcher.Invoke(() =>
            {
                Status("Connected to TWS server version " + m_tws.serverVersion + " at " + m_tws.TwsConnectionTime);
                myGrid.Visibility = Visibility.Visible;
                networkStatus.Text = "Online";

                if (m_task != null)
                {
                    m_taskToken.Cancel();
                    m_taskToken = new CancellationTokenSource();
                }

                m_task = Task.Factory.StartNew(WhileConnectedTick, m_taskToken.Token);

                RequestMarketData("USD", "EUR", "JPY", "GBP", "AUD", "CHF", "CAD");
            });
        }

        private void Status(String value)
        {
            Dispatcher.Invoke(() =>
            {
                generalStatus.Foreground = Brushes.Black;
                generalStatus.Text = value;
                Log.Info(value);
            });
        }

        private void StatusError(String value)
        {
            Dispatcher.Invoke(() =>
            {
                generalStatus.Foreground = Brushes.Red;
                generalStatus.Text = value;
                Log.Error(value);
            });
        }
    }
}

