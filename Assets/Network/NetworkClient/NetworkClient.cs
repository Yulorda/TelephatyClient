
using Serializator;

using System;
using System.Collections.Generic;

using UniRx;

using UniRxMessageBroker;

using UnityEngine;

public class NetworkClient : IDisposable, INetwork
{
    public IClient Client { get; private set; }
    public ISerializator Serializator { get; private set; }
    public List<IDisposable> disposables = new List<IDisposable>();
    
    protected Broker messageBroker = new Broker();

    public NetworkClient(IClient client, ISerializator serializator)
    {
        this.Client = client;
        this.Serializator = serializator;
    }

    public IDisposable AddListener<T>(Action<T> handler) where T : class
    {
        return messageBroker.Receive<T>().Subscribe(handler);
    }

    public virtual void Connect()
    {
        Client.Connect();
        disposables.Add(Observable.EveryEndOfFrame().Subscribe(x => Update()));
    }

    public virtual void Send(object value)
    {
        Client.Send(Serializator.Serialize(value));
    }

    public virtual void Update()
    {
        if (Client.State == IClient.Status.Connected)
        {
            while (Client.TryGetPackage(out var package))
            {
                if (Serializator.TryDeserialize(package, out var type, out var value))
                {
                    messageBroker.Publish(value, type);
                }
                else
                {
                    NetworkLogger(new NetworkLog(EventType.Error, "SerializeError"));
                }
            }
        }
    }

    public void NetworkLogger(NetworkLog networkLog)
    {
        switch (networkLog.eventType)
        {
            case EventType.Connected:
                Debug.Log(networkLog.message);
                break;

            case EventType.Disconnected:
                Debug.Log(networkLog.message);
                break;

            case EventType.Error:
                Debug.LogError(networkLog.message);
                break;

            case EventType.Data:
                Debug.LogWarning(networkLog.message);
                break;
        }
    }

    public void Disconnect()
    {
        Client.Disconnect();
    }

    public void Dispose()
    {
        messageBroker.Dispose();
        Client.Dispose();
        disposables.Clear();
    }
}