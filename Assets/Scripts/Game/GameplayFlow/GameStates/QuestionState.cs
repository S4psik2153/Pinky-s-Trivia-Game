using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Önceki yükleme durumunda <see cref="GameplayManager.questions"/> verisine kaydedilen soruların
/// oyuncuya yansıtılarak oyuncunun bir cevap seçeneği seçmesinin beklendiği oyun durumu sınıfıdır.
/// </summary>
public class QuestionState : GameState
{
    /// <summary>
    /// Soru oyun durumu sınıfının constructor metodu.
    /// </summary>
    /// <param name="context">
    /// <see cref="GameplayManager"/> referansı içeren oyun bağlamını
    /// parent sınıfa iletir.
    /// </param>
    public QuestionState(GameplayManager context) : base(context) { }

    /// <summary>
    /// Zamanlayıcının kalan süresi kritik bölgeye indiğinde
    /// kritik süre animasyonunu başlatan veri.
    /// </summary>
    private bool pulseStarted;

    /// <summary>
    /// Zamanlayıcı geri sayımında
    /// geriye kalan zaman bilgisini tutan veri.
    /// </summary>
    private float remainingTime = 0f;

    /// <summary>
    /// Karşılaştırma yapılabilmesi için zamanlayıcıya yazılmış en son değeri tutan veri.
    /// </summary>
    /// <remarks>
    /// Amacı sürekli aynı değeri yazdırma işleminden kaçınmak.
    /// </remarks>
    private int timerCounter = 0;

    /// <summary>
    /// Doğru cevabı barındıran butonun index değerini tutan veri.
    /// </summary>
    private int correctIndex;

    private float questionDuration;

    /// <summary>
    /// Buton indexlerini karıştırmak ve saklamak için kullanılan dizi yapısı.
    /// </summary>
    private int[] order;

    private Perk pendingChoicePerk;

    public override void EnterState()
    {
        // Zamanlayıcı hazırlanır.
        questionDuration = context.Perks.ModifyDuration(context.Config.questionDuration);
        remainingTime = questionDuration;
        timerCounter = Mathf.CeilToInt(remainingTime);

        context.UI.ResetMascot();

        // Sorunun geçerliliği test edilir.
        if (context.CurrentQuestion.error)
        {
            Debug.LogError("Invalid Question");
            
            if (context.HasMoreQuestions)
            {
                // Soru hatalıysa bir sonraki soruya geçilir.
                context.MoveToNextQuestion();
            }
            else
            {
                // Son soru da hatalıysa oyun bitirilir.
                context.ChangeGameState(new FinishGameState(context));
            }
            
            return;
        }

        context.ChoiceEliminationRequested += OnChoiceEliminationRequested;

        int baseCount = context.CurrentQuestion.choices.Length;
        int targetCount = baseCount + context.ExtraChoiceCount - context.RemovedChoiceCount;

        targetCount = Mathf.Clamp(targetCount, 2, baseCount + context.ExtraChoiceCount);

        List<string> choiceTexts = new()
        {
            context.CurrentQuestion.choices[0]  // doğru cevap
        };

        // yanlış cevaplardan targetCount-1 tanesini al
        int wrongNeeded = targetCount - 1;
        int wrongAvailable = baseCount - 1;

        for (int i = 1; i <= Mathf.Min(wrongNeeded, wrongAvailable); i++)
        {
            choiceTexts.Add(context.CurrentQuestion.choices[i]);
        }

        // yetmezse başka sorudan çal
        while (choiceTexts.Count < targetCount)
        {
            string stolen = context.GetRandomWrongChoiceFromPool();
            if (string.IsNullOrEmpty(stolen)) break;
            choiceTexts.Add(stolen);
        }

        // Seçenekleri karıştırmak için kullanılacak index dizisi tanımlanıyor.
        order = new int[choiceTexts.Count];
        
        // Index değerleri sıralama dizisine yerleştiriliyor
        for (int i = 0; i < order.Length; i++)
        {
            order[i] = i;
        }
        
        // Sıralama dizisi seçenek butonlarının indexleri karıştırılıyor.
        for (int i = order.Length - 1; i > 0; i--)
        {
            int rng = Random.Range(0, i + 1);
            (order[i], order[rng]) = (order[rng], order[i]);
        }

        // Soru seçeneklerinin bulunduğu diziyi karıştırırsak
        // 0. index'in doğru cevap olduğunu sonradan kontrol edemeyiz.
        // Bu yüzden karıştırılan metinleri içeren yeni bir dizi oluşturarak
        // metinleri butonlara o dizi ile yazıyoruz.
        string[] shuffledChoices = new string[order.Length];

        for (int i = 0; i < shuffledChoices.Length; i++)
        {
            // Seçenek metinleri karıştırılan index değerlerine göre
            // seçenek metni dizisine yerleştiriliyor.
            shuffledChoices[i] = choiceTexts[order[i]];
        }
        
        // Doğru cevabın buton index'i karşılaştırmada kullanılmak için kaydediliyor.
        // Doğru cevap index'i her zaman 0. index olduğundan doğru cevap metnini taşıyan
        // butonun index'i sıralama dizisinde 0 değerini bulunduran index'te yer alır.
        // Bu yüzden sıralama dizisindeki 0 değerinin yer aldığı index bizim
        // doğru cevap metnini barındıran butonumuz olur.
        correctIndex = System.Array.IndexOf(order, 0);

        // Kritik süre animasyonu bayrağı sıfırlanır.
        pulseStarted = false;

        context.UI.ShowActiveCardInventory(context.Perks.ActivePerks, context.PerkConfig);
        context.UI.PerkUsed += OnPerkUsed;

        context.Perks.TickQuestionCooldowns();
        context.UI.UpdateActivePerkCards();

        context.UI.UpdatePassivePerkDisplay(context.Perks.PassivePerks, context.PerkConfig);

        // Arayüz ayarlanıyor.
        context.UI.ShowQuestion(context.CurrentQuestion, context.CurrentQuestionNumber, shuffledChoices);

        int frozenChoiceCount = context.FrozenChoices().Count;

        if (frozenChoiceCount > 0)
        {
            List<int> frostChoiceList = new(order);

            for (int i = 0; i < frozenChoiceCount; i++)
            {
                int frozen = Random.Range(0, frostChoiceList.Count);

                context.UI.SetChoiceFrost(frozen, context.FrozenChoices()[i]);

                frostChoiceList.Remove(frozen);
            }
        }

        context.UI.SetChoiceButtonsInteractable(true);

        context.UI.OnAnswerSelected += OnAnswerSelected;

        context.UI.MascotHintRequested += OnMascotHintRequested;

        context.UI.UpdateTimer(timerCounter, context.TimerSpeed);

        context.UI.UpdateScore(context.Score, context.ScoreMultiplier);
    }

