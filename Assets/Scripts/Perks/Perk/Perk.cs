using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Perk", menuName = "Perks/Perk")]
public class Perk : ScriptableObject
{
    public enum PerkType { Passive, Active }

    [SerializeField] private string perkName;
    [SerializeField] private Sprite perkIcon;
    [SerializeField] private Sprite perkMiniIcon;
    [SerializeField] private PerkType perkType;

    [SerializeField] private List<PerkEffectEntry> effects;
    public IReadOnlyList<PerkEffectEntry> Effects => effects;

    private bool isPerkActive = true;
    public bool IsPerkActive => isPerkActive;

    private float perkMultiplier = 1f;
    public float PerkMultiplier { get => perkMultiplier; set => perkMultiplier = value; }

    public string Name => perkName;
    public Sprite Icon => perkIcon;
    public Sprite MiniIcon => perkMiniIcon;
    public PerkType Type => perkType;

    public enum PerkTargetType { Instant, Choice }
    [SerializeField] private PerkTargetType targetType;
    public PerkTargetType TargetType => targetType;

    [Tooltip("Aktif perklerin kullanım miktarı. -1 sınırsız kullanım.")]
    [SerializeField] private int useCount = 1;
    public int UseCount => useCount;

    [Tooltip("Aktif perklerin kullanım kullanım sonrası saniye cinsinden beklemeye girme süresi. -1 olan perk kullanıldıktan sonra beklemeye girmez pasifleşir, 0 bekleme yok anında yeniden dolum.")]
    [SerializeField] private float cooldownDuration = -1f;
    public float CooldownDuration => cooldownDuration;

    [Tooltip("Aktif perklerin kullanım kullanım sonrası soru sayısı cinsinden beklemeye girme süresi. -1 olan perk kullanıldıktan sonra beklemeye girmez pasifleşir, 0 bekleme yok anında yeniden dolum.")]
    [SerializeField] private int cooldownQuestion = -1;
    public int CooldownQuestion => cooldownQuestion;

    private int remainingUses;
    public int RemainingUses => remainingUses;

    private float cooldownRemaining;
    public float CooldownRemaining => cooldownRemaining;

    private int questionRemaining;
    public int QuestionRemaining => questionRemaining;

    public bool IsExhausted => useCount != -1 && remainingUses <= 0;

    public float TotalTokenValue
    {
        get
        {
            float totalTokenValue = 0f;

            if (effects == null || effects.Count == 0)
            {
                Debug.LogWarning($"Perk '{perkName}' has no effects. TotalTokenValue will be 0.");
                return totalTokenValue;
            }

            foreach (PerkEffectEntry effectEntry in effects)
            {
                if (effectEntry?.effect == null)
                {
                    Debug.LogError($"A null effect entry found in perk '{perkName}'.", this);

                    continue;
                }

                totalTokenValue += effectEntry.effect.TokenValue * GetEffectMultiplier(effectEntry);
            }

            return totalTokenValue;
        }
    }

    /// <summary>
    /// Yalnızca <see cref="PerkGenerator"/> tarafından çağrılmalı, elle oluşturulan asset'lerde kullanılmaz.
    /// </summary>
    /// <param name="name">
    /// <see cref="Perk"/> adı verisi.
    /// </param>
    /// <param name="icon">
    /// <see cref="Perk"/> iconu verisi.
    /// </param>
    /// <param name="miniIcon">
    /// <see cref="PerkType.Passive"/> perklerin
    /// oyun sırasında takip edilmesini sağlayan icon verisi
    /// </param>
    /// <param name="type">
    /// <see cref="PerkType"/> verisi.
    /// </param>
    /// <param name="effects">
    /// Perkün içerdiği <see cref="PerkEffect"/> özelliklerinin listesi.
    /// </param>
    public void Initialize(PerkData perkData, PerkType type, List<PerkEffectEntry> effects)
    {
        perkName = perkData.Name;
        perkIcon = perkData.Icon;
        perkMiniIcon = perkData.MiniIcon;
        perkType = type;
        this.effects = new List<PerkEffectEntry>(effects);

        targetType = PerkTargetType.Instant;

        foreach (PerkEffectEntry effect in this.effects)
        {
            if (effect.effect.RequiresChoice)
            {
                targetType = PerkTargetType.Choice;

                break;
            }
        }
    }

    private void OnValidate()
    {
        if (effects == null || effects.Count == 0)
        {
            Debug.LogWarning($"Perk '{perkName}' has no effects assigned. Please assign at least one PerkEffect.", this);
        }
        else
        {
            foreach (var effectEntry in effects)
            {
                if (effectEntry.effect == null)
                {
                    Debug.LogWarning($"Perk '{perkName}' has a null effect entry. Please assign a valid PerkEffect.", this);
                }

                if (perkType == PerkType.Active && effectEntry.effect.RequiresChoice && targetType != PerkTargetType.Choice)
                {
                    Debug.LogError($"A invalid perk target type found in perk '{perkName}'.", this);
                }
            }
        }
    }

