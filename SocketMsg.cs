using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace chatServer
{
    public class SocketMsg
    {
        public Socket _Socket { get; set; }
        public string _Name { get; set; }
        public SocketMsg(Socket socket)
        {
            this._Socket = socket;
        }
    }
}
