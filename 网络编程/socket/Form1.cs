using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace socket
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Any;

                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(txtPort.Text));

                socketWatch.Bind(point);
                ShowMsg("监听成功");
                socketWatch.Listen(10);

                Thread th = new Thread(Listen);
                th.IsBackground = true;
                th.Start(socketWatch);
            }
            catch
            { }
            
        }

        void ShowMsg(string str)
        {
            txtLog.AppendText(str + "\r\n");
        }

        Socket socketSend;
        Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();
        void Listen(object o)
        {
            Socket socketWatch = o as Socket;
            while (true)
            {
                try
                {
                    socketSend = socketWatch.Accept();
                    dicSocket.Add(socketSend.RemoteEndPoint.ToString(), socketSend);
                    cboUsers.Items.Add(socketSend.RemoteEndPoint.ToString());
                    ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + "连接成功");

                    Thread th = new Thread(Recive);
                    th.IsBackground = true;
                    th.Start(socketSend);
                }
                catch
                {
                }

            }
        }

        void Recive(object o)
        {
            Socket socketSend = o as Socket;
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024 * 2];
                    byte[] endbuf = new byte[20];
                    
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    
                    string str = Encoding.UTF8.GetString(buffer, 0, r);
                    ShowMsg(socketSend.RemoteEndPoint + ":" + str);
                    if (buffer[0] == 4)
                    {
                        sendlist(socketSend);
                    }
                    else 
                    {
                        string[] arry = str.Split('&');
                        string strnew = arry[1] + "&" + socketSend.RemoteEndPoint;
                        byte[] sendb = System.Text.Encoding.UTF8.GetBytes(strnew);
                        //ShowMsg(arry[0]);
                        //ShowMsg(arry[1]);
                        dicSocket[arry[0]].Send(sendb);
                    }

                }
                catch
                { }
            }
        }
        /// <summary>
        /// 服务器给客户端发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            string str = txtMsg.Text;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
            List<byte> list = new List<byte>();
            list.Add(0);
            list.AddRange(buffer);
            byte[] newBuffer = list.ToArray();
            string ip = cboUsers.SelectedItem.ToString();
            dicSocket[ip].Send(newBuffer);

        }

        private void btnZD_Click(object sender, EventArgs e)
        {
            byte[] buffer = new byte[1];
            buffer[0] = 2;
            dicSocket[cboUsers.SelectedItem.ToString()].Send(buffer);
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = @"C:\Users";
            ofd.Title = "请选择要发送的文件";
            ofd.Filter = "所有文件|*.*";
            ofd.ShowDialog();

            txtPath.Text = ofd.FileName;
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            string path = txtPath.Text;
            using (FileStream fsRead = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024 * 1024 * 5];
                int r = fsRead.Read(buffer, 0, buffer.Length);
                List<byte> list = new List<byte>();
                list.Add(8);
                list.AddRange(buffer);
                byte[] newbuffer = list.ToArray();
                dicSocket[cboUsers.SelectedItem.ToString()].Send(newbuffer, 0, r + 1, SocketFlags.None);
            }
        }
        void sendlist(Socket socketlist)
        {
            List<string> list = new List<string>();
            foreach (var item in dicSocket)
            {
                list.Add(item.Value.RemoteEndPoint.ToString());
            }
            string[] str = list.ToArray();
            byte[] buffer;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter wr = new BinaryWriter(stream))
                {
                    for (int i = 0; i < str.Length; i++)
                    {
                        wr.Write(str[i]);
                    }
                }
                buffer = stream.ToArray();
            }
            socketlist.Send(buffer,0,buffer.Length,SocketFlags.None);
        }
    }
}
