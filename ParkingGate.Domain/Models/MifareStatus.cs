namespace ParkingGate.Domain.Models;

public enum MifareStatus
{
    Idle,
    Searching,
    Writing,
    CardRead,
    CardWritten,
    Error
}

public record MifareCard(string CardNumber, DateTime ReadTime);
