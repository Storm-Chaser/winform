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
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Socket socketSend;
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse(txtServer.Text);
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(txtPort.Text));
                socketSend.Connect(point);
                ShowMsg("连接成功");

                Thread th = new Thread(Recive);
                th.IsBackground = true;
                th.Start();
            }
            catch
            { }
            
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string str =this.listBox1.SelectedItem.ToString()+"&*";
            str +=txtMsg.Text.Trim();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);

            socketSend.Send(buffer);
        }

        void Recive()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 2];
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    if (buffer[0] == 0)
                    {
                        string str = Encoding.UTF8.GetString(buffer, 1, r - 1);
                        ShowMsg(socketSend.RemoteEndPoint + ":" + str);
                    }
                    else if (buffer[0] == 8)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.InitialDirectory = @"C:\Users";
                        sfd.Title = "选择保存路径";
                        sfd.Filter = "所有文件|*.*";
                        sfd.ShowDialog(this);
                        string path = sfd.FileName;
                        using (FileStream fsWrite = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            fsWrite.Write(buffer, 1, r - 1);
                        }
                        MessageBox.Show("保存成功");
                    }
                    else if (buffer[0] == 2)
                    {
                        for (int i = 0; i < 500; i++)
                        {
                            this.Location = new Point(200, 200);
                            this.Location = new Point(280, 280);
                        }
                    }
                    else if (buffer[0] == '*')
                    {
                        string str = Encoding.UTF8.GetString(buffer, 1, r-1);
                        string[] struse = str.Split('&');
                        byte[] newbuffer = System.Text.Encoding.UTF8.GetBytes(struse[0]);
                        ShowMsg(struse[1] + ":" + struse[0]);
                    }
                    else
                    {
                        listBox1.Items.Clear();
                        List<string> list = new List<string>();
                        string[] str;
                        using (MemoryStream stream = new MemoryStream(buffer))
                        {
                            using (BinaryReader rd = new BinaryReader(stream))
                            {
                                while (stream.Position < stream.Length)
                                {
                                    list.Add(rd.ReadString());
                                }
                            }
                            str = list.ToArray();
                        }
                        for (int i = 0; i < r; i++)
                        {
                            listBox1.Items.Add(str[i]);
                        }

                    }
                    
                }
                catch
                { }
                
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;

        }

        void ShowMsg(string str)
        {
            txtLog.AppendText(str + "\r\n");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] buffer = new byte[1];
            buffer[0] = 4;
            socketSend.Send(buffer);
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            int index = this.listBox1.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                txtLog.AppendText("向"+this.listBox1.SelectedItem.ToString()+"用户发送消息：");
            }
            
        }

        private void txtMsg_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
