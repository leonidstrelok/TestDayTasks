using System.Reflection;
using Grpc.Core;
using MagicOnion.Serialization;

namespace Map.Shared.Extensions;

public class MemoryPackMagicOnionSerializerProvider : IMagicOnionSerializerProvider
{
    private readonly IMagicOnionSerializer _serializer;

    public MemoryPackMagicOnionSerializerProvider()
    {
        _serializer = new MemoryPackMagicOnionSerializer();
    }

    public IMagicOnionSerializer Create(MethodType methodType, MethodInfo? methodInfo)
    {
        return _serializer;
    }
}