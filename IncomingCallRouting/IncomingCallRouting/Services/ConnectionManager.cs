using System;
using System.Collections.Generic;
using System.Linq;

namespace IncomingCallRouting.Services
{
    public class ConnectionManager : IConnectionManager
    {
        public List<string> Connections { get; } = new();

        private int currentIndex;

        public void Register(string connectionId)
        {
            if (!Connections.Contains(connectionId))
            {
                Connections.Add(connectionId);
            }
        }

        public void Deregister(string connectionId)
        {
            Connections.Remove(connectionId);

            if (currentIndex >= Connections.Count)
            {
                currentIndex = 0;
            }
        }

        public string Next()
        {
            if (Connections.Count > 0)
            {
                var connection = Connections[currentIndex];
                currentIndex = (currentIndex+1) % Connections.Count;
                return connection;
            }

            return null;
        }
    }
}
