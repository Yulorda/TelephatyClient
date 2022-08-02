using System;

public interface IClient : IDisposable
{
    public enum Status
    {
        Disconnect,
        Connectind,
        Connected,
    }

    Status State { get; }

    void Connect();

    void Disconnect();

    bool TryGetPackage(out byte[] networkPackage);

    bool Send(byte[] networkPackage);
}