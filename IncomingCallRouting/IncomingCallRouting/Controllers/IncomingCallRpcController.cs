// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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


        public override async Task Register(RegistrationRequest request, IServerStreamWriter<RegistrationResponse> responseStream, ServerCallContext context)
        {
            _ = Task.Run(() =>
            {
                _incomingCallEventService.Register($"IncomingCall", async (CallingEventDto callingEvent) =>
                {
                    await responseStream.WriteAsync(new RegistrationResponse
                    {
                        CallId = $"{request.ClientId}-{callingEvent.Id}"
                    });
                });
            });

            await Task.Delay(-1);
        }

        public override async Task SubscribeToEvents(CallingEventRequst request, IServerStreamWriter<CallingEventResponse> responseStream, ServerCallContext context)
        {
            _ = Task.Run(() =>
            {
                _incomingCallEventService.Register($"CallingEvents", async (CallingEventDto callingEvent) =>
                {
                    var response = new CallingEventResponse
                    {
                        CallId = callingEvent.Id,
                        EventType = callingEvent.EventType,
                    };

                    if (callingEvent.EventType == EventType.CallConnection)
                    {
                        response.CallConnectionState = callingEvent.CallConnectionState;
                    }

                    if (callingEvent.EventType == EventType.DtmfTone)
                    {
                        response.DtmfTone = callingEvent.DtmfToneValue;
                    }

                    await responseStream.WriteAsync(response);
                });
            });

            await Task.Delay(-1);
        }

        private static async Task DispatchEvent(CallingEventDto callingEvent, IServerStreamWriter<CallingEventResponse> responseStream)
        {
            var response = new CallingEventResponse
            {
                CallId = callingEvent.Id,
                EventType = callingEvent.EventType,
            };

            if (callingEvent.EventType == EventType.CallConnection)
            {
                response.CallConnectionState = callingEvent.CallConnectionState;
            }

            if (callingEvent.EventType == EventType.DtmfTone)
            {
                response.DtmfTone = callingEvent.DtmfToneValue;
            }

            await responseStream.WriteAsync(response);
        }
    }
}