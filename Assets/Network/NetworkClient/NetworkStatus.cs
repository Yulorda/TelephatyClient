
using System;

using UniRx;

public class NetworkStatus : INetworkStatus
{
    public event Action OnConnected;
    public event Action OnDisconnected;

    public NetworkStatus(NetworkClient networkClient)
    {
        networkClient.disposables.Add(networkClient.Client.ObserveEveryValueChanged(x => x.State, FrameCountType.EndOfFrame).Subscribe(ConnectionStateChange));
    }

    private void ConnectionStateChange(IClient.Status state)
    {
        switch (state)
        {
            case IClient.Status.Disconnect:
                OnDisconnected?.Invoke();
                break;
            case IClient.Status.Connectind:
                break;
            case IClient.Status.Connected:
                OnConnected?.Invoke();
                break;
        }
    }
}
