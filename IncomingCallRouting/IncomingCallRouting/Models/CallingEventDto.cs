// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Communication.CallingServer;
using LiveWire.IncomingCall;

namespace IncomingCallRouting.Models
{
    public class CallingEventDto : CallingServerEventBase 
    {
        public string Id { get; set; }

        public EventType EventType { get; set; }

        public CallConnectionState CallConnectionState { get; set; }

        public DtmfTone DtmfToneValue { get; set; }
    }
}