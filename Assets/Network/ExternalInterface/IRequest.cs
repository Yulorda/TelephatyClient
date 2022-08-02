using Cysharp.Threading.Tasks;

public interface IRequest
{
    UniTask<T> Request<T, U>(DBOp dBOp, U obj);
    UniTask<T> Request<T, U>(U obj);
}
