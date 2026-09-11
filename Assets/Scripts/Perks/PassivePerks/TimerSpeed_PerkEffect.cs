using UnityEngine;

[CreateAssetMenu(fileName = "TimerSpeedEffect", menuName = "Perks/Passive Perk Effects/Timer Speed")]
public class TimerSpeed_PerkEffect : PerkEffect
{
    [SerializeField] private float timerSpeedRate = 0f;

    public override void OnAcquired(GameplayManager context, float multiplier)
    {
        context.TimerSpeed = Mathf.Max(0f, (1 + timerSpeedRate * multiplier) * context.TimerSpeed);
    }

    public override string GetDescription(float multiplier)
    {
        return string.Format(perkDescription, Mathf.Abs(Mathf.RoundToInt(timerSpeedRate * multiplier * 100f)));
    }
}
