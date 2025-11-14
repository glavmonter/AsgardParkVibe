namespace ParkingGate.Domain.Models;

public enum GateMode
{
    Entry,
    Exit
}

public enum HealthStatus
{
    Healthy,
    Degraded,
    Critical
}

public class GateState
{
    public GateMode Mode { get; set; } = GateMode.Entry;
    public BarrierStatus BarrierStatus { get; set; } = BarrierStatus.Closed;
    public LightStatus LightStatus { get; set; } = LightStatus.Green;
    public MifareStatus MifareStatus { get; set; } = MifareStatus.Idle;
    public HealthStatus SystemHealth { get; set; } = HealthStatus.Healthy;
    public string? LastError { get; set; }
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    public string CurrentStateName { get; set; } = "WaitingForVehicle";
}
