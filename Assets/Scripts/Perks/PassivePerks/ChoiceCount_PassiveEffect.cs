using UnityEngine;

[CreateAssetMenu(fileName = "ChoiceCountEffect", menuName = "Perks/Passive Perk Effects/Choice Count")]
public class ChoiceCount_PassiveEffect : PerkEffect
{
    [SerializeField] private int additionCount = 1;

    public override void OnAcquired(GameplayManager context, float multiplier)
    {
        int count = Mathf.RoundToInt(additionCount * multiplier);

        if (count > 0)
        {
            context.ExtraChoiceCount += count;
        }
        else if (count < 0)
        {
            context.RemovedChoiceCount += -count;
        }
    }

    public override string GetDescription(float multiplier)
    {
        return string.Format(perkDescription, Mathf.Abs(Mathf.RoundToInt(additionCount * multiplier)));
    }
}