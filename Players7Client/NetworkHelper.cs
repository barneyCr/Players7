using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Players7Client
{
    public delegate void WriteLogDelegate(string format, params object[] obj);
    public partial class NetworkHelper
    {
        #region Static members
        static byte[] NameRequiredPacket = new byte[] { 2, 8, 18, 32 };
        static byte[] FullAuthRequiredPacket = new byte[] { 2, 32, 8, 18 };
        static byte[] InviteCodeRequiredPacket = new byte[] { 2, 8, 32, 18 };
        static byte[] ServerIsFullPacket = new byte[] { 2, 18, 8, 32 };
        static byte[] AccessGrantedPacket = new byte[] { 2, 32, 18, 8 };
        static byte[] AccessDeniedPacket = new byte[] { 2, 18, 32, 8 };

        static ASCIIEncoding enc = new ASCIIEncoding();
        public static ASCIIEncoding Encoding { get { return NetworkHelper.enc; } }
    
        #endregion


        public NetworkHelper(string ip, int port, string username, string password, WriteLogDelegate callbackMethod)
        {
            this.IP = ip;
            this.Port = port;
            this.Username = username;
            this.Password = password;

            this.WriteLog = callbackMethod;

            settings.Username = username;
            settings.PreferredIP = ip;
            settings.Save();
        }

        public MainForm Form { private get; set; }
        private Thread listenThread;
        public bool Connected { get; private set; }
        public bool Kicked { get; set; }
        public int Port { get; private set; }
        public string IP { get; private set; }
        public Socket Socket { get; set; }
        public string Username, Password;
        public int UID { get; set; }

        static readonly Properties.Settings settings = global::Players7Client.Properties.Settings.Default;
        readonly WriteLogDelegate WriteLog;

        public bool Connect()
        {
            try
            {
                this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.Socket.Connect(IP, Port);
                Connected = true;

                byte[] buffer = new byte[4];
                int bytesRead = 0;

                if (!Receive(buffer, ref bytesRead))
                {
                    return false;
                }
                else
                {
                    byte[] toSend;
                    byte[] username__;
                    if (buffer.SequenceEqual(NameRequiredPacket))
                    {
                        username__ = enc.GetBytes(Helper.XorText(this.Username, 0x45));
                        toSend = new byte[username__.Length + 1];
                        toSend[0] = 0x45;
                        Buffer.BlockCopy(username__, 0, toSend, 1, username__.Length);
                        toSend = toSend.GetWhileNotNullBytes();
                        int sent = Socket.Send(toSend);
                    }
                    else if (buffer.SequenceEqual(FullAuthRequiredPacket))
                    {
                        string packet = String.Concat(
                            (char)0x55,
                            Helper.XorText(this.Username, 0x55),
                            (char)0x7f,
                            Helper.XorText(this.Password, 0x55));
                        toSend = enc.GetBytes(packet);
                        Socket.Send(toSend);
                    }
                    else if (buffer.SequenceEqual(InviteCodeRequiredPacket))
                    {
                        string packet = String.Concat(
                            (char)0x65,
                            this.Username,
                            (char)0x7f,
                            this.Password);
                        toSend = enc.GetBytes(packet);
                        Socket.Send(toSend);
                    }
                    else if (buffer.SequenceEqual(ServerIsFullPacket))
                    {
                        this.WriteLog("The server's capacity has been reached. Come back later !");
                        return false;
                    }

                    buffer = new byte[4];
                    if (!Receive(buffer, ref bytesRead))
                        return false;
                    else
                    {
                        if (buffer.SequenceEqual(AccessGrantedPacket))
                        {
                            this.listenThread = new Thread(NetworkAction);
                            this.listenThread.IsBackground = true;
                            this.listenThread.Start();
                            this.ConnectionLost += NetworkHelper_ConnectionLost;
                            return true;
                        }
                        else if (buffer.SequenceEqual(AccessDeniedPacket))
                        {
                            this.WriteLog("Access denied. Wrong password maybe ?");
                        }
                        return false;
                    }
                }
            }
            catch (SocketException)
            {
                return false;
            }
        }

        private bool Receive(byte[] buffer, ref Int32 bytesRead)
        {
            try
            {
                bytesRead = Socket.Receive(buffer);
                return bytesRead != 0;
            }
            catch (SocketException)
            {
                return false;
            }
        }


        private void NetworkAction()
        {
            // changed from 512 to 1024 (8th march 2016)
            byte[] buff = new byte[1024];
            int bytes = 0;
            while (Socket.Connected)
            {
                if (!Receive(buff, ref bytes)) { break; }

                Packet packet;
                string[] packetArray = enc.GetString(buff, 0, bytes).Split('\n');

                foreach (var pstr in packetArray)
                {
                    using (packet = new Packet(pstr, false))
                    {
                        HandlePacket(packet);
                    }
                }
            }
            if (this.ConnectionLost != null && Kicked == false)
                this.ConnectionLost();
        }



        public void Send(string msg, params object[] obj)
        {
            Socket.Send(enc.GetBytes(string.Format(msg, obj)));
        }

        private async void HandlePacket(Packet p)
        {
            p.Seek(+1);
            // in ignore1.txt
            if (p.Header == "")
            {
                return;
            }
            else if (p.Header == "1") // INIT 
            {
                int myUID = this.UID = p.ReadInt();
                string myName = this.Username = p.ReadString();
                Player.Me = new Player(myUID, myName);
                Player.All.Add(myUID, Player.Me);
                // maybe more
            }
            else if (p.Header == "31") // NEW CONNECTED
            {
                int id = p.ReadInt();
                string name = p.ReadString();
                if (!Player.All.ContainsKey(id))
                    Player.All.Add(id, new Player(id, name));
                // todo Form.AnnounceConnection
            }
			else if (p.Header == "29") // ADD PREVIOUSLY CONNECTED PLAYER
			{
				int id = p.ReadInt();
				if (!Player.All.ContainsKey(id))
					Player.All.Add(id, new Player(id, p.ReadString()));
			}
            
            else if (p.Header == "3") // BROADCAST
            {
                //todo
            }
            else if (p.Header == "4") // ADMIN MESSAGE
            {
                //todo
            }
            else if(p.Header == "12") // CLIENT DISCONNECTED 
            {
                int uid = p.ReadInt() ^ 0x50;
                // todo Form.AnnounceDisconnection
                Player.All.Remove(uid);
            }
            else if (p.Header == HeaderTypes.GAME_SERVER_SETS_PL_LEVERAGE.ToString())
            {
                Player.Me.MyLeverage.Value = p.ReadDouble();
            }
            else if (p.Header == HeaderTypes.GAME_FREEZE_LEVERAGE.ToString())
            {

            }
            else if (p.Header == "-1") // kicked!
            {
				this.ConnectionLost = null; // do not reconnect!
				this.Kicked = true;
				//this.Form.WriteLog("You have been kicked from the server. Retry connecting in a few minutes.");
				await Task.Delay(5000);
				Environment.Exit(-1);
            }
        }
        // todo this throws an exception when authmethod = invitecode
        // when the client tries to reconnect and finally finds the server,
        // it throws InvalidOperationException
        async void NetworkHelper_ConnectionLost()
        {
            await Task.Run(async () =>
            {
                Player.All.Clear();

                while (!this.Socket.Connected)
                {
                    if (this.ReconnectTick != null)
                        this.ReconnectTick();
                    if (this.Connect())
                    {
                        if (this.Reconnected != null)
                            this.Reconnected();
                        this.ConnectionLost -= this.NetworkHelper_ConnectionLost;
                    }
                    else
                        await Task.Delay(5000);
                }
            });
        }

        public event Action ConnectionLost;
        public event Action ReconnectTick;
        public event Action Reconnected;
    }
}