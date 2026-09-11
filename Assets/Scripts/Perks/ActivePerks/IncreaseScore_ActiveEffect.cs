using UnityEngine;

[CreateAssetMenu(fileName = "ScoreIncreaseEffect", menuName = "Perks/Active Perk Effects/Score Increase")]
public class IncreaseScore_ActiveEffect : PerkEffect
{
    [SerializeField] private int scoreIncreaseAmount = 0;

    public override void OnUsed(GameplayManager context, float multiplier)
    {
        context.AddScore(Mathf.RoundToInt(scoreIncreaseAmount * multiplier));
        context.UI.UpdateScore(context.Score, context.ScoreMultiplier);
    }

    public override string GetDescription(float multiplier)
    {
        return string.Format(perkDescription, Mathf.RoundToInt(scoreIncreaseAmount * multiplier));
    }
}
