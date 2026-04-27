using Content.Medical.Common._Inky.Events;
using Content.Medical.Common.Targeting;
using Content.Medical.Shared.Body;
using Content.Medical.Shared.Targeting;
using Content.Shared.Body;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Medical;
using Content.Shared.Medical.Healing;

namespace Content.Medical.Shared._Inky.Healing;

public sealed partial class FancyHealingSystem : EntitySystem
{
    [Dependency] private readonly HealingSystem _heal = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    private static readonly Dictionary<string, TargetBodyPart> Target = new() // todo inkymed: UNFUCK ME!
    {
        { "Head", TargetBodyPart.Head },
        { "Torso", TargetBodyPart.Chest },

        { "ArmLeft", TargetBodyPart.LeftArm },
        { "ArmRight", TargetBodyPart.RightArm },

        { "LegLeft", TargetBodyPart.LeftLeg },
        { "LegRight", TargetBodyPart.RightLeg },
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<HealLimbRequestEvent>(OnHealLimbRequest);
    }

    public void RequestHealLimb(string categoryId)
        => RaiseNetworkEvent(new HealLimbRequestEvent(categoryId));

    private void OnHealLimbRequest(HealLimbRequestEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not {} user)
            return;

        var heldItem = _hands.GetActiveItem(user);
        if (heldItem == null
            || !TryComp(heldItem.Value, out HealingComponent? healingComp))
            return;

        if (!TryComp<BodyComponent>(user, out _))
            return;

        if (!Target.TryGetValue(msg.CategoryId, out var targetPart))
            return;

        if (TryComp<TargetingComponent>(user, out var targeting))
        {
            targeting.Target = targetPart;
            Dirty(user, targeting);
        }

        _heal.TryHeal((heldItem.Value, healingComp), (user, null!), user);
    }
}
