using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Players7Server.Networking;
using Players7Server.GameLogic;

namespace Players7Server
{
	public enum ConnectionState
	{
		Offline,
		Online // lol
	}

	public class Client
	{
		public void Send(string message, params object[] p)
		{
			this.Send(string.Format(message, p));
		}
		public void Send(string message)
		{
			this.Socket.Send(message);
			//Console.WriteLine("Packet to" + this.Username +": "+ message);
            Program.Write("Sending message of type " + Networking.Server.GetHeaderType(message.Split('|')[0]) + " to " + this.Username + "[" + this.UserID + "]",
				"PacketLogs", ConsoleColor.Blue);
		}

		public ConnectionState ConnectionState { get; set; }
		public Thread Thread { get; set; }
		public Socket Socket { get; set; }
		public int MessagesSent { get; set; }
		public string Username { get; set; }
		public int UserID { get; set; }

        /// <summary>
        /// todo
        /// </summary>
        /// <value>The personal leverage.</value>
        public double PersonalLeverage { get; set; }

        //public CardPack PackOfCards { get; set; }


		public Dictionary<int, DateTime> Reports { get; set; }
        public Game CurrentGame { get; internal set; }

        public string GetEndpoint()
		{
			return this.Socket.RemoteEndPoint.ToString().Split(':')[0];
		}

		/// <summary>
		/// Returns a new instance of the Client class, and generates a UID between 70 and 500000
		/// </summary>
		/// <param name="name">Given name</param>
		/// <param name="socket">TCP connection</param>
		public Client(string name, Socket socket)
		{
			this.Username = name;
			this.Socket = socket;
			int uid;
            lock (Program.Server.Connections) {
                do
                    uid = Helper.Randomizer.Next(70, 50000);
                while (Program.Server.Connections.ContainsKey(uid));
            }
            this.UserID = uid;
			this.Reports = new Dictionary<int, DateTime>();
		}
	}
}