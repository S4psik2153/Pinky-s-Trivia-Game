using UnityEngine;

[CreateAssetMenu(fileName = "PerkBufferEffect", menuName = "Perks/Passive Perk Effects/Perk Buffer")]
public class PerkBuffer_PassiveEffect : PerkEffect
{
    [SerializeField] private float boostAmount = 1f;

    [SerializeField] private bool isPermanent;

    public override void OnAcquired(GameplayManager context, float multiplier)
    {
        if (isPermanent) context.PerkMultiplier *= 1f + boostAmount * multiplier;
        else             context.TemporaryPerkMultiplier *= 1f + boostAmount * multiplier;
    }

    public override string GetDescription(float multiplier)
    {
        return string.Format(perkDescription, Mathf.RoundToInt(boostAmount * multiplier * 100f));
    }
}
