using System.Buffers;
using MagicOnion.Serialization;
using MemoryPack;

namespace Map.Shared.Extensions;

public class MemoryPackMagicOnionSerializer : IMagicOnionSerializer
{
    public T Deserialize<T>(byte[] bytes)
    {
        try
        {
            return MemoryPackSerializer.Deserialize<T>(bytes)
                   ?? throw new InvalidOperationException("Deserialized value is null");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize type {typeof(T)}", ex);
        }
    }

    public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
    {
        try
        {
            return MemoryPackSerializer.Deserialize<T>(bytes)
                   ?? throw new InvalidOperationException("Deserialized value is null");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize type {typeof(T)}", ex);
        }
    }

    public byte[] Serialize<T>(in T value)
    {
        try
        {
            return MemoryPackSerializer.Serialize(value);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to serialize type {typeof(T)}", ex);
        }
    }

    public void Serialize<T>(IBufferWriter<byte> writer, in T value)
    {
        try
        {
            MemoryPackSerializer.Serialize(writer, value);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to serialize type {typeof(T)}", ex);
        }
    }
}