using UnityEngine;

[CreateAssetMenu(fileName = "TimerBoostEffect", menuName = "Perks/Passive Perk Effects/Timer Boost")]
public class TimerBoost_PerkEffect : PerkEffect
{
    [SerializeField] private float timerBonusAmount = 0f;

    public override float ModifyDuration(float timer, GameplayManager context, float multiplier)
    {
        return timer + Mathf.RoundToInt(timerBonusAmount * multiplier);
    }

    public override string GetDescription(float multiplier)
    {
        return string.Format(perkDescription, Mathf.RoundToInt(timerBonusAmount * multiplier));
    }
}
