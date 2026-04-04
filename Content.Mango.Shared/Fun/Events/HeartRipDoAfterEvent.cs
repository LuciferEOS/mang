using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Mango.Shared.Fun.Events;

[Serializable, NetSerializable]
public sealed partial class HeartRipDoAfterEvent : SimpleDoAfterEvent {}
