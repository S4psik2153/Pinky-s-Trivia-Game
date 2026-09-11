using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Skor sayacında bir skor değişikliği yaşandığında
/// görsel bildirim sağlayan süzülen skor objesinin
/// animasyonlarını uygulayan sınıf.
/// </summary>
public class AnimatedFloatingScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI    text;
    [SerializeField] private RectTransform      rectTransform;
    [SerializeField] private Color              positiveColor   = Color.green;
    [SerializeField] private Color              negativeColor   = Color.red;

    [SerializeField] private CanvasGroup        canvasGroup;

    [SerializeField] private float              duration        = 0.3f;
    [SerializeField] private float              waitDuration    = 0.5f;

    [SerializeField] private Ease               ease;

    /// <summary>
    /// Süzülen skor objesinin animasyonlarını uygulayan metot.
    /// </summary>
    /// <param name="amount">
    /// Skora eklenen değer verisi.
    /// </param>
    /// <param name="startPosition">
    /// Oluşturulan süzülen skor objesinin başlangıç pozisyonu verisi.
    /// </param>
    /// <param name="targetPosition">
    /// Oluşturulan süzülen skor objesinin hareket edeceği konum verisi.
    /// </param>
    /// <param name="onArrived">
    /// Animasyon tamamlandığında uyarılacak aksiyon referansı.
    /// </param>
    public void Play(int amount, Vector2 startPosition, Vector2 targetPosition, Action onArrived)
    {
        text.text = amount > 0 ? $"+{amount}" : amount.ToString();

        text.color = amount > 0 ? positiveColor : negativeColor;

        rectTransform.anchoredPosition = startPosition;

        canvasGroup.alpha = 1f;

        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(waitDuration);

        sequence.Append(rectTransform.DOAnchorPos(targetPosition, duration).SetEase(ease));

        sequence.Insert(waitDuration + duration * 0.5f, canvasGroup.DOFade(0f, duration * 0.5f));

        sequence.OnComplete(() => onArrived?.Invoke());
    }

    private void OnDisable()
    {
        rectTransform.DOKill();

        canvasGroup.DOKill();

        canvasGroup.alpha = 1f;
    }
}
