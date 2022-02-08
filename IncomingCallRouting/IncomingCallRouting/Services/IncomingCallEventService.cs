// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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

        private readonly IDictionary<string, Func<CallingEventDto, Task>> _connectionIdToCallback = new Dictionary<string, Func<CallingEventDto, Task>>();
        private readonly IDictionary<string, string> _callIdToConnectionId = new Dictionary<string, string>();


        public async Task Invoke(string connectionId, CallingEventDto callingEvent)
        {
            string? connectionId = null;

            if (callingEvent.EventType is EventType.IncomingCall)
            {
                connectionId = _connectionManager.Next();
                // _callIdToConnectionId.Add(eventId, )
            }
            else
            {
                // send to same connection?
            }

            if (_callIdToConnectionId.TryGetValue(connectionId, out var callingEventDispatcher))
            {
                await callingEventDispatcher(callingEvent);
            }
        }

        public void Register(string clientId, Func<CallingEventDto, Task> callingEventDispatcher)
        {
            _connectionManager.Register(clientId);

            if (!_connectionIdToCallback.ContainsKey(clientId))
            {
                _connectionIdToCallback.TryAdd(clientId, callingEventDispatcher);
            }
            else
            {
                _connectionIdToCallback[clientId] = callingEventDispatcher;
            }
        }
    }
}