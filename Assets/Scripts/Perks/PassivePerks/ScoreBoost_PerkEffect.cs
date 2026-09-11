using UnityEngine;

[CreateAssetMenu(fileName = "ScoreBoostEffect", menuName = "Perks/Passive Perk Effects/Score Boost")]
public class ScoreBoost_PerkEffect : PerkEffect
{
    [SerializeField] private int scoreBonusAmount = 0;

    public override int ModifyScore(int score, GameplayManager context, bool? isCorrect, float multiplier)
    {
        if (isCorrect == true)
        {
            return score + Mathf.RoundToInt(scoreBonusAmount * multiplier);
        }

        return score;
    }

    public override string GetDescription(float multiplier)
    {
        return string.Format(perkDescription, Mathf.RoundToInt(scoreBonusAmount * multiplier));
    }
}
