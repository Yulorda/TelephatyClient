using Cysharp.Threading.Tasks;

using System;
using System.Threading;

using UniRx;

using UnityEngine;

public class Requests : IRequest
{
    private NetworkClient networkClient;

    public Requests(NetworkClient networkClient)
    {
        this.networkClient = networkClient;
    }

    public async UniTask<T> Request<T, U>(DBOp dBOp, U obj)
    {
        var id = new Guid().ToString();
        var cancelToken = new CancellationTokenSource();
        var subject = new Subject<T>();

        var package = new DBOperation<U>() { id = id, message = obj, dBOp = dBOp };
        var disposeListener = networkClient.AddListener<DBOperation<T>>(x => SubsribeOnId(id, x, subject));
        var disposeWaitPackage = Observable.Timer(new TimeSpan(0, 0, 10)).Subscribe(x =>
        {
            cancelToken.Cancel();
        });

        networkClient.Send(package);

        T result = default;

        try
        {
            result = await subject.ToUniTask(true, cancellationToken: cancelToken.Token);
        }
        catch
        {
            Debug.LogError("Request error");
        }

        disposeWaitPackage.Dispose();
        disposeListener.Dispose();

        return result;
    }

    public async UniTask<T> Request<T, U>(U obj)
    {
        var id = new Guid().ToString();
        var subject = new Subject<T>();
        var cancelToken = new CancellationTokenSource();

        var package = new PackageWithId<U>() { id = id, message = obj };

        var disposeListener = networkClient.AddListener<PackageWithId<T>>(x => SubsribeOnId<T>(id, x, subject));
        var disposeWaitPackage = Observable.Timer(new TimeSpan(0, 0, 10)).Subscribe(x =>
        {
            cancelToken.Cancel();
        });

        networkClient.Send(package);
        T result = default;

        try
        {
            result = await subject.ToUniTask(true, cancellationToken: cancelToken.Token);
        }
        catch
        {
            Debug.LogError("Request error");
        }

        disposeWaitPackage.Dispose();
        disposeListener.Dispose();

        return result;
    }

    private void SubsribeOnId<T>(string id, PackageWithId<T> packageWithId, ISubject<T> reactiveProperty)
    {
        if (id.Equals(packageWithId.id))
        {
            reactiveProperty?.OnNext(packageWithId.message);
        }
    }
}