    public float GetEffectMultiplier(PerkEffectEntry effectEntry)
    {
        if (effectEntry == null)
        {
            Debug.LogWarning($"Effect entry is null in perk '{perkName}'. Returning default multiplier of 1.");
            return 1f;
        }

        return perkMultiplier * effectEntry.multiplier;
    }

    public void OnAcquired(GameplayManager context)
    {
        if (effects == null || effects.Count == 0)
        {
            Debug.LogWarning($"Perk '{perkName}' has no effects to apply.");
            return;
        }

        remainingUses = useCount;

        cooldownRemaining = 0f;
        questionRemaining = 0;

        foreach (PerkEffectEntry effectEntry in effects)
        {
            if (effectEntry?.effect == null)
            {
                Debug.LogError($"A null effect entry found in perk '{perkName}'.", this);

                continue;
            }

            float multiplier = GetEffectMultiplier(effectEntry);
            effectEntry.effect.OnAcquired(context, multiplier);
        }
    }

    public void OnRemoved(GameplayManager context)
    {
        if (effects == null || effects.Count == 0)
        {
            Debug.LogWarning($"Perk '{perkName}' has no effects to remove.");
            return;
        }

        foreach (PerkEffectEntry effectEntry in effects)
        {
            if (effectEntry?.effect == null)
            {
                Debug.LogError($"A null effect entry found in perk '{perkName}'.", this);

                continue;
            }

            float multiplier = GetEffectMultiplier(effectEntry);
            effectEntry.effect.OnRemoved(context, multiplier);
        }
    }

    public void ActivatePerk()
    {
        isPerkActive = true;
    }

    public void DeactivatePerk()
    {
        isPerkActive = false;
    }

    public string GetDescription()
    {
        if (effects == null || effects.Count == 0)
        {
            Debug.LogWarning($"Perk '{perkName}' has no effects. Returning empty description.");
            return string.Empty;
        }

        string description = "";

        foreach (PerkEffectEntry effectEntry in effects)
        {
            if (effectEntry?.effect == null)
            {
                Debug.LogError($"A null effect entry found in perk '{perkName}'.", this);

                continue;
            }
            
            description += "- " + effectEntry.effect.GetDescription(GetEffectMultiplier(effectEntry)) + "\n";
        }

        return description.TrimEnd('\n');
    }

    public void OnUsed(GameplayManager context)
    {
        if (effects == null || effects.Count == 0)
        {
            Debug.LogWarning($"Perk '{perkName}' has no effects to apply.");
            return;
        }

        foreach (var effectEntry in effects)
        {
            if (effectEntry?.effect == null)
            {
                Debug.LogError($"A null effect entry found in perk '{perkName}'.", this);

                continue;
            }

            float multiplier = GetEffectMultiplier(effectEntry);
            effectEntry.effect.OnUsed(context, multiplier);
        }
    }

    public void OnUsedOnChoice(GameplayManager context, int choiceIndex)
    {
        if (effects == null || effects.Count == 0)
        {
            Debug.LogWarning($"Perk '{perkName}' has no effects to apply.");
            return;
        }

        foreach (var effectEntry in effects)
        {
            if (effectEntry?.effect == null)
            {
                Debug.LogError($"A null effect entry found in perk '{perkName}'.", this);

                continue;
            }

            float multiplier = GetEffectMultiplier(effectEntry);
            effectEntry.effect.OnUsedOnChoice(context, choiceIndex, multiplier);
        }
    }

    public void ConsumeUse(int consumedUsage = 1)
    {
        remainingUses -= consumedUsage;

        DeactivatePerk();

        cooldownRemaining = cooldownDuration == -1 ? 0f : cooldownDuration;
        questionRemaining = cooldownQuestion == -1 ? 0 : cooldownQuestion;
    }

    public void TickCooldownDuration(float cooldownTick = 1f)
    {
        if (cooldownDuration == -1 || cooldownRemaining <= 0) return;

        cooldownRemaining -= cooldownTick;

        if (cooldownRemaining <= 0 && (remainingUses > 0 || useCount == -1))
        {
            ActivatePerk();
        }
    }

    public void TickQuestionCooldown(int questionCooldownTick = 1)
    {
        if (cooldownQuestion == -1 || questionRemaining <= 0) return;

        questionRemaining -= questionCooldownTick;

        if (questionRemaining <= 0 && (remainingUses > 0 || useCount == -1))
        {
            ActivatePerk();
        }
    }
}

[Serializable]
public class PerkEffectEntry
{
    public PerkEffect effect;
    public int multiplier = 1;
}
