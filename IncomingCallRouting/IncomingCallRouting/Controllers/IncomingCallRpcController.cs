// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
            List<string> clientIds = new ();

            await foreach (var request in requestStream.ReadAllAsync())
            {
                clientIds.Add(request.ClientId);
                _incomingCallEventService.Register(request.ClientId, async callingEvent =>
                {
                    await responseStream.WriteAsync(new CallingEventResponse
                    {
                        CallId = callingEvent.Id,
                    });
                });
            }

            clientIds.ForEach(clientId => _incomingCallEventService.Deregister(clientId));
        }
    }
}