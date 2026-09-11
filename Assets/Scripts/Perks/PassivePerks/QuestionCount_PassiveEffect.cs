using UnityEngine;

[CreateAssetMenu(fileName = "QuestionCountEffect", menuName = "Perks/Passive Perk Effects/Question Count")]
public class QuestionCount_PassiveEffect : PerkEffect
{
    [Tooltip("Pozitif değer soru ekler, negatif değer soru çıkarır.")]
    [SerializeField] private int questionAmount = 1;

    public override void OnAcquired(GameplayManager context, float multiplier)
    {
        int count = Mathf.RoundToInt(questionAmount * multiplier);

        if (count > 0)
        {
            for (int i = 0; i < count; i++) context.AddQuestion();
        }
        else
        {
            for (int i = 0; i < -count; i++) context.RemoveQuestion();
        }
    }

    public override string GetDescription(float multiplier)
    {
        return string.Format(perkDescription, Mathf.Abs(Mathf.RoundToInt(questionAmount * multiplier)));
    }
}
