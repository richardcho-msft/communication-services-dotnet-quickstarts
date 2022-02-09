using IncomingCallRouting.Services;

namespace IncomingCallRouting.Models
{
    public class ConnectionManagerOptions
    {
        public DistributionMode DistributionMode { get; set; } = DistributionMode.RoundRobin;
    }
}
