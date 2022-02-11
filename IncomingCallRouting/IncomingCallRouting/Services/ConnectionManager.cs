using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using IncomingCallRouting.Models;
using Microsoft.Extensions.Configuration;

namespace IncomingCallRouting.Services
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly DistributionMode _mode;
        private readonly ConcurrentDictionary<string, int> _connectionCounts = new ();
        private IEnumerator<KeyValuePair<string, int>> _enumerator;

        public ConnectionManager(ConnectionManagerOptions options)
        {
            _mode = options.DistributionMode;
            _enumerator = _connectionCounts.GetEnumerator();
        }

        public void Register(string connectionId)
        {
            _connectionCounts.AddOrUpdate(connectionId, 1, (_, count) => count + 1);
        }

        public void Deregister(string connectionId)
        {
            var count = _connectionCounts.AddOrUpdate(connectionId, 0, (_, c) => c - 1);
            if (count < 1) _connectionCounts.TryRemove(connectionId, out _);
        }

        public string Next()
        {
            switch (_mode)
            {
                case DistributionMode.RoundRobin:
                    if (!_enumerator.MoveNext())
                    {
                        _enumerator = _connectionCounts.GetEnumerator();
                        _enumerator.MoveNext();
                    }

                    return _enumerator.Current.Key;
                case DistributionMode.LeastIdle:
                    return _connectionCounts.MinBy(connection => connection.Value).Key;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum DistributionMode
    {
        RoundRobin = 0,
        LeastIdle = 1
    }
}
