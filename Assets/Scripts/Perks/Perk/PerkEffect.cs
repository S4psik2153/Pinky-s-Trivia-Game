using System.Collections.Generic;
using UnityEngine;

public abstract class PerkEffect : ScriptableObject
{
    [SerializeField] protected string perkDescription;

    /// <summary>
    /// Perk öncelik sırasını belirten veri.
    /// Önceliği yüksek olan perkler,
    /// önceliği düşük olan perklerden önce işlenir.
    /// Öncelik sırası: çarpım -> toplama -> bölme -> çıkarma şeklinde olmalıdır.
    /// </summary>
    [SerializeField] private int perkPriority;
    public int PerkPriority => perkPriority;

    [Tooltip("Perk etkisinin oyun içi token değerini belirten veri. Pozitif değer oyuncu lehine özellik, negatif değer oyuncu aleyhine özellik ve 0 dengeli özellik olarak işlenir. Token değerleri +16 ile -16 arasında olmalıdır.")]
    [SerializeField] private int tokenValue;
    public int TokenValue => tokenValue;

    [Tooltip("Bu efektle birlikte aynı perkte oluşmaya yönelen efektlerin listesi.")]
    [SerializeField] private List<PerkEffect> effectWhiteList;
    public List<PerkEffect> WhiteList => effectWhiteList;

    [Tooltip("Bu efektle birlikte aynı perkte oluşamayacak efektlerin listesi.")]
    [SerializeField] private List<PerkEffect> effectBlackList;
    public List<PerkEffect> BlackList => effectBlackList;

    [Tooltip("Efekt çarpanının alabileceği minimum değer. -1 değeri için sınır yok kabul edilir.")]
    [SerializeField] private int minMultiplierLimit;
    public int MinLimit => minMultiplierLimit;

    [Tooltip("Efekt çarpanının alabileceği maximum değer. -1 değeri için sınır yok kabul edilir.")]
    [SerializeField] private int maxMultiplierLimit;
    public int MaxLimit => maxMultiplierLimit;

    public virtual bool RequiresChoice => false;

    public virtual void OnAcquired(GameplayManager context, float multiplier) {}
    public virtual void OnRemoved(GameplayManager context, float multiplier) {}
    public virtual int ModifyScore(int score, GameplayManager context, bool? isCorrect, float multiplier) => score;
    public virtual float ModifyDuration(float duration, GameplayManager context, float multiplier) => duration;
    public virtual string GetDescription(float multiplier) => perkDescription;
    public virtual void OnUsed(GameplayManager context, float multiplier) {}
    public virtual void OnUsedOnChoice(GameplayManager context, int choiceIndex, float multiplier) {}
}
