using Serializator;
using Telepathy;
using Zenject;

public class NetworkInstaller : Installer<NetworkInstaller>
{
    public NetworkConfig networkConfig;

    public override void InstallBindings()
    {
        Container.Bind<INetwork>()
            .FromMethod(x => GetNetworkClient(networkConfig))
            .AsSingle()
            .NonLazy();

        Container.Bind<INetworkStatus>()
            .To<NetworkStatus>()
            .AsSingle()
            .NonLazy();

        Container.Bind<IRequest>()
            .To<Requests>()
            .AsSingle()
            .NonLazy();
    }

    public NetworkClient GetNetworkClient(NetworkConfig networkConfig)
    {
        var client = new Client(networkConfig.ip, networkConfig.port);
        var networkClient = new NetworkClient(client, new JSONSerializator());
        client.actionLog = x => networkClient.NetworkLogger(x);
        //TODO: можно сделать это явно
        client.Connect();
        return networkClient;
    }
}
