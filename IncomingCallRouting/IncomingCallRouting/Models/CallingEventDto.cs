// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using LiveWire.IncomingCall;

namespace IncomingCallRouting.Models
{
    public class CallingEventDto 
    {
        public string Id { get; set; }

        public EventType EventType { get; set; }

        public CallConnectionState CallConnectionState { get; set; }

        public DtmfTone DtmfToneValue { get; set; }

        public PlayMediaOptions PlayMediaOptions { get; set; }

        public PlayMediaState PlayMediaState { get; set; }

        public AddParticipantOptions AddParticipantOptions { get;  set; }

        public IEnumerable<Participant> Participants { get; set; }

        public IncomingCallContext IncomingCallContext { get; set; }

        public SpeachOptions SpeachOptions { get; set; }

        public RecordingOptions RecordingOptions { get; set; }
    }
}