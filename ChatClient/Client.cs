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

using ChatApplication;

namespace ChatClient
{
    public partial class Client : Form
    {
        #region Private Members

        // Client socket
        private Socket clientSocket;

        // Client name
        private string name;
        //destination name
        private string Destname;

        // Server End Point
        private EndPoint epServer;

        // Data stream
        private byte[] dataStream = new byte[1024];

        // Display message delegate
        private delegate void DisplayMessageDelegate(string sender,string message);
        private DisplayMessageDelegate displayMessageDelegate = null;

        #endregion

        #region Constructor

        public Client()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        private void Client_Load(object sender, EventArgs e)
        {
            // Initialise delegate
            
            this.displayMessageDelegate = new DisplayMessageDelegate(this.DisplayMessage);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // Initialise a packet object to store the data to be sent
                Packet sendData = new Packet();
                sendData.ChatName = this.name;
                sendData.ChatMessage = txtMessage.Text.Trim();
                sendData.ChatDataIdentifier = DataIdentifier.Message;
                sendData.ChatDest = this.Destname;

                // Get packet as byte array
                byte[] byteData = sendData.GetDataStream();

                // Send packet to the server
                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);

                txtMessage.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send Error: " + ex.Message, "UDP Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (this.clientSocket != null)
                {
                    // Initialise a packet object to store the data to be sent
                    Packet sendData = new Packet();
                    sendData.ChatDataIdentifier = DataIdentifier.LogOut;
                    sendData.ChatName = this.name;
                    sendData.ChatMessage = null;
                    sendData.ChatDest = this.Destname;

                    // Get packet as byte array
                    byte[] byteData = sendData.GetDataStream();

                    // Send packet to the server
                    this.clientSocket.SendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer);

                    // Close the socket
                    this.clientSocket.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Closing Error: " + ex.Message, "UDP Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                this.name = txtName.Text.Trim();
                this.Destname = txtDest.Text.Trim();
                // Initialise a packet object to store the data to be sent
                Packet sendData = new Packet();
                sendData.ChatName = this.name;
                sendData.ChatMessage = null;
                sendData.ChatDataIdentifier = DataIdentifier.LogIn;
                sendData.ChatDest = this.Destname;

                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress serverIP = IPAddress.Parse(txtServerIP.Text.Trim());

                IPEndPoint server = new IPEndPoint(serverIP, 30000);

                epServer = (EndPoint)server;

                byte[] data = sendData.GetDataStream();

                clientSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);

                this.dataStream = new byte[1024];

                clientSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Error: " + ex.Message, "UDP Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region Send And Receive

        private void SendData(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send Data: " + ex.Message, "UDP Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReceiveData(IAsyncResult ar)
        {
            try
            {
                this.clientSocket.EndReceive(ar);

                Packet receivedData = new Packet(this.dataStream);
                
                if (receivedData.ChatMessage != null)
                    this.Invoke(this.displayMessageDelegate, new object[] { receivedData.ChatName, receivedData.ChatMessage });

                this.dataStream = new byte[1024];

                clientSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show("Receive Data: " + ex.Message, "UDP Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Other Methods

        private void DisplayMessage(string sender,string messge)
        {
            if (this.name == sender)
                sender = "You";
            rtxtConversation.Text += sender+": "+messge + Environment.NewLine;
        }

        #endregion

        private void rtxtConversation_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
