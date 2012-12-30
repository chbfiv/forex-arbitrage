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
using System.Windows.Shapes;

namespace forex_arbitrage
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow : Window
    {
        public ConnectWindow()
        {
            InitializeComponent();

            Host = "127.0.0.1";
            Port = 7496;
            ClientId = 1;
        }

        public static readonly DependencyProperty HostProperty =
            DependencyProperty.Register("Host", typeof(String),
            typeof(ConnectWindow));

        public String Host
        {
            get { return (String)GetValue(HostProperty); }
            set { SetValue(HostProperty, value); }
        }

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(int),
            typeof(ConnectWindow));

        public int Port
        {
            get { return (int)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        public static readonly DependencyProperty ClientIdProperty =
            DependencyProperty.Register("ClientId", typeof(int),
            typeof(ConnectWindow));

        public int ClientId
        {
            get { return (int)GetValue(ClientIdProperty); }
            set { SetValue(ClientIdProperty, value); }
        }

        private void ConnectClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public bool Prompt()
        {
            bool? result = ShowDialog();
            if (result.HasValue && result.Value)
            {
                return true;
            }
            return false;
        }
    }
}
