public struct NetworkLog
{
    public readonly EventType eventType;
    public readonly string message;

    public NetworkLog(EventType eventType, string message = null)
    {
        this.eventType = eventType;
        this.message = message;
    }
}