namespace MonitoringService.Entities
{
    public class TraceEvent
    {
        public int Id { get; set; }
        public string TraceId { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public bool IsError { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
