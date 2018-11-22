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
using System.Windows.Navigation;
using System.Windows.Shapes;
//添加Socket类
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Threading;

//软件目的，v0.0.1 支持一对一的进行连接 并发送
//每个人的机器相当于一个服务器 和 客户端 （防止一个人只能等待连接，一个人只能发送连接），UDP来代替这个？组播发送？
//后期需要给这个软件分离一些逻辑，修改起来太麻烦
//任意一个人可以发起连接请求
//逻辑，Window控件自动的加载服务端口，绑定一个空闲的端口
//每个人只试图连接其他人，这个时候抽象服务器，除了连接成功之后，发送的两个人的信息，其他的都不需要

namespace Lan
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //客户端变量
        Socket c;
        //服务端变量，监听Socket这个只是声明，但是没有初始化
        SocketListener listener;

        //初始化服务器
        //Server LanServer = new Server();
        public MainWindow()
        {
            InitializeComponent();
            //服务器启动
            //LanServer.ServerStart();

            InitServer();
            //添加逻辑，自动的开启服务器监听
            //增加一个线程，替代button
            Thread ActiveSocketThread = new Thread(new ThreadStart(SocketListen));
            ActiveSocketThread.Start();
            InitClient();

        }
        //这是一个委托对象，这个对象接受一个string类型
        public delegate void ShowTextHandler(string text);
        //声明这个委托对象，但是这个对象还没有被初始化，此时相当于一个指针，可以用null来检查
        //有没有初始化
        ShowTextHandler setText;
        private void ShowText(string text)
        {
            //Bug：：：服务器信息逻辑，这个函数涉及了很有意思的线程问题。
            if (System.Threading.Thread.CurrentThread != richTextBox.Dispatcher.Thread)
                //系统当前线程不是对话窗口控件线程
            {
                if (setText == null)
                    //如果当前的线程不对，并且委托还没有初始化
                {
                    setText = new ShowTextHandler(ShowText);
                    //嵌套耶，初始化这个委托
                }
                richTextBox.Dispatcher.BeginInvoke(setText, DispatcherPriority.Normal, new string[] { text });
                //控件的线程进行调整，让它执行
            }
            else
            //当前就在执行控件线程
            {
                //控件的内容变动
                richTextBox.AppendText(text + "\n");
            }
        }

        private void InitServer()
            //该段代码的含义是定期的检查连接。连接的逻辑在其他函数
        {
            System.Timers.Timer t = new System.Timers.Timer(2000);
            //实例化Timer类，设置间隔时间为5000毫秒；
            //Bug：注意事件的绑定
            t.Elapsed += new System.Timers.ElapsedEventHandler(CheckListen);
            //到达时间的时候执行检查连接事件，函数可以不写小括号
            t.AutoReset = true;
            t.Start();
        }
        private void CheckListen(object sender, System.Timers.ElapsedEventArgs e)
            //检查当前是否存在连接
        {
            if (listener != null && listener.Connection != null)
                //只要监听对象存在，并且存在正在连接的线程
            {
                //ConnectingCount.Content = listener.Connection.Count.ToString();
                //ShowText("连接数：" + listener.Connection.Count.ToString());
                //不想一直让他显示1 太烦心了
                //非常有趣，ShowText通过某种方法可以直接显示到控件中
                //listener可以同时监听多线程。
            }
        }
        private void SocketListen()
            //监听函数，该函数触发信息发送，这里初始化监听对象
        {
            listener = new SocketListener();
            //创建新的监听对象
            //Bug：注意事件的绑定
            listener.ReceiveTextEvent += new SocketListener.ReceiveTextHandler(ShowText);
            //当监听对象接收到信息，触发ShowText事件。
            listener.StartListen();
            //开始监听
        }
        //*******************************此处是客户端
        private void InitClient()
        {
            //添加Button，点击才能连接到需要的IP上
            int port = 2000;
            string host = "127.0.0.1";
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, port);//把ip和端口转化为IPEndPoint实例
            c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建一个Socket

            ShowText("客户端（这里应该是谁）连接到Socket服务（谁）...");
            //似乎是Winform？

            c.Connect(ipe);//连接到服务器
        }



        //发送信息按钮逻辑，服务器逻辑也被写在这里了
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //bug：修复Show在TextList的显示，下面这行是测试
                richTextBox.AppendText("Test发送消息到服务端..." + "\n");
                ShowText("发送消息到服务端...");
                string sendStr = MessageBox.Text;
                byte[] bs = Encoding.ASCII.GetBytes(sendStr);
                c.Send(bs, bs.Length, 0);

                string recvStr = "";
                byte[] recvBytes = new byte[1024];
                int bytes;
                bytes = c.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息
                recvStr += Encoding.UTF8.GetString(recvBytes, 0, bytes);

                richTextBox.AppendText("服务器返回信息：" + recvStr + "\n");
                ShowText("服务器返回信息：" + recvStr);
                
            }
            catch (ArgumentNullException ex1)
            {
                //很奇怪这里可能有Console吗？应该是没有的啊
                Console.WriteLine("ArgumentNullException:{0}", ex1);
            }
            catch (SocketException ex2)
            {
                Console.WriteLine("SocketException:{0}", ex2);
            }

        }
        //暂时改名
        private void ClientShowText(string text)
            //
        {
            //注意，富文本才能使用AppendText
            richTextBox.AppendText(text + "\n");
        }

        //如果我一不小心增加了一个事件，删除代码处的，还应该删除那里的？
        private void richTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
