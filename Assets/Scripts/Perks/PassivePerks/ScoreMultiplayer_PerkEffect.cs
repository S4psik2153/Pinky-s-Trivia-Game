using UnityEngine;

[CreateAssetMenu(fileName = "ScoreMultiplayerEffect", menuName = "Perks/Passive Perk Effects/Score Multiplayer")]
public class ScoreMultiplayer_PerkEffect : PerkEffect
{
    [SerializeField] private float scoreMultiplier = 0f;

    public override void OnAcquired(GameplayManager context, float multiplier)
    {
        context.ScoreMultiplier = Mathf.Max(0f, (1 + scoreMultiplier * multiplier) * context.ScoreMultiplier);
    }

    public override string GetDescription(float multiplier)
    {
        return string.Format(perkDescription, Mathf.Abs(Mathf.RoundToInt(scoreMultiplier * multiplier * 100f)));
    }
}
