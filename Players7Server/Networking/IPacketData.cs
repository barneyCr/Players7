using System;
using System.Collections.Generic;

namespace Players7Server.Networking
{
    public interface IPacketData
    {
        IEnumerable<string> Pack();
        void UnpackFrom(string data);
    }
}
