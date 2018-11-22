using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//启动服务端Socket监听
namespace Lan
{
    class SocketListener
    {
        public Hashtable Connection = new Hashtable();

        public void StartListen()
        {
            try
            {
                //端口号、IP地址
                int port = 2000;
                string host = "127.0.0.1";
                IPAddress ip = IPAddress.Parse(host);
                IPEndPoint ipe = new IPEndPoint(ip, port);

                //创建一个Socket类
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                s.Bind(ipe);//绑定2000端口
                s.Listen(0);//开始监听

                ReceiveText("启动Socket监听...");

                while (true)
                {
                    Socket connectionSocket = s.Accept();//为新建连接创建新的Socket

                    ReceiveText("客户端[" + connectionSocket.RemoteEndPoint.ToString() + "]连接已建立...");

                    Connection gpsCn = new Connection(connectionSocket);
                    gpsCn.ReceiveTextEvent += new Connection.ReceiveTextHandler(ReceiveText);

                    Connection.Add(connectionSocket.RemoteEndPoint.ToString(), gpsCn);

                    //在新线程中启动新的socket连接，每个socket等待，并保持连接
                    Thread thread = new Thread(new ThreadStart(gpsCn.WaitForSendData));
                    thread.Name = connectionSocket.RemoteEndPoint.ToString();
                    thread.Start();
                }
            }
            catch (ArgumentNullException ex1)
            {
                ReceiveText("ArgumentNullException:" + ex1);
            }
            catch (SocketException ex2)
            {
                ReceiveText("SocketException:" + ex2);
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
