using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

//Implements a sample socket client to connect to the server implmented in the AsyncSocketServer project
//Usage: AsyncSocketClient.exe <destination IP address> <destination port number>

//Destination IP address: The IP Address of the server to connect to
//Destination Port Number: The port number to connect to

namespace forex_arbitrage
{
    public class GainCapitalUserToken
    {
        private Socket m_socket;

        public Socket Socket
        {
            get { return m_socket; }
            //set { m_socket = value; }
        }

        private string m_incomplete;

        public string Incomplete
        {
            get { return m_incomplete; }
            set { m_incomplete = value; }
        }

        public GainCapitalUserToken(Socket socket)
        {
            m_socket = socket;
        }
    }

    public class GainCapitalClient
    {
        static ManualResetEvent clientDone = new ManualResetEvent(false);
        private SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
        private IPAddress m_ipAddress = IPAddress.Loopback;
        private int m_port = 4444;
        private string m_token = string.Empty;
        private Socket m_socket;
        private Dictionary<int, int> m_currencyMap = new Dictionary<int, int>();

        public delegate void RateTickDelegate(int id, DateTime dt, double bid, double ask, double price, bool dealable);
        public event RateTickDelegate RateTick;
        public delegate void SocketDelegate(IPEndPoint endpoint);
        public event SocketDelegate Connected;
        public event SocketDelegate Disconnected;
        public event SocketDelegate Lost;

        public bool IsConnected
        {
            get
            {
                return m_socket != null && m_socket.Connected;
            }
        }

        public GainCapitalClient()
        {

        }

        public GainCapitalClient(string host, int port, string token)
        {
            m_ipAddress = Dns.Resolve(host).AddressList[0];
            m_port = port;
            m_token = token;
        }

        public GainCapitalClient(IPAddress ipAddress, int port, string token)
        {
            m_ipAddress = ipAddress;
            m_port = port;
            m_token = token;
        }

        public void Connect()
        {
            Disconnect();

            // Create a socket and connect to the server
            m_socket = new Socket(m_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            socketEventArg.RemoteEndPoint = new IPEndPoint(m_ipAddress, m_port);
            socketEventArg.UserToken = new GainCapitalUserToken(m_socket);
            m_socket.ConnectAsync(socketEventArg);
        }

        public void Connect(IPAddress ipAddress, int port, string token)
        {
            m_ipAddress = ipAddress;
            m_port = port;
            m_token = token;
            Connect();
        }

        public void Connect(String host, int port, string token)
        {
            m_ipAddress = Dns.Resolve(host).AddressList[0];
            m_port = port;
            m_token = token;
            Connect();
        }

        public void Disconnect()
        {
            if (m_socket != null)
            {
                m_socket.Shutdown(SocketShutdown.Send);
                m_socket.Close();
                m_socket = null;
            }
        }

        /// <summary>
        /// A single callback is used for all socket operations. This method forwards execution on to the correct handler 
        /// based on the type of completed operation
        /// </summary>
        private void SocketEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e, m_token);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new Exception("Invalid operation completed");
            }
        }

        /// <summary>
        /// Called when a ConnectAsync operation completes
        /// </summary>
        private void ProcessConnect(SocketAsyncEventArgs e, string token)
        {
            if (e.SocketError == SocketError.Success)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(token + "\r");

                e.SetBuffer(buffer, 0, buffer.Length);
                Socket sock = ((GainCapitalUserToken)e.UserToken).Socket;

                SocketDelegate temp = Connected;
                if (temp != null)
                    temp((IPEndPoint)sock.RemoteEndPoint);

                bool willRaiseEvent = sock.SendAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessSend(e);
                }
            }
            else
            {
                throw new SocketException((int)e.SocketError);
            }
        }

        /// <summary>
        /// Called when a ReceiveAsync operation completes
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                String buffer = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
                //Console.WriteLine("Received from server: {0}", buffer);

                GainCapitalUserToken token = (GainCapitalUserToken)e.UserToken;

                String messageGroup = token.Incomplete != null ? token.Incomplete + buffer : buffer;

                if (!messageGroup.EndsWith("\r"))
                {
                    int index = messageGroup.LastIndexOf('\r');
                    if (index >= 0)
                    {
                        token.Incomplete = messageGroup.Substring(index);
                        messageGroup = messageGroup.Substring(0, index);
                    }
                    else
                    {
                        token.Incomplete = messageGroup;
                        messageGroup = String.Empty;
                    }
                }
                else
                {
                    token.Incomplete = null;
                }

                String[] messages = messageGroup.Split(new char[] {'\r'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (String message in messages)
                {
                    Console.WriteLine("message: {0}", message);

                    if (message.StartsWith("S")) ProccessSMessage(message.Substring(1));
                    if (message.StartsWith("R")) ProccessRMessage(message.Substring(1));
                }

                //Read data sent from the server
                Socket sock = ((GainCapitalUserToken)e.UserToken).Socket;
                bool willRaiseEvent = sock.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                throw new SocketException((int)e.SocketError);
            }
        }

        /// <summary>
        /// Called when a SendAsync operation completes
        /// </summary>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Console.WriteLine("Sent 'Hello World' to the server");

                //Read data sent from the server
                Socket sock = ((GainCapitalUserToken)e.UserToken).Socket;
                bool willRaiseEvent = sock.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                throw new SocketException((int)e.SocketError);
            }
        }

        private void ProccessSMessage(String message)
        {
            string[] rateMessages = message.Split(new char[] {'$'}, StringSplitOptions.RemoveEmptyEntries);

            foreach (string rateMessage in rateMessages)
            {
                string[] rateFields = rateMessage.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

                int id = Convert.ToInt32(rateFields[0]);
                String pair = rateFields[1];
                double bid = Convert.ToDouble(rateFields[2]);
                double ask = Convert.ToDouble(rateFields[3]);

                try
                {
                    int i = (int)Enum.Parse(typeof(Currencies), pair.Split('/')[0]);
                    int j = (int)Enum.Parse(typeof(Currencies), pair.Split('/')[1]);
                    m_currencyMap[id] = (i << 0) | (j << 16);
                }
                catch
                {
                    
                }
            }
        }

        private void ProccessRMessage(String message)
        {
            string[] rateFields = message.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            if (rateFields.Length == 5)
            {
                int id = Convert.ToInt32(rateFields[0]);
                double bid = Convert.ToDouble(rateFields[1]);
                double ask = Convert.ToDouble(rateFields[2]);
                double price = (bid + ask)/2;
                bool dealable = rateFields[3] == "D";
                DateTime dt = DateTime.Parse(rateFields[4]);

                if (m_currencyMap.ContainsKey(id))
                {
                    RateTickDelegate temp = RateTick;
                    if (temp != null)
                        temp(m_currencyMap[id], dt, bid, ask, price, dealable);
                }
            }
        }
    }
}
