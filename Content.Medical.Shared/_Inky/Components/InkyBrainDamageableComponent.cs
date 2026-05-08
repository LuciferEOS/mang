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
    public List<ProtoId<DamageContainerPrototype>> DamageContainers = new() // todo inkymed obsolete?
    {
        "Brain"
    };

    [AutoNetworkedField, ViewVariables]
    public float BrainHealth = 100f;

    [ViewVariables, AutoNetworkedField]
    public float TotalDamage { get; set; } // todo inkymed isnt used

    /// <summary>
    /// Current oxygen level of the brain.
    /// if it reaches 0, the brain starts to recieve damage
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public float OxygenLevel = 100f;

    /// <summary>
    /// Ammount of oxygen being consumed by the brain every 2 seconds
    /// </summary>
    [DataField]
    public float ConsumeRate = 4f;
}
