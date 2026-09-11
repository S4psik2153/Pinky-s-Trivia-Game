/// <summary>
/// Sorular yüklendikten sonra maskotun oyuncuyu karşıladığı
/// ve oyunun başlatılmasını beklediği oyun durumu sınıfıdır.
/// </summary>
public class IntroductionState : GameState
{
    public IntroductionState(GameplayManager context) : base(context) { }

    public override void EnterState()
    {
        context.UI.MascotBubbleClosed += OnBubbleClosed;
        context.UI.ScreenTapped += OnScreenTapped;

        // Tanıtım boyunca ekranın her yeri ilerletme için dokunulabilir olur.
        context.UI.SetTapCatcherActive(true);

        context.UI.ShowCountdownPanel();

        context.UI.ShowMascotIntroduction();
    }

    public override void ExitState()
    {
        context.UI.MascotBubbleClosed -= OnBubbleClosed;
        context.UI.ScreenTapped -= OnScreenTapped;

        context.UI.SetTapCatcherActive(false);

        context.UI.HideMascotHint();
    }

    /// <summary>
    /// Ekrana dokunulduğunda baloncuğu ilerleten metottur.
    /// </summary>
    private void OnScreenTapped()
    {
        context.UI.AdvanceMascotBubble();
    }

    /// <summary>
    /// Tüm tanıtım metinleri okunup baloncuk kapatıldığında
    /// geri sayımı başlatan metottur.
    /// </summary>
    private void OnBubbleClosed()
    {
        context.UI.ScreenTapped -= OnScreenTapped;

        context.UI.SetTapCatcherActive(false);

        context.UI.PlayCountdown(OnCountdownComplete);
    }

    private void OnCountdownComplete()
    {
        context.ChangeGameState(new QuestionState(context));
    }
}