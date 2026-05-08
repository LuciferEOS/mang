using Content.Medical.Common._Inky;
using Content.Medical.Shared._Inky.Components;
using Content.Medical.Shared.Body;
using Content.Server.Body.Systems;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Damage.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Medical.Server._Inky.Systems.Brain;

/// <summary>
/// Mobstate 2
/// </summary>
public sealed partial class InkyBrainSystem : EntitySystem
{
    // [Dependency] private readonly DamageableSystem _dmgSys = default!; // todo inkymed?
    [Dependency] private readonly BodySystem _body = default!;

    private EntityQuery<InkyBrainDamageableComponent> _brainQuery;
    private EntityQuery<BodyStatusComponent> _bodyStatusQuery;
    private EntityQuery<InkyBrainComponent> _iBrainQuery;

    private EntityQuery<LungComponent> _lungQuery; // SLOOOOOOOOOOOOOOOOOOOOOOOPPP todo inkymed lung own system that isnt in core and replace this shit with a proper dependency

    private float _oxygenUpdateTimer = 0f; // todo inkymed maybe put this inside a comp?
    private const float OxygenUpdateInterval = 2f;

    public static readonly ProtoId<OrganCategoryPrototype> LungsCategory = "Lungs";
    public static readonly ProtoId<OrganCategoryPrototype> BrainCategory = "Brain";

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _brainQuery = GetEntityQuery<InkyBrainDamageableComponent>();
        _iBrainQuery = GetEntityQuery<InkyBrainComponent>();
        _bodyStatusQuery = GetEntityQuery<BodyStatusComponent>();
        _lungQuery = GetEntityQuery<LungComponent>();

        InitializeOxygen();
        InitializeBrain();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateOxy(frameTime);
        UpdateBrainHealth(frameTime);
    }

    public bool TryGetLungs(EntityUid uid, out LungComponent? lungComp) // booo copypaste
    {
        lungComp = null;
        var lungOrgan = _body.GetOrgan(uid, LungsCategory);
        return lungOrgan != null && _lungQuery.TryComp(lungOrgan.Value, out lungComp);
    }

    public bool TryGetShitBrain(EntityUid uid, out InkyBrainDamageableComponent? brainComp) // booo copypaste
    {
        brainComp = null;
        var brainOrgan = _body.GetOrgan(uid, BrainCategory);
        return brainOrgan != null && _brainQuery.TryComp(brainOrgan.Value, out brainComp);
    }
}
