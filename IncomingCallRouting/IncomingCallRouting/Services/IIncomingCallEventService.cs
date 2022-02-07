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

        void Register(string eventId, Func<CallingEventDto, Task> callingEventDispatcher);


        Task Invoke(string eventId, CallingEventDto callingEvent);
    }
}