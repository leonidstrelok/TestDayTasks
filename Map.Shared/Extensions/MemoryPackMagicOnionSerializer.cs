using System.Buffers;
using System.Reflection;
using Grpc.Core;
using MagicOnion.Serialization;
using MemoryPack;

namespace Map.Shared.Extensions;

public class MemoryPackMagicOnionSerializer : IMagicOnionSerializer, IMagicOnionSerializerProvider
{
    public T Deserialize<T>(byte[] bytes)
    {
        return MemoryPackSerializer.Deserialize<T>(bytes) 
               ?? throw new InvalidOperationException("Deserialized value is null");
    }

    public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
    {
        return MemoryPackSerializer.Deserialize<T>(bytes) 
               ?? throw new InvalidOperationException("Deserialized value is null");
    }

    public byte[] Serialize<T>(in T value)
    {
        return MemoryPackSerializer.Serialize(value);
    }

    public void Serialize<T>(IBufferWriter<byte> writer, in T value)
    {
        MemoryPackSerializer.Serialize(writer, value);
    }

    public IMagicOnionSerializer Create(MethodType methodType, MethodInfo? methodInfo)
    {
        throw new NotImplementedException();
    }
}