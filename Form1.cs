using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace chatServer
{
    public partial class Form1 : Form
    {
        private string IP;
        private byte[] _buffer = new byte[1024];//verinin boyutu
        public List<SocketMsg> _clientSockets { get; set; }
        List<string> _names = new List<string>();
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static string terminated_client = "";
        public Form1()
        {
            InitializeComponent();
            findIP();
            _clientSockets = new List<SocketMsg>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void btnStartServer_Click(object sender, EventArgs e)
        {
        }
        private void btnServerStop_Click(object sender, EventArgs e)
        {
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
        }


        #region Server
        private void setup()
        {
            label4.Text = "Server has started...";
            _serverSocket.Bind(new IPEndPoint(IPAddress.Parse(IP), 100));
            _serverSocket.Listen(1);

            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }

        private void AppceptCallback(IAsyncResult ar)
        {
            Socket socket = _serverSocket.EndAccept(ar);
            _clientSockets.Add(new SocketMsg(socket));
            listUsers.Items.Add(socket.RemoteEndPoint.ToString());

            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);

            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            if (socket.Connected)
            {
                int received;
                try
                {
                    received = socket.EndReceive(ar);//Check client still exists if client has disconnected catch and remove from lists
                }
                catch (Exception)
                {
                    foreach (SocketMsg msg in _clientSockets)
                    {
                        if (msg._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            terminated_client = msg._Name.Substring(1, msg._Name.Length - 1);
                            _clientSockets.Remove(msg);
                            label6.Text = _clientSockets.Count.ToString();
                            for (int j = 0; j < listUsers.Items.Count; j++)
                            {
                                if (listUsers.Items[j].Equals(terminated_client))
                                {
                                    listUsers.Items.RemoveAt(j);
                                }
                            }
                        }
                    }

                    return;
                }
            }
        }
       
        //if pc connected to network server IP = ; no-network localhost
        private void findIP()
        {
            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ip in localIP)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    IP = ip.ToString();
            }
        }
        #endregion
    }
}