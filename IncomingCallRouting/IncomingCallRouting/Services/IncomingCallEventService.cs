// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IncomingCallRouting.Models;
using LiveWire.IncomingCall;

namespace IncomingCallRouting.Services
{
    public class IncomingCallEventService : IIncomingCallEventService
    {
        private readonly IConnectionManager _connectionManager;

        public IncomingCallEventService(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        private readonly ConcurrentDictionary<string, Func<CallingEventDto, Task>> _clientIdToCallback = new ();
        private readonly ConcurrentDictionary<string, string> _callIdToClientId = new ();

        public void Register(string clientId, Func<CallingEventDto, Task> callingEventDispatcher)
        {
            _connectionManager.Register(clientId);
            _clientIdToCallback.AddOrUpdate(clientId, callingEventDispatcher, (_, _) => callingEventDispatcher);
        }

        public void Deregister(string clientId)
        {
            _connectionManager.Deregister(clientId);
            _clientIdToCallback.TryRemove(clientId, out _);
            _callIdToClientId.Where(kv => kv.Value == clientId).ToList().ForEach(kv => _callIdToClientId.TryRemove(kv.Key, out _));
        }

        public async Task SendEvent(CallingEventDto callingEvent)
        {
            var clientId = _callIdToClientId.GetOrAdd(callingEvent.Id, _connectionManager.Next());
            if (_clientIdToCallback.TryGetValue(clientId, out var callingEventDispatcher))
            {
                await callingEventDispatcher(callingEvent);
            }
        }
    }
}