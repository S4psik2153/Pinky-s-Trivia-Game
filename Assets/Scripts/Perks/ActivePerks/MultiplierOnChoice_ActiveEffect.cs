using UnityEngine;

[CreateAssetMenu(fileName = "MultiplierOnChoiceEffect", menuName = "Perks/Active Perk Effects/Multiplier On Choice")]
public class MultiplierOnChoice_ActiveEffect : PerkEffect
{
    [SerializeField] private float choiceScoreMultiplier = 1f;

    public override bool RequiresChoice => true;

    public override void OnUsedOnChoice(GameplayManager context, int choiceIndex, float multiplier)
    {
        context.MarkedChoiceIndex = choiceIndex;
        context.MarkedChoiceMultiplier *= Mathf.Max(0f, 1 + choiceScoreMultiplier * multiplier);
    }

    public override string GetDescription(float multiplier)
    {
        return string.Format(perkDescription, Mathf.Abs(Mathf.RoundToInt(choiceScoreMultiplier * multiplier * 100f)));
    }
}
