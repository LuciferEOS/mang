using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Inky.Shared.Cyber;

[RegisterComponent]
public sealed partial class FastTrakkConsoleComponent : Component
{
    // holy protoid shitpost
    [DataField]
    public EntProtoId RemoteEntityProto = "AbductorHumanObservationConsoleEye"; // todo

    [DataField]
    public EntProtoId HologramSpawner = "HologramFastTrakk";

    [DataField]
    public EntProtoId HologramSpawnerDeparture = "HologramFastTrakkDeparture";

    [DataField]
    public EntProtoId<ActionComponent> DeployAction = "ActionFastTrakkDeploy";

    [DataField]
    public TimeSpan TeleportDelay = TimeSpan.FromSeconds(7);

    [DataField]
    public float PadRadius = 1.5f;

    [DataField]
    public int DestinationRadius = 1;
}
