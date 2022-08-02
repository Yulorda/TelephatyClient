using System;

namespace Serializator
{
    public interface ISerializator
    {
        bool TryDeserialize(byte[] message, out Type type, out object networkPackage);

        byte[] Serialize(object networkPackage);
    }
}