    public override void ExitState()
    {
        context.UI.StopTimerPulse();

        // Kalan süre bağlama bildiriliyor.
        context.IncreaseElapsedTime(questionDuration - remainingTime);

        context.UI.SetChoiceTargetingMode(false);

        context.UI.HideActiveCardInventory();
        context.UI.PerkUsed -= OnPerkUsed;

        // Durum çıkışında aksiyon aboneliği iptal edilir.
        // Bırakılmazsa sonraki soruda aksiyon yeniden abone olur ve
        // bir tıklama iki kez sayılır.
        context.UI.OnAnswerSelected -= OnAnswerSelected;

        context.ChoiceEliminationRequested -= OnChoiceEliminationRequested;

        context.UI.MascotHintRequested -= OnMascotHintRequested;
        context.UI.HideMascotHint();

        context.MarkedChoiceIndex = -1;
        context.MarkedChoiceMultiplier = 1f;
    }

    public override void Tick()
    {
        context.Perks.TickDurationCooldowns(Time.deltaTime);
        context.UI.UpdateActivePerkCards();

        // Her framede süre saniye cinsinden azaltılır.
        remainingTime -= Time.deltaTime * context.TimerSpeed;

        // Zamanlayıcıda gösterilmek için tam sayıya yuvarlanır.
        int currentTime = Mathf.CeilToInt(remainingTime);

        context.UI.UpdateTimerFill(Mathf.Max(remainingTime, 0f));

        // Kalan süre karşılaştırılır. Önce sürenin kritik bölgede olup olmadığına bakılır.
        // Sonra bir önceki framelerde yazılan tam sayı değeri şu anki tam sayı değeri ile karşılaştırılır.
        // Eğer karşılaştırma sırası ters olsaydı kritik bölgede sayaçta gösterilen sayı
        // sürenin tam sayı olduğu framede sayaçta da tam sayı olarak görünürdü ve bu titremeye sebep olurdu.
        if (remainingTime < context.Config.recklessTime)
        {
            // Kritik süre bayrağı kontrol edilir.
            if (!pulseStarted)
            {
                // Kritik bölgeye ilk defa girildiğinde
                // kritik bölge animasyonu başlatılır.
                pulseStarted = true;
                context.UI.StartTimerPulse();
            }

            // Kalan süre kritik bölgedeyse sayaç doğrudan ondalıklı değer göstermeye başlar (float aşırı yüklemesi ve "0.0" formatı ile).
            context.UI.UpdateTimer(Mathf.Max(remainingTime, 0), context.TimerSpeed);
        }
        else if (timerCounter != currentTime)
        {
            // Kritik süreden yüksek kalan süreler tam sayıya yuvarlanarak gösterilir (int aşırı yüklemesi ve "0" formatı ile).
            timerCounter = currentTime;
            context.UI.UpdateTimer(Mathf.Max(timerCounter, 0), context.TimerSpeed);
        }

        if (remainingTime <= 0)
        {
            // Zamanlayıcı süresi sona erdiğinde sayaç ve sayaç göstergesi sıfırlanır.
            context.UI.UpdateTimer(0, context.TimerSpeed);
            context.UI.UpdateTimerFill(0f);

            // Zaman doldu animasyonu başlatılır.
            context.UI.PlayTimeoutFeedback();

            // Kalan süre sıfır veya sıfırdan düşük bir değeri görürse soru zaman aşımına uğrar.
            context.AddScore(Mathf.RoundToInt(context.Perks.ModifyScore(context.Config.timeoutScore, null) * context.ScoreMultiplier));

            context.UI.SetChoiceButtonsInteractable(false);

            // Zaman aşımı yaşandığı için seçilen seçenek butonu seçeneği -1 olarak iletildi.
            context.ChangeGameState(new AnswerRevealState(context, -1, correctIndex));
            return;
        }
    }

