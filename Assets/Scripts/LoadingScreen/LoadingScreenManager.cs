using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Yükleme ekranını yöneten singleton yapıdaki sınıf.
/// Yükleme ekranının sahneler arasında kaybolmamasını sağlamak için
/// <see cref="UnityEngine.Object.DontDestroyOnLoad"/> metodu kullanıldı.
/// </summary>
/// <remarks>
/// Bu sınıfın bağlı olduğu canvasın Sort Order değeri yüksek olarak ayarlanmalı.
/// Yoksa diğer UI elementleri yükleme ekranı önünde kalabilir.
/// </remarks>
public class LoadingScreenManager : MonoBehaviour
{
    /// <summary>
    /// Diğer sınıfların bu sınıfa erişerek yükleme ekranının durumuna
    /// müdahale edebilmesi için kullanılan Instance alanı.
    /// </summary>
    public static LoadingScreenManager Instance { get; private set; }

    [Header("Screen Canvas Groups")]
    [SerializeField] private AnimatedPanel animatedLoadingScreenPanel;
    [SerializeField] private AnimatedPanel animatedErrorScreenPanel;

    [Header("Loading Screen Elements")]
    [SerializeField] private Slider loading_Slider;

    [Header("Error Screen Elements")]
    [SerializeField] private TextMeshProUGUI error_Text;
    [SerializeField] private Button loadingFailure_Button;

    [Header("Load Progress")]
    [Tooltip("Yükleme ekranının yükleme ilerlemesinin sahne yüklenmesi için ayrılmış yüzdelik kısım. Değişiklik yapılırsa QuestionManager.questionLoadProgressWeight verisinin değeri de güncellenmelidir.")]
    [SerializeField] private float sceneLoadProgressWeight = 0.5f;
    public float SceneLoadProgressWeight => sceneLoadProgressWeight;

    [Tooltip("Yükleme ekranının yükleme ilerlemesinin soru verilerinin indirilmesi için ayrılmış yüzdelik kısım. Değişiklik yapılırsa SceneLoader.sceneLoadProgressWeight verisinin değeri de güncellenmelidir.")]
    [SerializeField] private float questionLoadProgressWeight = 0.5f;
    public float QuestionLoadProgressWeight => questionLoadProgressWeight;

    private Tweener progressTween;
    [SerializeField] private float progressTweenDuration = 0.25f;
    [SerializeField] private Ease progressEase = Ease.Linear;

    [SerializeField] private int stepCount = 8;
    [SerializeField] private float stepInterval = 0.1f;
    [SerializeField] private Transform target;
    private float timer;
    private int currentStep;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < stepInterval) return;

        timer = 0f;
        currentStep = (currentStep + 1) % stepCount;
        target.localRotation = Quaternion.Euler(0f, 0f, -currentStep * (360f / stepCount));
    }

    void OnDestroy()
    {
        progressTween?.Kill();
    }

    /// <summary>
    /// Yükleme ekranının gösterilmesi için
    /// kullanılan public API metot.
    /// </summary>
    /// <remarks>
    /// Ekranın görünürlüğünü,
    /// ekran ile etkileşimini ve
    /// raycastlerin ekrandan geçişini
    /// aktifleştirir.
    /// </remarks>
    /// <param name="onComplete">
    /// Animasyon tamamlandığında çağırılacak
    /// callback Action.
    /// Varsayılan olarak <c>null</c>.
    /// </param>
    public Tween ShowLoadingScreen(Action onComplete = null)
    {
        progressTween?.Kill();
        loading_Slider.value = 0f;

        return animatedLoadingScreenPanel.ShowPanel(onComplete);
    }

    /// <summary>
    /// Yükleme ekranının gizlenmesi için
    /// kullanılan public API metot.
    /// </summary>
    /// <remarks>
    /// Ekranın görünürlüğünü,
    /// ekran ile etkileşimini ve
    /// raycastlerin ekrandan geçişini
    /// deaktifleştirir.
    /// </remarks>
    /// <param name="onComplete">
    /// Animasyon tamamlandığında çağırılacak
    /// callback Action.
    /// Varsayılan olarak <c>null</c>.
    /// </param>
    /// <returns>
    /// Animasyon bilgisi <see cref="Tween"/>
    /// verisi olarak döndürülür.
    /// </returns>
    public Tween HideLoadingScreen(Action onComplete = null)
    {
        return animatedLoadingScreenPanel.HidePanel(onComplete);
    }

    /// <summary>
    /// Yükleme ekranının yüklenme barının
    /// dolum miktarının ayarlanması için
    /// kullanılan public API metot.
    /// </summary>
    /// <remarks>
    /// Sadece <c>0</c> - <c>1</c> arasındaki
    /// değerlerle işlem yapılabilir.
    /// </remarks>
    /// <returns>
    /// Animasyon bilgisi yükleme barının dolmasının beklenebilmesi için
    /// <see cref="Tweener"/> verisi olarak döndürülür.
    /// </returns>
    public Tweener SetLoadingProgress(float progress)
    {
        if (progressTween != null && progressTween.IsActive())
        {
            return progressTween.ChangeEndValue(progress, true);
        }
        else
        {
            return progressTween = loading_Slider.DOValue(progress, progressTweenDuration).SetEase(progressEase);
        }
    }

    /// <summary>
    /// Hata ekranının gösterilmesi için
    /// kullanılan public API metot.
    /// </summary>
    /// <remarks>
    /// Ekranın görünürlüğünü,
    /// ekran ile etkileşimini ve
    /// raycastlerin ekrandan geçişini
    /// aktifleştirir.
    /// </remarks>
    /// <param name="errorMessage">
    /// Gelen hata mesajını ileten veri.
    /// </param>
    /// <param name="onDismiss">
    /// Butona basılınca çağırılacak
    /// callback Action.
    /// </param>
    /// <param name="onComplete">
    /// Animasyon tamamlandığında çağırılacak
    /// callback Action.
    /// Varsayılan olarak <c>null</c>.
    /// </param>
    /// <returns>
    /// Animasyon bilgisi <see cref="Tween"/>
    /// verisi olarak döndürülür.
    /// </returns>
    public Tween ShowErrorScreen(string errorMessage, Action onDismiss, Action onComplete = null)
    {
        // Eğer açık bir yükleme ekranı varsa önce onu kapat.
        HideLoadingScreen();

        error_Text.text = errorMessage;

        // Metot iki defa çağırılırsa eski callbacklerin birikmemesi için:
        loadingFailure_Button.onClick.RemoveAllListeners();
        loadingFailure_Button.onClick.AddListener(() => onDismiss?.Invoke());

        return animatedErrorScreenPanel.ShowPanel(onComplete);
    }

    /// <summary>
    /// Hata ekranının gizlenmesi için
    /// kullanılan public API metot.
    /// </summary>
    /// <remarks>
    /// Ekranın görünürlüğünü,
    /// ekran ile etkileşimini ve
    /// raycastlerin ekrandan geçişini
    /// deaktifleştirir.
    /// </remarks>
    /// <param name="onComplete">
    /// Animasyon tamamlandığında çağırılacak
    /// callback Action.
    /// Varsayılan olarak <c>null</c>.
    /// </param>
    /// <returns>
    /// Animasyon bilgisi <see cref="Tween"/>
    /// verisi olarak döndürülür.
    /// </returns>
    public Tween HideErrorScreen(Action onComplete = null)
    {
        loadingFailure_Button.onClick.RemoveAllListeners();

        return animatedErrorScreenPanel.HidePanel(onComplete);
    }
}
