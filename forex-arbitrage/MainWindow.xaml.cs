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

        public MainWindow()
        {
            InitializeComponent();

            //TODO: hack grid lines colors
            var T = Type.GetType("System.Windows.Controls.Grid+GridLinesRenderer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            var GLR = Activator.CreateInstance(T);
            GLR.GetType().GetField("s_oddDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.Gray, 1.0));
            GLR.GetType().GetField("s_evenDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.Gray, 1.0));

            myGrid.ShowGridLines = true;

            myGrid.Visibility = Visibility.Hidden;
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

