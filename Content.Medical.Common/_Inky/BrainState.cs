namespace Content.Medical.Common._Inky;

[Serializable, NetSerializable]
public enum BrainState : byte
{
    Invalid = 0,
    Alive = 1,
    Critical = 2,
    Dead = 3
}
