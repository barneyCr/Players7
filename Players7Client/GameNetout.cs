using System;

namespace Players7Client
{
    public partial class NetworkHelper
    {
        public void SendCreateGame(int maxplayers, int bet, string name)
        {
            this.Send(Packet.CreatePacket(HeaderTypes.GAME_CREATE, maxplayers, bet, name));
        }

        public void SendJoinGameRequest(string ids)
        {
            this.Send(Packet.CreatePacket(HeaderTypes.GAME_JOIN_REQUEST, ids));
        }

        public void SendIAmReady()
        {
            this.Send(Packet.CreatePacket(HeaderTypes.GAME_PLAYER_READY));
        }

        public void SendPutCard(byte val, byte type)
        {
            this.Send(Packet.CreatePacket(HeaderTypes.GAME_PLAYER_PUT_CARD, val, type));
        }

        public void SendSetLeverageRequest(double value)
        {
            this.Send(Packet.CreatePacket(HeaderTypes.GAME_SET_LEVERAGE_REQUEST, value));
        }
    }
}