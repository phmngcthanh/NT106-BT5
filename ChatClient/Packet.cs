using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApplication
{
    // ----------------
    // Packet Structure
    // ----------------

    // Description   -> |dataIdentifier|name length|message length| dest length |    name   |   dest    |    message   |
    // Size in bytes -> |       4      |     4     |       4      |      4      |name length|dest length|message length|

    public enum DataIdentifier
    {
        Message,
        LogIn,
        LogOut,
        Null
    }

    public class Packet
    {
        #region Private Members
        private DataIdentifier dataIdentifier;
        private string name;
        private string message;
        private string dest;
        #endregion

        #region Public Properties
        public DataIdentifier ChatDataIdentifier
        {
            get { return dataIdentifier; }
            set { dataIdentifier = value; }
        }

        public string ChatName
        {
            get { return name; }
            set { name = value; }
        }

        public string ChatMessage
        {
            get { return message; }
            set { message = value; }
        }
        public string ChatDest
        {
            get { return dest; }
            set { dest = value; }
        }
        #endregion

        #region Methods

        // Default Constructor
        public Packet()
        {
            this.dataIdentifier = DataIdentifier.Null;
            this.message = null;
            this.name = null;
            this.dest = null;
        }

        public Packet(byte[] dataStream)
        {
            // Read the data identifier from the beginning of the stream (4 bytes)
            this.dataIdentifier = (DataIdentifier)BitConverter.ToInt32(dataStream, 0);

            // Read the length of the name (4 bytes)
            int nameLength = BitConverter.ToInt32(dataStream, 4);


            // Read the length of the message (4 bytes)
            int destLength = BitConverter.ToInt32(dataStream, 8);
            // Read the length of the message (4 bytes)
            int msgLength = BitConverter.ToInt32(dataStream, 12);
            // Read the name field
            if (nameLength > 0)
                this.name = Encoding.UTF8.GetString(dataStream, 16, nameLength);
            else
                this.name = null;
            // Read the message field
            if (destLength > 0)
                this.dest = Encoding.UTF8.GetString(dataStream, 16 + nameLength, destLength);
            else
                this.dest = null;
            // Read the message field
            if (msgLength > 0)
                this.message = Encoding.UTF8.GetString(dataStream, 16 + +nameLength + destLength, msgLength);
            else
                this.message = null;


        }

        // Converts the packet into a byte array for sending/receiving 
        public byte[] GetDataStream()
        {
            List<byte> dataStream = new List<byte>();

            // Add the dataIdentifier
            dataStream.AddRange(BitConverter.GetBytes((int)this.dataIdentifier));

            // Add the name length
            if (this.name != null)
                dataStream.AddRange(BitConverter.GetBytes(this.name.Length));
            else
                dataStream.AddRange(BitConverter.GetBytes(0));

            // Add the dest length
            if (this.dest != null)
                dataStream.AddRange(BitConverter.GetBytes(this.dest.Length));
            else
                dataStream.AddRange(BitConverter.GetBytes(0));

            // Add the message length
            if (this.message != null)
                dataStream.AddRange(BitConverter.GetBytes(this.message.Length));
            else
                dataStream.AddRange(BitConverter.GetBytes(0));


            // Add the name
            if (this.name != null)
                dataStream.AddRange(Encoding.UTF8.GetBytes(this.name));
            // Add the dest
            if (this.dest != null)
                dataStream.AddRange(Encoding.UTF8.GetBytes(this.dest));
            // Add the message
            if (this.message != null)
                dataStream.AddRange(Encoding.UTF8.GetBytes(this.message));


            return dataStream.ToArray();
        }

        #endregion
    }
}