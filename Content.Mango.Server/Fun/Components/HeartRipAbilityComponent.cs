namespace Content.Mango.Server.Fun.Components;

[RegisterComponent]
public sealed partial class HeartRipAbilityComponent : Component
{
    [DataField]
    public TimeSpan Duration =  TimeSpan.FromSeconds(6);

    [DataField]
    public bool ActiveOnlyWhenFunned;
}
