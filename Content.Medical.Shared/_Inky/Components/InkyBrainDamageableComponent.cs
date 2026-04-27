using Content.Shared.Damage.Prototypes;

namespace Content.Medical.Shared._Inky.Components;

/// <summary>
/// Handles damage stuff for brain, which is different from InternalOrgan.IntegrityThresholds
/// </summary> // TODO INKYMED merge with the comp above i guess??
[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class InkyBrainDamageableComponent : Component
{
    [DataField] public string OxyAlertId = "BrainOxy";

    [DataField]
    [AutoNetworkedField]
    public List<ProtoId<DamageContainerPrototype>> DamageContainers = new()
    {
        "Brain"
    };

    [AutoNetworkedField, ViewVariables]
    public float TotalDamage { get; set; } // larp yes
    // TODO INKYMED /\ ISNT YET IMPLEMENTED

    /// <summary>
    /// Current oxygen level of the brain.
    /// if it reaches 0, the brain starts to recieve damage
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public float OxygenLevel = 100f;
    // TODO INKYMED /\ ISNT YET IMPLEMENTED
}
