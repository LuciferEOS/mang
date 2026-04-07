using Content.Shared.Actions;
using Content.Shared.UserInterface;
using Robust.Shared.Serialization;

namespace Content.Mango.Shared.Name;

public abstract class SharedSelfRenamerSystem : EntitySystem
{
}

[Serializable, NetSerializable]
public sealed class SelfRenamerNameMessage : BoundUserInterfaceMessage
{
    public readonly string NewName;

    public SelfRenamerNameMessage(string newName)
    {
        NewName = newName;
    }
}
