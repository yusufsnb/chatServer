using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace chatServer
{
    public partial class Form1 : Form
    {
        #region variables
        private string IP;
        private int PORT;

        public List<SocketMsg> _clientSockets { get; set; }
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private byte[] _buffer = new byte[1024];

        private string chatStoreFile = "Chat.txt";
        static string terminated_client = "";
        StreamWriter sw;

        #endregion

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            findIP();
            _clientSockets = new List<SocketMsg>();
        }

        #region formEvents

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
                    createFile();
                }
                catch (Exception ex) { }
            }
            btnStartServer.Enabled = false;
        }
        private void btnSend_Click(object sender, EventArgs e) //Choose one or multiple client and send message
        {
            if (listUsers.SelectedItems.Count == 0)
                MessageBox.Show("Please select client from list");
            else
            {
                for (int i = 0; i < listUsers.SelectedItems.Count; i++)
                {
                    string t = listUsers.SelectedItems[i].ToString();
                    for (int j = 0; j < _clientSockets.Count; j++)
                    {
                        if (_clientSockets[j]._Socket.Connected && _clientSockets[j]._Name.Equals("@" + t))
                            Sendata(_clientSockets[j]._Socket, txtMsg.Text);
                    }
                }
                txtMessages.AppendText("\nServer: " + txtMsg.Text + "\n");
                saveChat("Server", listUsers.SelectedItem.ToString(), txtMsg.Text);
                txtMsg.Text = string.Empty;
            }
        }

        #endregion

        #region Server
        //Server setup
        private void setup()
        {
            label4.Text = "Runnning";
            _serverSocket.Bind(new IPEndPoint(IPAddress.Parse(IP), PORT));
            _serverSocket.Listen(1);

            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        #region Callbacks
        private void AcceptCallback(IAsyncResult ar) //Accepting socket comes here added to sockets
        {
            Socket socket = _serverSocket.EndAccept(ar);
            _clientSockets.Add(new SocketMsg(socket));
            listUsers.Items.Add(socket.RemoteEndPoint.ToString());

            //Run as a async and wait callback from socket
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);

            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
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
                    for (int i = 0; i < _clientSockets.Count; i++)
                    {
                        if (_clientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            terminated_client = _clientSockets[i]._Name.Substring(1, _clientSockets[i]._Name.Length - 1);
                            _clientSockets.RemoveAt(i);
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

                    string text = Encoding.UTF8.GetString(dataBuf);
                    Debug.WriteLine("receiving message " + text);
                    string reponse = string.Empty;

                    //If data contains filtering filter and send
                    if (text.Contains("filtering**"))
                    {
                        SendFilterData(socket, allChatFiltering(text, findClientSocket(socket)));
                    }
                    else //Sending message or client list
                    {
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
                        string recCli;

                        string message = string.Empty;
                        int length = (text.Length) - (index + 2);
                        index += 2;
                        message = text.Substring(index, length);

                        send_receiving_messages(cli, text, message);

                        recCli = text.Substring(0, index - 2);
                        message = message.Substring(0, message.IndexOf("*"));

                        for (int i = 0; i < _clientSockets.Count; i++)
                        {
                            if (socket.RemoteEndPoint.ToString().Equals(_clientSockets[i]._Socket.RemoteEndPoint.ToString()))
                            {
                                txtMessages.AppendText($"\n {_clientSockets[i]._Name.Substring(1)} --> {recCli} {message} \n");
                                saveChat(_clientSockets[i]._Name.Substring(1), recCli, message);

                            }
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
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        private void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        #endregion

        private void removeFromClients(string term_cli) //Removing disconnected client
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

        //Send receiving client message to other client
        public void send_receiving_messages(string cli, string text, string message)
        {
            cli = "@" + cli;

            int index = (message.IndexOf("*") + 1);
            string piece = message.Substring(index, message.Length - index);
            string mess = message.Substring(0, (index - 1));
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
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        void SendFilterData(Socket socket, List<string> filteringMsgs)
        {
            string text = "filtering**" + String.Join("\n", filteringMsgs);
            byte[] data = Encoding.UTF8.GetBytes(text);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }



        #region Configs
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

        private string findClientSocket(Socket socket)
        {
            string clientName = string.Empty;
            for (int i = 0; i < _clientSockets.Count; i++)
            {
                if (socket.RemoteEndPoint.ToString().Equals(_clientSockets[i]._Socket.RemoteEndPoint.ToString()))
                {
                    clientName = _clientSockets[i]._Name.Substring(1);
                }
            }
            return clientName;
        }

        #endregion

        #endregion

        #region Store

        private void createFile()
        {

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), chatStoreFile);
            using (StreamWriter sw = File.CreateText(path)) ;
        }

        private void saveChat(string sendingCli, string receivingCli, string message)
        {
            sw = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), chatStoreFile), true);
            sw.WriteLine($"{sendingCli}\t{receivingCli}\t{message}");
            sw.Close();
        }
        #endregion

        #region Filtering

        private List<string> allChatFiltering(string text, string clientName)
        {
            string[] filteringOptions = new string[6];
            filteringOptions = (text.Substring(text.IndexOf("**") + 3).Split(","));
            List<string> msgList = new List<string>();
            msgList = File.ReadLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), chatStoreFile)).ToList();
            // From-To filter
            msgList = filterWithSendTo(msgList, clientName, bool.Parse(filteringOptions[0]), bool.Parse(filteringOptions[1]));
            if (bool.Parse(filteringOptions[2])) // Last X messages filter
                msgList = filterWithLastXMessages(msgList, int.Parse(filteringOptions[3]));
            if (bool.Parse(filteringOptions[4])) // Contains filter
                msgList = filterWithContains(msgList, filteringOptions[5].ToString());
            return msgList;

        }

        //Filter all messages from me or to me
        private List<string> filterWithSendTo(List<string> msgList, string clientName, bool filteringOn, bool filter)
        {
            List<string> filteringList = new List<string>();
            foreach (string msg in msgList)
            {
                string[] values = msg.Split("\t");
                if (filteringOn)
                {
                    if (!filter)
                    {
                        if (values[0].Equals(clientName))
                            filteringList.Add(msg);
                    }
                    else
                    {
                        if (values[1].Equals(clientName))
                            filteringList.Add(msg);
                    }
                }
                else // If filtering off
                {
                    if (values[0].Equals(clientName) || values[1].Equals(clientName))
                        filteringList.Add(msg);
                }
            }
            return filteringList;
        }

        private List<string> filterWithLastXMessages(List<string> msgList, int count)
        {
            List<string> filteringList = new List<string>();
            if (count > filteringList.Count)
                count = filteringList.Count;
            filteringList = msgList.TakeLast(count).ToList();
            return filteringList;
        }

        private List<string> filterWithContains(List<string> msgList, string contains)
        {
            List<string> filteringList = new List<string>();
            foreach (string msg in msgList)
            {
                string[] values = msg.Split('\t');
                if (values[2].Contains(contains))
                    filteringList.Add(msg);
            }
            return filteringList;
        }
        #endregion
    }
}