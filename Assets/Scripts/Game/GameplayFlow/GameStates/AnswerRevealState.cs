/// <summary>
/// Soru aşamasında yapılan seçimin ardından
/// verilen cevabın kontrol edildiği oyun durumu sınıfıdır.
/// </summary>
public class AnswerRevealState : GameState
{
    /// <summary>
    /// Oyuncunun seçenek butonları arasından yaptığı cevap seçiminin index değerini belirten veri.
    /// <c>0</c> - <c>x</c> seçilen butonun index değerini,
    /// <c>-1</c> sorunun zaman aşımına uğradığını belirtir.
    /// </summary>
    private readonly int selectedIndex;

    /// <summary>
    /// Sorunun gerçek doğru cevabının seçenek butonu index değerini belirten değer.
    /// <c>-1</c> sorunun belirlenmiş cevap index değerinin geçersiz olduğunu belirtir.
    /// </summary>
    private readonly int correctAnswerIndex;

    /// <summary>
    /// Cevap açığa çıkma oyun durumu sınıfının constructor metodu.
    /// </summary>
    /// <param name="context">
    /// <see cref="GameplayManager"/> referansı içeren oyun bağlamını
    /// parent sınıfa iletir.
    /// </param>
    /// <param name="selectedIndex">
    /// Oyuncunun seçtiği cevabın seçenek butonunun index'i.
    /// </param>
    /// <param name="correctAnswerIndex">
    /// <see cref="GameplayManager.questions"/> verisinden elde edilen doğru cevap index'i.
    /// </param>
    public AnswerRevealState(GameplayManager context, int selectedIndex, int correctAnswerIndex) : base(context)
    {
        this.selectedIndex = selectedIndex;
        this.correctAnswerIndex = correctAnswerIndex;
    }

    public override void EnterState()
    {
        // Arayüz güncellenir.
        context.UI.ShowAnswerResult(selectedIndex, correctAnswerIndex);
        context.UI.UpdateScore(context.Score, context.ScoreMultiplier);
        context.UI.SetNextQuestionButtonActive(true);

        if (selectedIndex == correctAnswerIndex) context.UI.PlayMascotCorrect();
        else                                     context.UI.PlayMascotWrong();

        context.UI.NextQuestionRequested += NextQuestionRequested;
    }

    public override void ExitState()
    {
        
        context.UI.SetNextQuestionButtonActive(false);

        // Cevap açığa çıkma durumu çıkışında aksiyon aboneliği iptal edilir.
        // Bırakılmazsa sonraki soruda aksiyon yeniden abone olur ve
        // bir tıklama iki kez sayılır.
        context.UI.NextQuestionRequested -= NextQuestionRequested;
    }

    /// <summary>
    /// Sonraki soru butonuna basıldığında uyarılan metottur.
    /// Bağlama göre <see cref="PerkSelectionState"/> oyun durumuna veya
    /// <see cref="GameplayManager.MoveToNextQuestion"/> metodunu çağırarak
    /// sonraki soruya geçmeyi sağlar.
    /// </summary>
    private void NextQuestionRequested()
    {
        if (context.HasMoreQuestions)
        {
            context.ChangeGameState(new PerkSelectionState(context));
        }
        else
        {
            context.MoveToNextQuestion();
        }
    }
}
