// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using IncomingCallRouting.Models;
using IncomingCallRouting.Services;
using LiveWire.IncomingCall;

namespace IncomingCallRouting.Controllers
{
    public class IncomingCallRpcController : IncomingCall.IncomingCallBase
    {
        private readonly IIncomingCallEventService _incomingCallEventService;

        public IncomingCallRpcController(IIncomingCallEventService incomingCallEventService)
        {
            _incomingCallEventService = incomingCallEventService;
        }

        public override async Task Register(IAsyncStreamReader<CallingEventRequest> requestStream,
            IServerStreamWriter<CallingEventResponse> responseStream, ServerCallContext context)
        {
            List<string> clientIds = new();

            await foreach (var request in requestStream.ReadAllAsync())
            {
                switch (request.Action)
                {
                    case ActionType.Register:
                        Console.WriteLine($"Client {request.ClientId} connected.");

                        clientIds.Add(request.ClientId);

                        _incomingCallEventService.Register(request.ClientId, async callingEvent =>
                       {
                           Console.WriteLine($"Sending calling event {callingEvent.Id} of type {callingEvent.EventType} to client {request.ClientId}.");

                           await responseStream.WriteAsync(ParseResponse(callingEvent));
                       });

                       await responseStream.WriteAsync(new CallingEventResponse
                           {
                               EventType = EventType.RegisterClient,
                           });

                        break;

                    case ActionType.PlayMedia:
                        await _incomingCallEventService.PlayMedia(new CallingEventDto
                        {
                            Id = request.CallId,
                            PlayMediaOptions = request.PlayMediaOptions
                        });

                        break;

                    case ActionType.AddParticipant:
                        await _incomingCallEventService.AddParticipant(new CallingEventDto
                        {
                            Id = request.CallId,
                            AddParticipantOptions = request.AddParticipantOptions,
                        });

                        break;

                    case ActionType.EndCall:
                        await _incomingCallEventService.EndCall(new CallingEventDto { Id = request.CallId });

                        break;

                    case ActionType.StopMedia:
                        await _incomingCallEventService.StopMedia(new CallingEventDto { Id = request.CallId });

                        break;

                    default:
                        break;
                }

            }

            clientIds.ForEach(clientId => _incomingCallEventService.Deregister(clientId));
            clientIds.ForEach(clientId => Console.WriteLine($"Client {clientId} disconnected."));

        }

        private static CallingEventResponse ParseResponse(CallingEventDto callingEventDto)
        {
            var response = new CallingEventResponse
            {
                CallId = callingEventDto.Id,
                EventType = callingEventDto.EventType
            };

            switch (callingEventDto.EventType)
            {
                case EventType.CallConnection:
                    response.CallConnectionState = callingEventDto.CallConnectionState;

                    break;

                case EventType.DtmfTone:
                    response.DtmfTone = callingEventDto.DtmfToneValue;

                    break;

                case EventType.MediaPlayed:
                    response.PlayMediaState = callingEventDto.PlayMediaState;

                    break;

                case EventType.IncomingCall:
                    response.IncomingCallContext = callingEventDto.IncomingCallContext;
                    
                    break;

                default:
                    break;
            }

            return response;
        }
    }
}