using ParkingGate.Domain.Models;

namespace ParkingGate.Infrastructure.Services;

public interface IGateStateService
{
    GateState GetCurrentState();
    void UpdateBarrierStatus(BarrierStatus status);
    void UpdateLightStatus(LightStatus status);
    void UpdateMifareStatus(MifareStatus status);
    void UpdateHealthStatus(HealthStatus status, string? errorMessage = null);
    void UpdateStateName(string stateName);
}

public class GateStateService : IGateStateService
{
    private readonly GateState _state = new();
    private readonly object _lock = new();

    public GateState GetCurrentState()
    {
        lock (_lock)
        {
            return new GateState
            {
                Mode = _state.Mode,
                BarrierStatus = _state.BarrierStatus,
                LightStatus = _state.LightStatus,
                MifareStatus = _state.MifareStatus,
                SystemHealth = _state.SystemHealth,
                LastError = _state.LastError,
                LastUpdate = _state.LastUpdate,
                CurrentStateName = _state.CurrentStateName
            };
        }
    }

    public void UpdateBarrierStatus(BarrierStatus status)
    {
        lock (_lock)
        {
            _state.BarrierStatus = status;
            _state.LastUpdate = DateTime.UtcNow;
        }
    }

    public void UpdateLightStatus(LightStatus status)
    {
        lock (_lock)
        {
            _state.LightStatus = status;
            _state.LastUpdate = DateTime.UtcNow;
        }
    }

    public void UpdateMifareStatus(MifareStatus status)
    {
        lock (_lock)
        {
            _state.MifareStatus = status;
            _state.LastUpdate = DateTime.UtcNow;
        }
    }

    public void UpdateHealthStatus(HealthStatus status, string? errorMessage = null)
    {
        lock (_lock)
        {
            _state.SystemHealth = status;
            _state.LastError = errorMessage;
            _state.LastUpdate = DateTime.UtcNow;
        }
    }

    public void UpdateStateName(string stateName)
    {
        lock (_lock)
        {
            _state.CurrentStateName = stateName;
            _state.LastUpdate = DateTime.UtcNow;
        }
    }
}