    /// <summary>
    /// Oyuncu herhangi bir seçenek butonu seçimi yaptığında uyarılan metottur.
    /// </summary>
    /// <param name="selectedIndex">
    /// Oyuncunun yaptığı seçenek butonu seçiminin index değeri.
    /// </param>
    private void OnAnswerSelected(int selectedIndex)
    {
        if (pendingChoicePerk != null)
        {
            context.Perks.UsePerk(pendingChoicePerk, selectedIndex);

            pendingChoicePerk = null;

            context.UI.UpdateActivePerkCards();

            context.UI.SetChoiceTargetingMode(false);
            
            return;
        }

        // Verilen cevap sorunun doğru cevabı ile karşılaştırılır ve ona göre bir skor uygulaması yapılır.
        // Cevap doğru ise doğru cevap puanı,
        // yanlış ise yanlış cevap puanı skora eklenir.
        int score = correctIndex == selectedIndex ? context.Perks.ModifyScore(context.Config.correctScore, true) : context.Perks.ModifyScore(context.Config.wrongScore, false);
        
        if (selectedIndex == context.MarkedChoiceIndex) score = Mathf.RoundToInt(score * context.MarkedChoiceMultiplier);
        
        context.AddScore(Mathf.RoundToInt(score * context.ScoreMultiplier));


        // Başka bir seçim yapılamaması için butonlar kapatılır.
        context.UI.SetChoiceButtonsInteractable(false);

        // Ardından oyun cevap açığa çıkma durumuna geçirilir.
        context.ChangeGameState(new AnswerRevealState(context, selectedIndex, correctIndex));
    }

    private void OnPerkUsed(Perk perk)
    {
        if (perk == null)
        {
            Debug.LogError("Selected active perk is null.");

            return;
        }

        if (!perk.IsPerkActive)
        {
            context.UI.PlayPerkNotReadyFeedback(perk);

            return;
        }

        if (perk.TargetType == Perk.PerkTargetType.Instant)
        {
            context.Perks.UsePerk(perk);
        }
        else if (perk.TargetType == Perk.PerkTargetType.Choice)
        {
            if (pendingChoicePerk == perk)
            {
                pendingChoicePerk = null;

                context.UI.SetChoiceTargetingMode(false);

            }
            else
            {
                pendingChoicePerk = perk;

                context.UI.SetChoiceTargetingMode(true, pendingChoicePerk);
            }
        }
        else
        {
            Debug.LogError("Unknown target type.");

            return;
        }

        context.UI.ShowActiveCardInventory(context.Perks.ActivePerks, context.PerkConfig);

        if (pendingChoicePerk != null) context.UI.SetChoiceTargetingMode(true, pendingChoicePerk);
    }

    private void OnChoiceEliminationRequested()
    {
        List<int> candidates = new();

        for (int i = 0; i < context.UI.ChoiceCount; i++)
        {
            if (i == correctIndex) continue;
            candidates.Add(i);
        }

        if (candidates.Count == 0) return;

        int chosen = candidates[Random.Range(0, candidates.Count)];

        context.UI.RemoveChoiceButton(chosen);

        if (chosen == context.MarkedChoiceIndex)
        {
            context.MarkedChoiceIndex = -1;
        }
        else if (chosen < context.MarkedChoiceIndex)
        {
            context.MarkedChoiceIndex--;
        }

        if (chosen < correctIndex) correctIndex--;
    }

    /// <summary>
    /// Maskota tıklandığında sorunun ipucunu gösteren metot.
    /// </summary>
    private void OnMascotHintRequested()
    {
        context.UI.ShowMascotHint(context.CurrentQuestion.hint);
    }
}
