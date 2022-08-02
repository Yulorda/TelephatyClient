using System;

[Serializable]
public class PackageWithId<T>
{
    public string id;
    public T message;
}

[Serializable]
public class DBOperation<T> : PackageWithId<T>
{
    public DBOp dBOp;
}

public enum DBOp
{
    Create,
    Read,
    Update,
    Delete,
}