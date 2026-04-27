using Content.Shared.Body.Components;

namespace Content.Server.Body.Components;

public sealed partial class RespiratorComponent
{
    [ViewVariables]
    public LungComponent? Lungs; // nullable because some entities dont have lungs (I GUESS)

    /// <summary>
    /// Maximum total asphyxiation damage an entity can take from suffocation
    /// if null the damage is uncapped
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public float? SuffocationDamageCap = null;

    // TODO INKYMED /\ THIS SHIT MAY NOT BE USED RN
}
