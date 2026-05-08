namespace Content.Medical.Common._Inky;

/// <summary>
/// Einstein 650iq braincomponent duplicate that has a shitton of fields to not bloat upstream braincomp
/// </summary> // todo inkymed
[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class InkyBrainComponent : Component
{
    [AutoNetworkedField, ViewVariables]
    public BrainState CurrentState { get; set; } = BrainState.Alive;

    [DataField]
    [AutoNetworkedField]
    public HashSet<BrainState> AllowedStates = new()
    {
        BrainState.Alive,
        BrainState.Critical,
        BrainState.Dead
    };

    [AutoNetworkedField, ViewVariables]
    public float Consciousness = 100f;

    /// <summary>
    /// Below what number should you go into BrainState.Critical
    /// </summary>
    [ViewVariables]
    public float ConsciousnessUnconciousnessThreshold = 40f;

    [DataField]
    public float ConsciousnessHealRate = 1f;

    [DataField]
    public float HealModifier = 1f;
}
