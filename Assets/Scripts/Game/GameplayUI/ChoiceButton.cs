using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dinamik cevap seçeneği oluşturabilmek için
/// cevap seçeneklerinin prefab objesinin özelliklerni
/// kontrol etmeyi sağlayan sınıf.
/// </summary>
public class ChoiceButton : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private Button choice_Button;
    [SerializeField] private TextMeshProUGUI choice_ButtonText;

    [Header("Answer Select Animatons")]
    [SerializeField] private float punchStrength = 0.2f;
    [SerializeField] private float punchDuration = 0.4f;
    [SerializeField] private float shakeStrength = 30f;
    [SerializeField] private float shakeDuration = 0.4f;

    [Header("Button Animations")]
    [SerializeField] private RectTransform  buttonContentTransform;
    [SerializeField] private Vector2        entranceOffset      = new(-200f, 0f);
    [SerializeField] private float          entranceDuration    = 0.25f;
    [SerializeField] private Ease           entranceEase        = Ease.OutBack;

    private Vector2 originalLocalAnchoredPosition;
    private Vector3 originalLocalScale;

    /// <summary>
    /// Bir cevap seçeneği seçildiğinde uyarılan metot aksiyonu.
    /// <see cref="ChoiceButton"/> parametresi yapılan seçimde seçilen butonun referansını iletir.
    /// </summary>
    public event Action<ChoiceButton> OnClicked;

    [SerializeField] private CanvasGroup choice_CanvasGroup;
    [SerializeField] private float eliminatedAlpha = 0.25f;
    [SerializeField] private float eliminatedScale = 0.9f;
    [SerializeField] private float eliminationDuration = 0.3f;

    [SerializeField] private Image frost_Image;          // Raycast Target KAPALI
    [SerializeField] private float frostBreakDuration = 0.15f;
    [SerializeField] private float frostBreakPunch = 0.15f;
    private int frostHitsRemaining;
    private int frostHitsTotal;

    void Awake()
    {
        // Butonun basılma aksiyonu dinleyiciye kaydedilir.
        choice_Button.onClick.AddListener(OnClickedButton);

        originalLocalAnchoredPosition = buttonContentTransform.anchoredPosition;
        originalLocalScale = buttonContentTransform.localScale;
    }

    void OnDestroy()
    {
        // Butonun basılma aksiyonu, obje yok edildiğinde dinleyiciden kaldırılır.
        choice_Button.onClick.RemoveListener(OnClickedButton);
    }

  void OnDisable()
    {       
        buttonContentTransform.DOKill();
        choice_CanvasGroup.DOKill();
        frost_Image.DOKill();

        frostHitsRemaining = 0;
        frost_Image.gameObject.SetActive(false);

        choice_CanvasGroup.alpha = 1f;

        buttonContentTransform.anchoredPosition = originalLocalAnchoredPosition;
        buttonContentTransform.localScale = originalLocalScale;

        Color c = frost_Image.color;
        c.a = 1f;
        frost_Image.color = c;
    }

    /// <summary>
    /// Butonun seçenek metnini tutan
    /// <see cref="choice_ButtonText"/> elementinin
    /// metin özelliğini değiştirmeyi sağlayan metot.
    /// </summary>
    /// <param name="text">
    /// Buton metnine yazılacak cevap seçeneği metni verisi.
    /// </param>
    public void SetChoiceText(string text)
    {
        choice_ButtonText.text = text;
    }

    /// <summary>
    /// <see cref="choice_Button"/> elementinin
    /// renk özelliğini değiştirmeyi sağlayan metot.
    /// </summary>
    /// <param name="color">
    /// Butonun güncellenecek renk verisi.
    /// </param>
    public void SetButtonColor(Color color)
    {
        choice_Button.image.color = color;
    }

    /// <summary>
    /// <see cref="choice_Button"/> elementinin
    /// etkileşime geçilebilme özelliğini değiştirmeyi sağlayan metot.
    /// </summary>
    /// <param name="interactable">
    /// <see langword="true"/> buton etkileşime geçilebilirliğini aç,
    /// <see langword="false"/> buton etkileşime geçilebilirliğini kapat.
    /// </param>
    public void SetButtonInteractable(bool interactable)
    {
        choice_Button.interactable = interactable;
    }

    /// <summary>
    /// <see cref="choice_Button"/> butonuna basıldığında
    /// <see cref="OnClicked"/> aksiyonunu uyaran metot.
    /// </summary>
    private void OnClickedButton()
    {
        if (frostHitsRemaining > 0)
        {
            BreakFrost();
            return;
        }

        OnClicked?.Invoke(this);
    }

    /// <summary>
    /// Soru seçenek butonlarının yüklenme
    /// animasyonunu uygulayan metot.
    /// </summary>
    /// <param name="delay">
    /// Butonların yüklenmesi sırasında her buton
    /// animasyonu arasında gecikme miktarını belirten veri.
    /// </param>
    public void PlayEntrance(float delay)
    {
        buttonContentTransform.DOKill();

        buttonContentTransform.anchoredPosition = originalLocalAnchoredPosition + entranceOffset;

        buttonContentTransform.DOAnchorPos(originalLocalAnchoredPosition, entranceDuration).SetDelay(delay).SetEase(entranceEase);
    }

    /// <summary>
    /// Doğru cevap seçeneğinin animasyonunu uygulayan metot.
    /// </summary>
    public void PlayCorrectFeedback()
    {
        buttonContentTransform.DOKill();

        buttonContentTransform.anchoredPosition = originalLocalAnchoredPosition;
        buttonContentTransform.localScale = originalLocalScale;

        buttonContentTransform.DOPunchScale(Vector3.one * punchStrength, punchDuration);
    }

    /// <summary>
    /// Doğru olmayan bir cevap seçeneği seçildiğinde
    /// oynayan animasyonu uygulayan metot.
    /// </summary>
    public void PlayWrongFeedback()
    {
        buttonContentTransform.DOKill();

        buttonContentTransform.anchoredPosition = originalLocalAnchoredPosition;
        buttonContentTransform.localScale = originalLocalScale;

        buttonContentTransform.DOShakePosition(shakeDuration, shakeStrength * Vector3.right);
    }

    public void PlayElimination()
    {
        buttonContentTransform.localScale = originalLocalScale;

        SetButtonInteractable(false);

        buttonContentTransform.DOKill();
        choice_CanvasGroup.DOKill();

        buttonContentTransform.anchoredPosition = originalLocalAnchoredPosition;

        buttonContentTransform.DOScale(originalLocalScale * eliminatedScale, eliminationDuration);
        choice_CanvasGroup.DOFade(eliminatedAlpha, eliminationDuration);
    }

    public void SetFrost(int hits)
    {
        frost_Image.DOKill();

        frostHitsTotal = hits;
        frostHitsRemaining = hits;

        frost_Image.gameObject.SetActive(hits > 0);

        Color c = frost_Image.color;
        c.a = 1f;
        frost_Image.color = c;
    }

    private void BreakFrost()
    {
        frostHitsRemaining--;

        frost_Image.DOKill();
        buttonContentTransform.DOKill();
        buttonContentTransform.localScale = originalLocalScale;
        buttonContentTransform.anchoredPosition = originalLocalAnchoredPosition;

        buttonContentTransform.DOPunchScale(Vector3.one * frostBreakPunch, frostBreakDuration);

        if (frostHitsRemaining <= 0)
        {
            frost_Image.DOFade(0f, frostBreakDuration)
                    .OnComplete(() => frost_Image.gameObject.SetActive(false));
        }
        else
        {
            float targetAlpha = (float)frostHitsRemaining / frostHitsTotal;
            frost_Image.DOFade(targetAlpha, frostBreakDuration);
        }
    }
}
