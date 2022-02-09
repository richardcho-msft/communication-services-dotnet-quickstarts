// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Azure.Communication.CallingServer;
using IncomingCallRouting.Models;

namespace IncomingCallRouting.Services
{
    public interface IIncomingCallEventService
    {
        void Register(string clientId, Func<CallingEventDto, Task> callingEventDispatcher);

        void Deregister(string clientId);

        Task Invoke(string callId, CallingEventDto callingEvent);
    }
}