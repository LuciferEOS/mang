using Robust.Shared.Audio;

namespace Content.Mango.Server.Fun.Components;

[RegisterComponent]
public sealed partial class HeartRipAbilityComponent : Component
{
    [DataField]
    public TimeSpan Duration =  TimeSpan.FromSeconds(6);

    [DataField]
    public bool ActiveOnlyWhenFunned;

    [DataField]
    public TimeSpan ParalyzeTime =  TimeSpan.FromSeconds(6);

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/gib1.ogg");
}
