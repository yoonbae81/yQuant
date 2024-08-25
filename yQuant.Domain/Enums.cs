namespace yQuant.Domain;
public enum OrderType
{
    Market,
    Limit
    // Stop
    // StopLimit,
    // TrailingStop
}

public enum OrderPosition
{
    Entry,
    Exit
}

public enum OrderStatus
{
    NotPlaced,
    Placed,
    PartiallyFilled,
    Filled,
    Cancelled,
    Rejected
}