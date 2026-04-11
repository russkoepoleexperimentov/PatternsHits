using Common.Contracts.MonitoringContracts;

namespace Common.Services
{
    public interface ITracingClient
    {
        Task SendAsync(TraceEventDto dto);
    }
}
