using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oyunun soruları yüklemeye başladığı yükleme durumudur.
/// <see cref="GameplayManager"/> tarafından çağırılan ilk oyun durumu sınıfıdır.
/// </summary>
public class LoadingState : GameState
{
    /// <summary>
    /// Yüklenme oyun durumu sınıfının constructor metodu.
    /// </summary>
    /// <param name="context">
    /// <see cref="GameplayManager"/> referansı içeren oyun bağlamını
    /// parent sınıfa iletir.
    /// </param>
    public LoadingState(GameplayManager context) : base(context) { }

    public override void EnterState()
    {
        // Soruların indirilmesi başlatılır.
        context.LoadQuestions(OnQuestionsLoaded);
    }

    /// <summary>
    /// Soruların indirilme işlemi tamamlandığında uyarılan metottur.
    /// </summary>
    /// <remarks>
    /// Sorular başarıyla indirilirse bir sonraki oyun durumuna geçilir.
    /// Gelen soru listesi boşsa hata ekranına yönlendirilir.
    /// </remarks>
    /// <param name="list">
    /// İndirme sonucu gelen soru listesi verisi.
    /// </param>
    private void OnQuestionsLoaded(List<Question> list)
    {
        if (list == null)
        {
            string errorMessage = "Question List is empty.";

            Debug.LogError(errorMessage);

            if (LoadingScreenManager.Instance != null) LoadingScreenManager.Instance.ShowErrorScreen(errorMessage, context.ReturnToMainMenu);

            // Hata ekranı ana menüye dönüldüğünde otomatik olarak kapatılır.
            return;
        }

        context.SetQuestions(list);

        if (LoadingScreenManager.Instance != null)  LoadingScreenManager.Instance.HideLoadingScreen(OnLoadingScreenHide);
        else                                        context.ChangeGameState(new IntroductionState(context));
    }

    /// <summary>
    /// Yükleme ekranı hala açıksa
    /// kapanma animasyonunun ardından
    /// ilk soruyu başlatan metot.
    /// </summary>
    private void OnLoadingScreenHide()
    {
        context.ChangeGameState(new IntroductionState(context));
    }
}
