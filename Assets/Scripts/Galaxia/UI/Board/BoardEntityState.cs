using System;
using Unity.Collections;
using Unity.Netcode;

public struct BoardEntityState : INetworkSerializable, IEquatable<BoardEntityState>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public int Golds;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Golds);
    }

    public bool Equals(BoardEntityState other)
    {
        return ClientId == other.ClientId && PlayerName.Equals(other.PlayerName) && Golds == other.Golds;
    }
}
