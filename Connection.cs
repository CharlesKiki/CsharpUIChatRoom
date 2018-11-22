using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
//服务端用于接收消息，处理具体的连接
namespace Lan
{
    class Connection
    {
        Socket _connection;

        public Connection(Socket socket)
        {
            _connection = socket;
        }

        public void WaitForSendData()
        {
            while (true)
            {
                byte[] bytes = new byte[1024];
                string data = "";

                //等待接收消息
                int bytesRec = this._connection.Receive(bytes);

                if (bytesRec == 0)
                {
                    ReceiveText("客户端[" + _connection.RemoteEndPoint.ToString() + "]连接关闭...");
                    break;
                }

                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                ReceiveText("收到消息：" + data);

                string sendStr = "服务端已经收到信息！";
                byte[] bs = Encoding.UTF8.GetBytes(sendStr);
                _connection.Send(bs, bs.Length, 0);
            }
        }

        public delegate void ReceiveTextHandler(string text);
        public event ReceiveTextHandler ReceiveTextEvent;
        private void ReceiveText(string text)
        {
            if (ReceiveTextEvent != null)
            {
                ReceiveTextEvent(text);
            }
        }
    }
}
