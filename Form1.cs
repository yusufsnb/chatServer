using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace chatServer
{
    public partial class Form1 : Form
    {
        private string IP;
        private int PORT;
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
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtIP.Text = IP;
        }
        private void btnStartServer_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPort.Text))
            {
                try
                {
                    PORT = int.Parse(txtPort.Text);
                    setup();
                }
                catch (Exception ex) { }
            }
        }
        private void btnServerStop_Click(object sender, EventArgs e)
        {
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listUsers.SelectedItems.Count; i++)
            {
                string t = listUsers.SelectedItems[i].ToString();
                for (int j = 0; j < _clientSockets.Count; j++)
                {
                    if (_clientSockets[j]._Socket.Connected && _clientSockets[j]._Name.Equals("@" + t))

                    {
                        Sendata(_clientSockets[j]._Socket, txtMsg.Text);//mesajý gönder
                    }
                }
            }
            txtMessages.AppendText("\nServer: " + txtMsg.Text);
        }


        #region Server
        private void setup()
        {
            label4.Text = "Runnning";
            _serverSocket.Bind(new IPEndPoint(IPAddress.Parse(IP), PORT));
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

                    removeFromClients(terminated_client);
                    return;
                }
                if (received != 0)
                {
                    byte[] dataBuf = new byte[received];

                    Array.Copy(_buffer, dataBuf, received);

                    string text = Encoding.ASCII.GetString(dataBuf);
                    Debug.WriteLine("receiving message " + text);
                    string reponse = string.Empty;

                    if (text.Contains("@@"))
                    {
                        for (int i = 0; i < listUsers.Items.Count; i++)
                        {
                            if (socket.RemoteEndPoint.ToString().Equals(_clientSockets[i]._Socket.RemoteEndPoint.ToString()))
                            {
                                listUsers.Items.RemoveAt(i);
                                listUsers.Items.Insert(i, text.Substring(1, text.Length - 1));
                                _clientSockets[i]._Name = text;
                                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                                sendNicknames();

                                return;
                            }
                        }
                    }
                    int index = text.IndexOf(" ");
                    string cli = text.Substring(0, index);

                    string message = string.Empty;
                    int length = (text.Length) - (index + 2);
                    index = index + 2;
                    message = text.Substring(index, length);
                    send_receiving_messages(cli, text, message);
                    for (int i = 0; i < _clientSockets.Count; i++)
                    {
                        if (socket.RemoteEndPoint.ToString().Equals(_clientSockets[i]._Socket.RemoteEndPoint.ToString()))
                        {
                            txtMessages.AppendText("\n" + _clientSockets[i]._Name + ": " + text);

                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _clientSockets.Count; i++)
                    {
                        if (_clientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            _clientSockets.RemoveAt(i);
                            label6.Text = _clientSockets.Count.ToString();
                        }
                    }
                }
            }
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);//almaya baþla soketten ReceiveCallback recursive
        }

        private void removeFromClients(string term_cli)
        {
            string remove = "remove*" + term_cli;
            for (int j = 0; j < _clientSockets.Count; j++)
            {
                if (_clientSockets[j]._Socket.Connected)
                {
                    Sendata(_clientSockets[j]._Socket, remove);
                    Thread.Sleep(20);
                }
            }
        }
        public void sendNicknames()
        {
            for (int j = 0; j < _clientSockets.Count; j++)//soket sayýsý kadar dön
            {
                if (_clientSockets[j]._Socket.Connected)//soket baglýysa
                {
                    for (int i = 0; i < listUsers.Items.Count; i++)//listedeki isimler kadar
                    {


                        Sendata(_clientSockets[j]._Socket, listUsers.Items[i].ToString());

                        Thread.Sleep(20);
                    }

                }
            }
        }

        public void send_receiving_messages(string cli, string text, string message)
        {
            string parcc = text.Substring(2, 2);
            cli = "@" + cli;


            int ind__ = (message.IndexOf("*") + 1);
            string piece = message.Substring(ind__, message.Length - ind__);
            string mess = message.Substring(0, (ind__ - 1));
            string send = piece + ": " + mess;

            try
            {
                for (int j = 0; j < _clientSockets.Count; j++)
                {
                    if (_clientSockets[j]._Socket.Connected)
                    {
                        if (_clientSockets[j]._Name.Equals(cli))
                        {
                            Sendata(_clientSockets[j]._Socket, send);
                            Thread.Sleep(20);
                        }

                    }
                }
            }
            catch (Exception e)
            {
            }
        }
        void Sendata(Socket socket, string msg)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }

        private void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
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