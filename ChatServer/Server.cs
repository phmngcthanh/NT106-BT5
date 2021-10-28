using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net.Sockets;
using System.Net;
using System.Collections;

using ChatApplication;

namespace ChatServer
{
    public partial class Server : Form
    {
        #region Private Members

// struct lưu giữ thông tin
        private struct Client
        {
            public EndPoint endPoint;
            public string name;
            
        }

         private ArrayList clientList;

         private Socket serverSocket;

         private byte[] dataStream = new byte[1024];

        // workaround để xử lý lỗi 
        private delegate void UpdateStatusDelegate(string status);
        private UpdateStatusDelegate updateStatusDelegate = null;

        #endregion

        #region Constructor

        public Server()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        private void Server_Load(object sender, EventArgs e)
        {
            try
            {
                this.clientList = new ArrayList();

                this.updateStatusDelegate = new UpdateStatusDelegate(this.UpdateStatus);

                // Init socket
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // listen port 30000
                IPEndPoint server = new IPEndPoint(IPAddress.Any, 30000);

                serverSocket.Bind(server);

                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);

                EndPoint epSender = (EndPoint)clients;

                serverSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(ReceiveData), epSender);

                lblStatus.Text = "Listening";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error";
                MessageBox.Show("Load Error: " + ex.Message, "UDP Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region Send And Receive

        public void SendData(IAsyncResult asyncResult)
        {
            try
            {
                serverSocket.EndSend(asyncResult);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("SendData Error: " + ex.Message, "UDP Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReceiveData(IAsyncResult asyncResult)
        {
            try
            {
                byte[] data;

                Packet receivedData = new Packet(this.dataStream);
                Packet sendData = new Packet();
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)clients;

                serverSocket.EndReceiveFrom(asyncResult, ref epSender);

                sendData.ChatDataIdentifier = receivedData.ChatDataIdentifier;
                sendData.ChatName = receivedData.ChatName;
                sendData.ChatDest = receivedData.ChatDest;
                
                switch (receivedData.ChatDataIdentifier)
                {
                    case DataIdentifier.Message:
                        sendData.ChatMessage = string.Format("{0}", receivedData.ChatMessage);
                        break;

                    case DataIdentifier.LogIn:
                        Client client = new Client();
                        client.endPoint = epSender;
                        client.name = receivedData.ChatName;
                        bool vaildloginname = true;
                        foreach (Client c in this.clientList)
                           
                        {
                            if (c.name==client.name)
                            {
                                sendData.ChatName = string.Format("SERVER:");
                                sendData.ChatMessage = string.Format("-- You are using the samename with {0} --", receivedData.ChatName);
                                data = sendData.GetDataStream();
                                serverSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, client.endPoint, new AsyncCallback(this.SendData), client.endPoint);
                                vaildloginname = false;
                                break;

                            }
                        }
                        if (vaildloginname)
                        {
                            this.clientList.Add(client);
                            
                            sendData.ChatName = string.Format("SERVER:");
                            
                            sendData.ChatMessage = string.Format("-- {0} is online --", receivedData.ChatName);
                        }
                        break;

                    case DataIdentifier.LogOut:
                        foreach (Client c in this.clientList)
                        {
                            if (c.endPoint.Equals(epSender))
                            {
                                this.clientList.Remove(c);
                                break;
                            }
                        }
                        sendData.ChatName = string.Format("SERVER:");
                        sendData.ChatMessage = string.Format("-- {0} has gone offline --", receivedData.ChatName);
                        break;
                }

                data = sendData.GetDataStream();
                
                foreach (Client client in this.clientList)
                {
                    if (client.name== sendData.ChatDest||client.name==sendData.ChatName)
                    {
                        
                        serverSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, client.endPoint, new AsyncCallback(this.SendData), client.endPoint);
                    }
                }

                serverSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(this.ReceiveData), epSender);

                this.Invoke(this.updateStatusDelegate, new object[] { sendData.ChatMessage });
            }
            catch (Exception ex)
            {
                MessageBox.Show("ReceiveData Error: " + ex.Message, "UDP Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region UpdateStatus

        private void UpdateStatus(string status)
        {
            rtxtStatus.Text += status + Environment.NewLine;
        }

        #endregion

        private void rtxtStatus_TextChanged(object sender, EventArgs e)
        {

        }
    }
}