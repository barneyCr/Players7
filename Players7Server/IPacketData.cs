using System;
using System.Collections.Generic;

namespace Players7Server
{
    public interface IPacketData
    {
        IEnumerable<string> Pack();
        void UnpackFrom(string data);
    }
}
