// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Communication;
using Azure.Communication.CallingServer;
using Azure.Communication.CallingServer.Models;
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

        private readonly ConcurrentDictionary<string, Func<CallingEventDto, Task>> _clientIdToCallback = new();
        private readonly ConcurrentDictionary<string, string> _callIdToClientId = new();
        // strictly for incoming call demo purposes
        private readonly ConcurrentDictionary<string, CallConnection> _callIdToConnection = new();

        public void Register(string clientId, Func<CallingEventDto, Task> callingEventDispatcher)
        {
            _connectionManager.Register(clientId);
            _clientIdToCallback.AddOrUpdate(clientId, callingEventDispatcher, (_, _) => callingEventDispatcher);
        }

        public void Deregister(string clientId)
        {
            _connectionManager.Deregister(clientId);
            _clientIdToCallback.TryRemove(clientId, out _);
            _callIdToClientId.Where(kv => kv.Value == clientId).ToList().ForEach(kv =>
            {
                _callIdToClientId.TryRemove(kv.Key, out _);
                _callIdToConnection.TryRemove(kv.Key, out _);
            });
        }

        public async Task SendEvent(CallingEventDto callingEvent)
        {
            var clientId = _callIdToClientId.GetOrAdd(callingEvent.Id, _connectionManager.Next());
            if (_clientIdToCallback.TryGetValue(clientId, out var callingEventDispatcher))
            {
                await callingEventDispatcher(callingEvent);
            }
        }

        public async Task PlayMedia(CallingEventDto callingEvent)
        {
            if (_callIdToConnection.TryGetValue(callingEvent.Id, out var callConnection))
            {
                var response = await callConnection.PlayAudioAsync(new Uri(callingEvent.PlayMediaOptions.MediaUrl), new PlayAudioOptions
                {
                    OperationContext = callingEvent.PlayMediaOptions.OperationalContext ?? new Guid().ToString(),
                    Loop = callingEvent.PlayMediaOptions.Loop
                });

                if (response.Value.Status == CallingOperationStatus.Running)
                {
                    await SendEvent(new CallingEventDto
                    {
                        Id = callingEvent.Id,
                        EventType = EventType.MediaPlayed,
                        PlayMediaState = PlayMediaState.Running
                    });
                }
            }
        }

        public async Task StopMedia(CallingEventDto callingEvent)
        {
            if (_callIdToConnection.TryGetValue(callingEvent.Id, out var callConnection))
            {
                await callConnection.CancelAllMediaOperationsAsync();
            }
        }

        public async Task AddParticipant(CallingEventDto callingEvent)
        {
            var identifierKind = GetIdentifierKind(callingEvent.AddParticipantOptions.ParticipantId);

            if (identifierKind == CommunicationIdentifierKind.UnknownIdentity)
            {
                Logger.LogMessage(Logger.MessageType.INFORMATION, "Unknown identity provided. Enter valid phone number or communication user id");
            }
            else
            {
                _callIdToConnection.TryGetValue(callingEvent.Id, out var callConnection);

                if (identifierKind == CommunicationIdentifierKind.UserIdentity)
                {
                    await callConnection.TransferToParticipantAsync(new CommunicationUserIdentifier(callingEvent.AddParticipantOptions.ParticipantId),
                                                                    null,
                                                                    null,
                                                                    callingEvent.AddParticipantOptions.OperationalContext).ConfigureAwait(false);

                }
                else if (identifierKind == CommunicationIdentifierKind.PhoneIdentity)
                {
                    await callConnection.TransferToParticipantAsync(new PhoneNumberIdentifier(callingEvent.AddParticipantOptions.ParticipantId),
                                                                    null,
                                                                    null,
                                                                    callingEvent.AddParticipantOptions.OperationalContext).ConfigureAwait(false);
                }
            }
        }

        public Task AcceptCall(CallingEventDto callingEvent)
        {
            throw new NotImplementedException();
        }

        public Task RejectCall(CallingEventDto callingEvent)
        {
            throw new NotImplementedException();
        }

        public async Task EndCall(CallingEventDto callingEvent)
        {
            try
            {
                if (_callIdToConnection.TryGetValue(callingEvent.Id, out var callConnection))
                {
                    await callConnection.HangupAsync();
                }
            }
            finally
            {
                _callIdToConnection.Remove(callingEvent.Id, out _);
                _callIdToClientId.Remove(callingEvent.Id, out _);
            }
        }

        // strictly for demo purposes
        public void RegisterCallConnection(CallConnection callConnection)
        {
            _callIdToConnection.AddOrUpdate(callConnection.CallConnectionId, callConnection, (_, _) => callConnection);
        }

        // strictly for demo purposes
        private CommunicationIdentifierKind GetIdentifierKind(string participantnumber)
        {
            //checks the identity type returns as string
            return Regex.Match(participantnumber, Constants.userIdentityRegex, RegexOptions.IgnoreCase).Success ? CommunicationIdentifierKind.UserIdentity :
                   Regex.Match(participantnumber, Constants.phoneIdentityRegex, RegexOptions.IgnoreCase).Success ? CommunicationIdentifierKind.PhoneIdentity :
                   CommunicationIdentifierKind.UnknownIdentity;
        }
    }
}