// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Grpc.Core;
using LiveWire.IncomingCall.Models;

namespace IncomingCallRouting.Controllers
{
    public class GrpcTestController : TestService.TestServiceBase
    {
        public override async Task Test(IAsyncStreamReader<TestMessage> requestStream, IServerStreamWriter<TestMessage> responseStream, ServerCallContext context)
        {
            await foreach(var request in requestStream.ReadAllAsync())
            {
                Console.WriteLine($"Receiving gRPC request: Count {request.Count} at {request.TimeStamp}.");

                await responseStream.WriteAsync(new TestMessage
                {
                    Id = request.Id,
                    TimeStamp = request.TimeStamp,
                    Count = request.Count
                });
            }
        }
    }
}