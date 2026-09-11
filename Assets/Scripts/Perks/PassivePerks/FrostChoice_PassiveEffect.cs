using UnityEngine;

[CreateAssetMenu(fileName = "FrostChoiceEffect", menuName = "Perks/Passive Perk Effects/Frost Choice")]
public class FrostChoice_PassiveEffect : PerkEffect
{
    [SerializeField] private int frostHits = 3;

    public override void OnAcquired(GameplayManager context, float multiplier)
    {
        context.FrozenChoices(Mathf.Max(1, Mathf.RoundToInt(frostHits * multiplier)));
    }

    public override string GetDescription(float multiplier)
    {
        return string.Format(perkDescription, Mathf.Max(1, Mathf.RoundToInt(frostHits * multiplier)));
    }
}
