using System.Threading.Tasks;
using Greet;
using Grpc.Core;

namespace IncomingCallRouting.Controllers
{
    public class GreeterService : Greeter.GreeterBase
    {

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello2 " + request.Name
            });
        }
    }
}