using UnityEngine;

[CreateAssetMenu(fileName = "ChoiceEliminatorEffect", menuName = "Perks/Active Perk Effects/Choice Eliminator")]
public class ChoiceEliminator_ActiveEffect : PerkEffect
{
    [SerializeField] private int eliminationCount = 1;

    public override void OnUsed(GameplayManager context, float multiplier)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(eliminationCount * multiplier));

        for (int i = 0; i < count; i++) context.RequestChoiceElimination();
    }

    public override string GetDescription(float multiplier)
    {
        return string.Format(perkDescription, Mathf.Max(1, Mathf.RoundToInt(eliminationCount * multiplier)));
    }
}
