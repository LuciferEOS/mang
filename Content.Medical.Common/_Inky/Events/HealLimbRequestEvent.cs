namespace Content.Medical.Common._Inky.Events;

[Serializable, NetSerializable]
public sealed class HealLimbRequestEvent : EntityEventArgs // todo NetMessage????
{
    public string CategoryId;
    public HealLimbRequestEvent(string categoryId) => CategoryId = categoryId;
}
