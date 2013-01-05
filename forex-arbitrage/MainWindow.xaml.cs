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
using Bluebit.MatrixLibrary;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;

using Matrix = Bluebit.MatrixLibrary.Matrix;
using Vector = Bluebit.MatrixLibrary.Vector;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Factorization;

namespace forex_arbitrage
{
    public enum Operation_Type
    {
        Insert = 0,
		Update = 1,
		Delete = 2
    }
       
    public enum side
    {
        ASK = 0,
		BID = 1
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private Tws m_tws = new Tws();
        private Task m_task;
        private CancellationTokenSource m_taskToken = new CancellationTokenSource();
        private ForexTicker[,] m_tickers;
        private Matrix m_a;
        private Matrix m_b;
        private Matrix m_c;

        private Matrix m_aTest = new Matrix(TEST_ARBITRAGE_SIZE, TEST_ARBITRAGE_SIZE);
        private Matrix m_aTest2 = new Matrix(TEST_ARBITRAGE_SIZE, TEST_ARBITRAGE_SIZE);
        //private DenseMatrix m_aTest2 = new DenseMatrix(TEST_ARBITRAGE_SIZE, TEST_ARBITRAGE_SIZE);
        private Matrix m_bTest = new Matrix(TEST_ARBITRAGE_SIZE, TEST_ARBITRAGE_SIZE);
        private Matrix m_cTest = new Matrix(TEST_ARBITRAGE_SIZE, TEST_ARBITRAGE_SIZE);
        private Vector m_eigenValuesTest = new Vector(TEST_ARBITRAGE_SIZE);

        private ArbitrageCycleComparer ARBITRAGE_CYCLE_COMPARER = new ArbitrageCycleComparer();
        private SortedSet<ArbitrageCycle> m_activeArbitrage = new SortedSet<ArbitrageCycle>();
        private SortedSet<ArbitrageCycle> m_historicalArbitrage = new SortedSet<ArbitrageCycle>();

        public const int NO_SECURITY_DEF_FOUND = 200;
        public const int TEST_ARBITRAGE_SIZE = 6;
        public const double LAMBDA_MAX = 6.0221;

        #endregion

        #region Properties

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty MeanProperty =
            DependencyProperty.Register("Mean", typeof(double),
            typeof(MainWindow));

        public double Mean
        {
            get { return (double)GetValue(MeanProperty); }
            set { SetValue(MeanProperty, value); }
        }

        public static readonly DependencyProperty SDProperty =
            DependencyProperty.Register("SD", typeof(double),
            typeof(MainWindow));

        public double SD
        {
            get { return (double)GetValue(SDProperty); }
            set { SetValue(SDProperty, value); }
        }

        public static readonly DependencyProperty ALambdaMaxProperty =
            DependencyProperty.Register("ALambdaMax", typeof(double),
            typeof(MainWindow));

        public double ALambdaMax
        {
            get { return (double)GetValue(ALambdaMaxProperty); }
            set { SetValue(ALambdaMaxProperty, value); }
        }

        public static readonly DependencyProperty BLambdaMaxProperty =
            DependencyProperty.Register("BLambdaMax", typeof(double),
            typeof(MainWindow));

        public double BLambdaMax
        {
            get { return (double)GetValue(BLambdaMaxProperty); }
            set { SetValue(BLambdaMaxProperty, value); }
        }

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            m_tws.tickPrice += m_tws_tickPrice;
            m_tws.updateMktDepth += m_tws_updateMktDepth;
            m_tws.updateMktDepthL2 += m_tws_updateMktDepthL2;
            m_tws.errMsg += m_tws_errMsg;
            m_tws.currentTime += m_tws_currentTime;

