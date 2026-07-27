namespace Content.Inky.Server.Cyber;

[RegisterComponent]
public sealed partial class FastTrakkOperatorComponent : Component
{
    [DataField] public EntityUid Console;
    [DataField] public EntityUid Eye;
    [DataField] public EntityUid? DeployAction;
    [DataField] public List<EntityUid> RemovedActions = [];
}
