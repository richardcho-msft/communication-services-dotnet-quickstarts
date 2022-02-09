using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace IncomingCallRouting.Services
{
    public interface IConnectionManager
    {
        void Register(string connectionId);

        void Deregister(string connectionId);

        string Next();
    }
}