            //TODO: hack grid lines colors
            var T = Type.GetType("System.Windows.Controls.Grid+GridLinesRenderer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            var GLR = Activator.CreateInstance(T);
            GLR.GetType().GetField("s_oddDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.Gray, 1.0));
            GLR.GetType().GetField("s_evenDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.Gray, 1.0));

            myGrid.ShowGridLines = true;

            myGrid.Visibility = Visibility.Hidden;

            FillTestData();
        }

        private DateTime m_currentTime = DateTime.Now;

        private void m_tws_currentTime(int time)
        {
            m_currentTime = new DateTime(time);
            m_currentTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            m_currentTime = m_currentTime.AddSeconds(time).ToLocalTime();
        }

        #endregion

        #region Members

        private void FillTestData()
        {
            //Eigen values
            m_eigenValuesTest[0] = 0.3802;
            m_eigenValuesTest[1] = 0.5548;
            m_eigenValuesTest[2] = 0.6881;
            m_eigenValuesTest[3] = 0.0035;
            m_eigenValuesTest[4] = 0.049;
            m_eigenValuesTest[5] = 0.2678;

            //A
            m_aTest[0, 0] = 1;
            m_aTest[0, 1] = 0.685636;
            m_aTest[0, 2] = 0.555772;
            m_aTest[0, 3] = 108.18;
            m_aTest[0, 4] = 7.8064;
            m_aTest[0, 5] = 1.4247;

            m_aTest[1, 0] = 1/m_aTest[0, 1];
            m_aTest[1, 1] = 1;
            m_aTest[1, 2] = 0.8113;
            m_aTest[1, 3] = 157.8017;
            m_aTest[1, 4] = 11.3894;
            m_aTest[1, 5] = 2.0789;

            m_aTest[2, 0] = 1 / m_aTest[0, 2];
            m_aTest[2, 1] = 1 / m_aTest[1, 2];
            m_aTest[2, 2] = 1;
            m_aTest[2, 3] = 200.35;
            m_aTest[2, 4] = 14.11;
            m_aTest[2, 5] = 2.5657;

            m_aTest[3, 0] = 1 / m_aTest[0, 3];
            m_aTest[3, 1] = 1 / m_aTest[1, 3];
            m_aTest[3, 2] = 1 / m_aTest[2, 3];
            m_aTest[3, 3] = 1;
            m_aTest[3, 4] = 0.074129;
            m_aTest[3, 5] = 0.013217;

            m_aTest[4, 0] = 1 / m_aTest[0, 4];
            m_aTest[4, 1] = 1 / m_aTest[1, 4];
            m_aTest[4, 2] = 1 / m_aTest[2, 4];
            m_aTest[4, 3] = 1 / m_aTest[3, 4];
            m_aTest[4, 4] = 1;
            m_aTest[4, 5] = 0.182582;

            m_aTest[5, 0] = 1 / m_aTest[0, 5];
            m_aTest[5, 1] = 1 / m_aTest[1, 5];
            m_aTest[5, 2] = 1 / m_aTest[2, 5];
            m_aTest[5, 3] = 1 / m_aTest[3, 5];
            m_aTest[5, 4] = 1 / m_aTest[4, 5];
            m_aTest[5, 5] = 1;


            //A2
            m_aTest2[5, 5] = 1;
            m_aTest2[5, 4] = 5.4845;
            m_aTest2[5, 3] = 77.32;
            m_aTest2[5, 2] = 0.389757;
            m_aTest2[5, 1] = 0.481348;
            m_aTest2[5, 0] = 0.702395;

            m_aTest2[4, 5] = 1 / m_aTest2[5, 4];
            m_aTest2[4, 4] = 1;
            m_aTest2[4, 3] = 14.35;
            m_aTest2[4, 2] = 0.070947;
            m_aTest2[4, 1] = 0.087828;
            m_aTest2[4, 0] = 0.128116;

            m_aTest2[3, 5] = 1 / m_aTest2[5, 3];
            m_aTest2[3, 4] = 1 / m_aTest2[4, 3];
            m_aTest2[3, 3] = 1;
            m_aTest2[3, 2] = 0.005199;
            m_aTest2[3, 1] = 0.00634;
            m_aTest2[3, 0] = 0.009248;

            m_aTest2[2, 5] = 1 / m_aTest2[5, 2];
            m_aTest2[2, 4] = 1 / m_aTest2[4, 2];
            m_aTest2[2, 3] = 1 / m_aTest2[3, 2];
            m_aTest2[2, 2] = 1;
            m_aTest2[2, 1] = 1.233198;
            m_aTest2[2, 0] = 1.7998;

            m_aTest2[1, 5] = 1 / m_aTest2[5, 1];
            m_aTest2[1, 4] = 1 / m_aTest2[4, 1];
            m_aTest2[1, 3] = 1 / m_aTest2[3, 1];
            m_aTest2[1, 2] = 1 / m_aTest2[2, 1];
            m_aTest2[1, 1] = 1;
            m_aTest2[1, 0] = 1.459;

            m_aTest2[0, 5] = 1 / m_aTest2[5, 0];
            m_aTest2[0, 4] = 1 / m_aTest2[4, 0];
            m_aTest2[0, 3] = 1 / m_aTest2[3, 0];
            m_aTest2[0, 2] = 1 / m_aTest2[2, 0];
            m_aTest2[0, 1] = 1 / m_aTest2[1, 0];
            m_aTest2[0, 0] = 1;            

            //B
            m_bTest[0, 0] = 1;
            m_bTest[0, 1] = 1111;
            m_bTest[0, 2] = 1111;
            m_bTest[0, 3] = 1111;
            m_bTest[0, 4] = 1111;
            m_bTest[0, 5] = 1111;

            m_bTest[1, 0] = 1111;
            m_bTest[1, 1] = 1;
            m_bTest[1, 2] = 1111;
            m_bTest[1, 3] = 1111;
            m_bTest[1, 4] = 1111;
            m_bTest[1, 5] = 1111;

            m_bTest[2, 0] = 1111;
            m_bTest[2, 1] = 1111;
            m_bTest[2, 2] = 1;
            m_bTest[2, 3] = 1111;
            m_bTest[2, 4] = 1111;
            m_bTest[2, 5] = 1111;

            m_bTest[3, 0] = 1111;
            m_bTest[3, 1] = 1111;
            m_bTest[3, 2] = 1111;
            m_bTest[3, 3] = 1;
            m_bTest[3, 4] = 1111;
            m_bTest[3, 5] = 1111;

            m_bTest[4, 0] = 1111;
            m_bTest[4, 1] = 1111;
            m_bTest[4, 2] = 1111;
            m_bTest[4, 3] = 1111;
            m_bTest[4, 4] = 1;
            m_bTest[4, 5] = 1111;

            m_bTest[5, 0] = 1111;
            m_bTest[5, 1] = 1111;
            m_bTest[5, 2] = 1111;
            m_bTest[5, 3] = 1111;
            m_bTest[5, 4] = 1111;
            m_bTest[5, 5] = 1;

            //C
            m_cTest[0, 0] = 1;
            m_cTest[0, 1] = 1111;
            m_cTest[0, 2] = 1111;
            m_cTest[0, 3] = 1111;
            m_cTest[0, 4] = 1111;
            m_cTest[0, 5] = 1111;

            m_cTest[1, 0] = 1111;
            m_cTest[1, 1] = 1;
            m_cTest[1, 2] = 1111;
            m_cTest[1, 3] = 1111;
            m_cTest[1, 4] = 1111;
            m_cTest[1, 5] = 1111;

            m_cTest[2, 0] = 1111;
            m_cTest[2, 1] = 1111;
            m_cTest[2, 2] = 1;
            m_cTest[2, 3] = 1111;
            m_cTest[2, 4] = 1111;
            m_cTest[2, 5] = 1111;

            m_cTest[3, 0] = 1111;
            m_cTest[3, 1] = 1111;
            m_cTest[3, 2] = 1111;
            m_cTest[3, 3] = 1;
            m_cTest[3, 4] = 1111;
            m_cTest[3, 5] = 1111;

            m_cTest[4, 0] = 1111;
            m_cTest[4, 1] = 1111;
            m_cTest[4, 2] = 1111;
            m_cTest[4, 3] = 1111;
            m_cTest[4, 4] = 1;
            m_cTest[4, 5] = 1111;

            m_cTest[5, 0] = 1111;
            m_cTest[5, 1] = 1111;
            m_cTest[5, 2] = 1111;
            m_cTest[5, 3] = 1111;
            m_cTest[5, 4] = 1111;
            m_cTest[5, 5] = 1;
        }

        private void m_tws_updateMktDepthL2(int id, int position, string marketMaker, int operation, int side, double price, int size)
        {
            
        }

        private void m_tws_updateMktDepth(int id, int position, int operation, int side, double price, int size)
        {
            
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

        private void File_Test_Clicked(object sender, RoutedEventArgs e)
        {
            CalulateTestArbitrage();
        }        

        private void RequestMarketData(params string[] symbols)
        {
            m_tickers = new ForexTicker[symbols.Length, symbols.Length];
            m_a = new Matrix(symbols.Length, symbols.Length); 
            m_b = new Matrix(symbols.Length, symbols.Length);
            m_c = new Matrix(symbols.Length, symbols.Length);

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

                        if (ticker.IsNormalized) m_tws.reqMktData2(ticker.Id, ticker.LocalSymbol, "CASH", "SMART", "IDEALPRO", ticker.LocalCurrency, Contract.GENERIC_TICK_TAGS, 0);
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

            ticker = m_tickers[j, i];
            if (ticker != null)
            {
                ticker.Price = price;
            }
        }

        private void WhileConnectedTick()
        {
            try
            {
                while (m_tws.serverVersion > 0)
                {
                    m_tws.reqCurrentTime();
                    CalulateArbitrage();

                    Thread.Sleep(50);
                }

                m_taskToken.Cancel();
                Disconnected(false);
            }
            catch (Exception ex)
            {
                StatusError(ex.Message);
            }
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

                //RequestMarketData("USD", "EUR", "JPY", "GBP", "AUD", "CHF", "CAD");
                RequestMarketData("USD", "EUR", "JPY", "CHF");
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

        private bool CanCalculateArbitrage()
        {
            return m_a != null && m_b != null && m_c != null;
        }

        #endregion

        #region Arbitrage A/B/C/SD

        private void CalulateArbitrage()
        {
            CalculateA();
            CalculateB();
            CalculateC();
            CalculateSD();
            CalculateShortestPath(); 
            CalculateHistorical();
        }

        private void CalulateTestArbitrage()
        {
            m_a = m_aTest2.Clone();
            m_b = new Matrix(TEST_ARBITRAGE_SIZE, TEST_ARBITRAGE_SIZE);
            m_c = new Matrix(TEST_ARBITRAGE_SIZE, TEST_ARBITRAGE_SIZE);

            CalculateB();
            CalculateC();
            CalculateSD();
            CalculateShortestPath();
            CalculateHistorical();
        }

        private void CalculateA()
        {
            if (!CanCalculateArbitrage()) return;
            
            Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < m_tickers.GetLength(0); i++)
                {
                    for (int j = 0; j < m_tickers.GetLength(1); j++)
                    {
                        ForexTicker tickerLeft = m_tickers[i, j];
                        ForexTicker tickerRight = m_tickers[j, i];
                        m_a[i, j] = tickerLeft != null && tickerRight != null ? tickerLeft.Price : 1;
                    }
                }
            });
        }

        private void CalculateB()
        {
            if (!CanCalculateArbitrage()) return;

            Eigen aE = new Eigen(m_a);

            CMatrix aECVec = aE.Eigenvectors;
            CVector aECVal = aE.Eigenvalues;

            double aLambdaMax = aECVal[0].Real;

            Vector aEVec = new Vector(aECVal.Length);

            for (int i = 0; i < aECVal.Length; i++)
            {
                aEVec[i] = aECVec[i, 0].Real;
            }

            for (int i = 0; i < m_a.Rows; i++)
            {
                for (int j = 0; j < m_a.Cols; j++)
                {
                    m_b[i, j] = aEVec[i] / aEVec[j];
                    //m_b[i, j] = (m_eigenValuesTest[i] / m_eigenValuesTest[j]);
                }
            }

            Eigen bE = new Eigen(m_b);

            CMatrix bECVec = bE.Eigenvectors;
            CVector bECVal = bE.Eigenvalues;

            double bLambdaMax = bECVal[0].Real;

            Dispatcher.Invoke(() =>
            {
                ALambdaMax = Math.Round(aLambdaMax, 5);
                BLambdaMax = Math.Round(bLambdaMax, 5);
            });
        }

        private void CalculateC()
        {
            if (!CanCalculateArbitrage()) return;

            for (int i = 0; i < m_b.Rows; i++)
            {
                for (int j = 0; j < m_b.Cols; j++)
                {
                    m_c[i, j] = m_a[i, j] / m_b[i, j];
                }
            }
        }

        private void CalculateSD()
        {
            int elements = m_c.Rows * m_c.Cols;
            double sum = 0;
            for (int i = 0; i < m_c.Rows; i++)
            {
                for (int j = 0; j < m_c.Cols; j++)
                {
                    sum += m_c[i, j];
                }
            }

            double mean = sum / elements;

            double sum2 = 0;
            for (int i = 0; i < m_c.Rows; i++)
            {
                for (int j = 0; j < m_c.Cols; j++)
                {
                    sum2 += Math.Pow(m_c[i, j] - mean, 2);
                }
            }

            double sd = Math.Sqrt(sum2 / elements);

            Dispatcher.Invoke(() =>
            {
                Mean = Math.Round(mean, 5);
                SD = Math.Round(sd, 5);
            });
        }

        #endregion

        #region Arbitrage ShortestPath/Historical

        private void CalculateShortestPath()
        {
            EdgeWeightedDigraph G = new EdgeWeightedDigraph(m_a.Rows);
            for (int v = 0; v < m_a.Rows; v++)
            {
                for (int w = 0; w < m_a.Cols; w++)
                {
                    DirectedEdge e = new DirectedEdge(v, w, -Math.Log(m_a[v, w]));                
                    G.addEdge(e);
                }
            }

            BellmanFordSP spt = new BellmanFordSP(G, 0);

            if (spt.negativeCycleCount > 2)
            {
                double stake = 25000;
                double arbitrage = 1;

                ArbitrageCycle cycle = new ArbitrageCycle(m_currentTime);
                
                foreach (DirectedEdge e in spt.negativeCycle)
                {
                    double weight = Math.Exp(-e.Weight);
                    Status(e.Pair + " " + weight);
                    arbitrage *= weight;
                    cycle.Edges.Add(new DirectedEdge(e.From, e.To, weight));
                }

                if (!m_activeArbitrage.Contains(cycle, ARBITRAGE_CYCLE_COMPARER))
                {
                    m_activeArbitrage.Add(cycle);
                    Status(cycle.Path + " added.");
                }

                Status("arbitrage(" + arbitrage + ") stake(" + stake + ") balance(" + (arbitrage * stake) + ") profit(" + Math.Round(((arbitrage * stake)/stake) - 1, 5) + "%)");
            }
            //else
            //{
            //    StatusError("no arbitrage opportunity");
            //}
        }

        private void CalculateHistorical()
        {
            for (int i = 0; i < m_activeArbitrage.Count; i++ )
            {
                ArbitrageCycle cycle = m_activeArbitrage.ToList()[i];
                cycle.Current = m_currentTime;
                for (int j = 0; j < cycle.Edges.Count; j++)
                {
                    DirectedEdge edge = cycle.Edges[j];
                    cycle.Edges[j] = new DirectedEdge(edge.From, edge.To, m_a[edge.From, edge.To]);
                }

                if (cycle.Profit < 0.001d)
                {
                    m_historicalArbitrage.Add(cycle);
                    m_activeArbitrage.Remove(cycle);
                }
            }
        }

        #endregion
    }
}

