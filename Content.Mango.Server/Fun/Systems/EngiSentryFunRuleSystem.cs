using Content.Mango.Server.Fun.Components;
using Content.Mango.Server.Fun.Components.Rules;
using Content.Server.GameTicking.Rules;
using Content.Shared.Damage.Systems;
using Content.Shared.Stacks;

namespace Content.Mango.Server.Fun.Systems;

public sealed class EngiSentryFunRuleSystem : GameRuleSystem<EngiSentryFunRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnOnDamageComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SpawnOnDamageComponent, DamageChangedEvent>(OnDamaged);
    }

    private void OnInit(EntityUid uid, SpawnOnDamageComponent comp, ComponentInit args)
    {
        var ruleActive = false;
        var eqe = EntityQueryEnumerator<EngiSentryFunRuleComponent>();
        while (eqe.MoveNext(out _, out _))
        {
            ruleActive = true;
            break;
        }
        if (!ruleActive)
            return;
        comp.Active = true;
    }

    private void OnDamaged(EntityUid uid, SpawnOnDamageComponent comp, DamageChangedEvent args)
    {
        if (!comp.Active
            || args.DamageDelta == null
            || !args.DamageIncreased)
            return;

        if (comp.EngiOnly
            || args.Origin != null && !HasComp<MangoEngineeringStaffComponent>(args.Origin.Value))
            return;

        if (TryComp<StackComponent>(uid, out var stack) && stack.Count != comp.StackRequired)
            return;

        var xform = Transform(uid);
        Spawn(comp.Spawn, xform.Coordinates);
        QueueDel(uid);
    }
}
