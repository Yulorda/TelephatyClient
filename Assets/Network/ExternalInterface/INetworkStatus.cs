
using System;

public interface INetworkStatus
{
    public event Action OnConnected;
    public event Action OnDisconnected;
}
