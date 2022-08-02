
using System;

//TODO: rename
public interface INetwork
{
    void Connect();
    void Disconnect();
    IDisposable AddListener<T>(Action<T> handler) where T : class;
    void Send(object value);
}
