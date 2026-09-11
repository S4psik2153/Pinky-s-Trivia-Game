using System.Collections.Generic;
using UnityEngine;

public class PerkManager
{
    private readonly GameplayManager context;
    private readonly PerkConfiguration perkConfig;
    private readonly PerkPool perkPool;

    private readonly List<Perk> activePerks;
    public List<Perk> ActivePerks => new(activePerks);

    private readonly List<Perk> passivePerks;
    public List<Perk> PassivePerks => new(passivePerks);

    private int inventoryCapacity = 4;
    public int InventoryCapacity { get => inventoryCapacity; set => inventoryCapacity = value; }

    public bool IsInventoryFull => activePerks.Count >= inventoryCapacity;

    private int perkChoiceCount = 3;
    public int PerkChoiceCount { get => perkChoiceCount; set => perkChoiceCount = value; }

    private readonly List<Perk> availablePerks;

    private readonly List<ActiveEffect> passiveEffects = new();

    private readonly PerkGenerator perkGenerator;

    public PerkManager(GameplayManager context, PerkPool perkPool)
    {
        this.context = context;
        this.perkPool = perkPool;

        activePerks = new List<Perk>();
        passivePerks = new List<Perk>();

        availablePerks = perkPool != null ? perkPool.GetAllPerks() : new();

        perkConfig = context.PerkConfig;

        perkGenerator = new PerkGenerator(context.EffectPool, perkConfig);
    }

    public bool AddPerk(Perk perkAsset)
    {
        if (perkAsset == null)
        {
            Debug.LogWarning("Perk asset is null. Cannot add perk.");
            return false;
        }

        Perk perk = Object.Instantiate(perkAsset);

        perk.PerkMultiplier = context.PerkMultiplier * context.TemporaryPerkMultiplier;
        context.TemporaryPerkMultiplier = 1f;

        switch (perk.Type)
        {
            case Perk.PerkType.Passive:
            {
                passivePerks.Add(perk);

                perk.OnAcquired(context);
                
                foreach (PerkEffectEntry effectEntry in perk.Effects)
                {
                    if (effectEntry.effect == null)
                    {
                        Debug.LogWarning($"PerkEffectEntry in perk '{perk.Name}' has a null effect. Skipping this effect.");
                        continue;
                    }

                    ActiveEffect activeEffect = new()
                    {
                        perk = perk,
                        entry = effectEntry
                    };

                    passiveEffects.Add(activeEffect);
                }

                passiveEffects.Sort((a, b) => b.entry.effect.PerkPriority.CompareTo(a.entry.effect.PerkPriority));
            }
            break;

            case Perk.PerkType.Active:
            {
                if (IsInventoryFull)
                {
                    Debug.LogWarning("Active perk inventory is full. Cannot add more active perks.");

                    Object.Destroy(perk);

                    return false;
                }

                activePerks.Add(perk);

                perk.OnAcquired(context);
            }
            break;

            default:
            {
                Debug.LogWarning("Unknown perk type. Cannot add perk.");
                return false;
            }
        }

        return true;
    }

    public void RemovePerk(Perk perk)
    {
        if (perk == null)
        {
            Debug.LogWarning("Perk to remove is null. Cannot remove perk.");
            return;
        }

        if (activePerks.Contains(perk))
        {
            perk.OnRemoved(context);

            activePerks.Remove(perk);
        }
        else if (passivePerks.Contains(perk))
        {
            perk.OnRemoved(context);

            passivePerks.Remove(perk);

            passiveEffects.RemoveAll(activeEffect => activeEffect.perk == perk);
        }
        else
        {
            Debug.LogWarning("Perk not found in the list. Cannot remove perk.");
            return;
        }

        Object.Destroy(perk);
    }

    public int ModifyScore(int score, bool? isCorrect)
    {
        foreach (ActiveEffect activeEffect in passiveEffects)
        {
            if (!activeEffect.perk.IsPerkActive) continue;

            float multiplier = activeEffect.perk.GetEffectMultiplier(activeEffect.entry);
            score = activeEffect.entry.effect.ModifyScore(score, context, isCorrect, multiplier);
        }

        return score;
    }

    public float ModifyDuration(float duration)
    {
        foreach (ActiveEffect activeEffect in passiveEffects)
        {
            if (!activeEffect.perk.IsPerkActive) continue;

            float multiplier = activeEffect.perk.GetEffectMultiplier(activeEffect.entry);
            duration = activeEffect.entry.effect.ModifyDuration(duration, context, multiplier);
        }

        return duration;
    }

    public List<Perk> GetRandomPerks(int count)
    {
        List<Perk> randomPerks = new();

        if (availablePerks.Count < count)
        {
            availablePerks.AddRange(perkGenerator.GeneratePerks(count - availablePerks.Count, SelectProfile()));
        }

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availablePerks.Count);
            Perk selectedPerk = availablePerks[randomIndex];

            availablePerks.RemoveAt(randomIndex);

            randomPerks.Add(selectedPerk);
        }

        foreach (Perk perk in randomPerks)
        {
            perk.PerkMultiplier = context.PerkMultiplier * context.TemporaryPerkMultiplier;
        }

        return randomPerks;
    }

    public bool UsePerk(Perk perk, int choiceIndex = -1)
    {
        if (perk == null)
        {
            Debug.LogError("Cannot use a null perk.");

            return false;
        }

        if (!activePerks.Contains(perk))
        {
            Debug.LogWarning("Cannot use a non active perk.");

            return false;
        }

        if (!perk.IsPerkActive)
        {
            return false;
        }

        if (perk.TargetType == Perk.PerkTargetType.Choice && choiceIndex < 0)
        {
            Debug.LogError("Invalid perk target type or selected index.");

            return false;
        }

        if (perk.TargetType == Perk.PerkTargetType.Instant)
        {
            perk.OnUsed(context);
        }
        else if (perk.TargetType == Perk.PerkTargetType.Choice)
        {
            perk.OnUsedOnChoice(context, choiceIndex);

            perk.OnUsed(context);
        }
        else
        {
            Debug.LogError("Invalid perk target type.");

            return false;
        }

        perk.ConsumeUse();

        if (perk.IsExhausted)
        {
            RemovePerk(perk);
        }

        return true;
    }

    public void TickQuestionCooldowns()
    {
        if (activePerks == null || activePerks.Count <= 0)
        {
            return;
        }

        foreach (Perk perk in activePerks)
        {
            if (perk == null) continue;

            perk.TickQuestionCooldown();
        }
    }

    public void TickDurationCooldowns(float deltaTime)
    {
        if (activePerks == null || activePerks.Count <= 0)
        {
            return;
        }

        foreach (Perk perk in activePerks)
        {
            if (perk == null) continue;

            perk.TickCooldownDuration(deltaTime);
        }
    }

    private PerkGenerator.TokenProfile SelectProfile()
    {
        if (context.CurrentQuestionNumber == context.TotalQuestionCount / 2)
        return PerkGenerator.TokenProfile.Negative;

        float rng = Random.value;

        if (rng < perkConfig.negativePerkChance)                                    return PerkGenerator.TokenProfile.Negative;

        if (rng < perkConfig.negativePerkChance + perkConfig.positivePerkChance)    return PerkGenerator.TokenProfile.Positive;

        return PerkGenerator.TokenProfile.Balanced;
    }

    private class ActiveEffect
    {
        public Perk perk;
        public PerkEffectEntry entry;
    }
}