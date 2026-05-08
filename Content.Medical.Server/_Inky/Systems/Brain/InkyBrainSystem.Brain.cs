using Content.Medical.Common._Inky;
using Content.Medical.Shared._Inky.Components;

namespace Content.Medical.Server._Inky.Systems.Brain;

public sealed partial class InkyBrainSystem
{
    private float _brainUpdateTimer = 0f; // todo inkymed maybe put inside a comp?
    private const float BrainUpdateInterval = 2f;

    private void InitializeBrain()
    {
    }

    private void UpdateBrainHealth(float frameTime)
    {
        _brainUpdateTimer += frameTime;
        if (_brainUpdateTimer < BrainUpdateInterval)
            return;

        _brainUpdateTimer -= BrainUpdateInterval;

        var eqe = EntityQueryEnumerator<InkyBrainDamageableComponent, InkyBrainComponent>();
        while (eqe.MoveNext(out var uid, out var brainDmgComp, out var brainComp))
        {
            UpdateConsciousness(uid, brainDmgComp, brainComp);
        }
    }
    private void UpdateConsciousness(EntityUid uid, InkyBrainDamageableComponent brainDmgComp, InkyBrainComponent brainComp)
    {
        // consciousness is absolute-capped by your total brain health, it means that
        // at 65 brain health, your absolute consc cap will be 65
        if (brainDmgComp.OxygenLevel <= 0f)
            DoBrainDamage(uid, brainDmgComp, 2f); // todo inkymed use events
        brainComp.Consciousness = Math.Min(brainComp.Consciousness, brainDmgComp.BrainHealth);

        if (brainComp.Consciousness < brainComp.ConsciousnessUnconciousnessThreshold)
        {
            if (brainComp.CurrentState != BrainState.Critical)
            {
                brainComp.CurrentState = BrainState.Critical; // todo inkymed replace with events
                Dirty(uid, brainComp);
            }
        }
        else if (brainComp.CurrentState == BrainState.Critical)
        {
            brainComp.CurrentState = BrainState.Alive; // todo inkymed use events
            Dirty(uid, brainComp);
        }
    }

    public void DoBrainDamage(EntityUid uid, InkyBrainDamageableComponent brainDmgComp, float amount)
    {
        if (!_iBrainQuery.TryComp(uid, out var brainComp))
            return;

        brainDmgComp.BrainHealth -= amount;
        brainDmgComp.BrainHealth = Math.Max(brainDmgComp.BrainHealth, 0f);

        brainComp.Consciousness = Math.Min(brainComp.Consciousness, brainDmgComp.BrainHealth);

        if (brainComp.Consciousness < brainComp.ConsciousnessUnconciousnessThreshold)
            brainComp.CurrentState = BrainState.Critical; // todo inkymed use events

        Dirty(uid, brainDmgComp);
        Dirty(uid, brainComp);
    }

    public void RestoreConsciousness(EntityUid uid, float amount) // todo inkymed
    {
        if (!_iBrainQuery.TryComp(uid, out var brainComp))
            return;

        if (!TryGetShitBrain(uid, out var brainDmgComp)
            || brainDmgComp == null)
            return;

        brainComp.Consciousness += amount;
        brainComp.Consciousness = Math.Min(brainComp.Consciousness, brainDmgComp.BrainHealth);

        Dirty(uid, brainComp);
    }
}
