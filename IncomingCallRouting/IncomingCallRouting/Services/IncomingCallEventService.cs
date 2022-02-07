// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IncomingCallRouting.Models;

namespace IncomingCallRouting.Services
{
    public class IncomingCallEventService : IIncomingCallEventService
    {
        private readonly IDictionary<string, Func<CallingEventDto, Task>> _eventDict = new Dictionary<string, Func<CallingEventDto, Task>>();

        public async Task Invoke(string eventId, CallingEventDto callingEvent)
        {
            if (_eventDict.TryGetValue(eventId, out var callingEventDispatcher))
            {
                await callingEventDispatcher(callingEvent);
            }
        }

        public void Register(string eventId, Func<CallingEventDto, Task> callingEventDispatcher)
        {
            if (!_eventDict.ContainsKey(eventId))
            {
                _eventDict.TryAdd(eventId, callingEventDispatcher);
            }
            else
            {
                _eventDict[eventId] = callingEventDispatcher;
            }
        }
    }
}