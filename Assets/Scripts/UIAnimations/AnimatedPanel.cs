using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Sahnelerdeki panel objelerini
/// animasyonlandırmak için oluşturulmuş sınıf.
/// </summary>
/// <remarks>
/// Panel için kayma, boyutlanma ve solma animasyonları
/// panelin açılışı ve kapanışı için desteklenmektedir.
/// </remarks>
public class AnimatedPanel : MonoBehaviour
{
    /// <summary>
    /// Panel animasyon türlerini belirten enum.
    /// </summary>
    /// <remarks>
    /// <see cref="Slide"/> kaydırma animasyonlarını,
    /// <see cref="Scale"/> boyutlandırma animasyonlarını,
    /// <see cref="Fade"/> solma animasyonlarını
    /// temsil eder
    /// </remarks>
    public enum PanelAnimationType { Slide, Scale, Fade }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelTransform;

    [SerializeField] private PanelAnimationType inAnimation;
    [SerializeField] private PanelAnimationType outAnimation;

    [SerializeField] private float hiddenScale = 0.5f;
    [SerializeField] private Vector2 hiddenInOffset;
    [SerializeField] private Vector2 hiddenOutOffset;

    [SerializeField] private float duration = 0.3f;

    [SerializeField] private Ease inEase;
    [SerializeField] private Ease outEase;
        
    private Vector2 visiblePosition;
    private Vector3 visibleScale;

    private void Awake()
    {
        visiblePosition = panelTransform.anchoredPosition;
        
        visibleScale = panelTransform.localScale;

        ResetInitialTransform();
    }

    /// <summary>
    /// Paneli açan ve
    /// <see cref="inAnimation"/> animasyonunu başlatan metot.
    /// </summary>
    /// <returns>
    /// Animasyon sonucuna göre <see cref="Tween"/> verisi döndürülür.
    /// </returns>
    /// <param name="onComplete">
    /// Animasyon tamamlandığında çağırılacak <see cref="Action"/> referansı.
    /// Her animasyon işleminde gerekmediğinden varsayılan değer olarak
    /// <c>null</c> tanımlandı.
    /// </param>
    public Tween ShowPanel(Action onComplete = null)
    {
        panelTransform.DOKill();
        canvasGroup.DOKill();

        canvasGroup.interactable = true;

        canvasGroup.blocksRaycasts = true;

        void callback() => onComplete?.Invoke();   

        switch (inAnimation)
        {
            case PanelAnimationType.Slide:
            {
                return SlideIn().OnComplete(callback);
            }
            case PanelAnimationType.Scale:
            {
                return ScaleIn().OnComplete(callback);
            }
            case PanelAnimationType.Fade:
            {
                return FadeIn().OnComplete(callback);
            }
        }

        Debug.LogError("Invalid animation type: " + inAnimation);

        onComplete?.Invoke();

        return null;
    }

    /// <summary>
    /// Paneli kapatan ve
    /// <see cref="outAnimation"/> animasyonunu başlatan metot.
    /// </summary>
    /// <remarks>
    /// Animasyon sonlandıktan sonra
    /// <see cref="ResetInitialTransform"/> metodu uyarılır.
    /// </remarks>
    /// <returns>
    /// Animasyon sonucuna göre <see cref="Tween"/> verisi döndürülür.
    /// </returns>
    /// <param name="onComplete">
    /// Animasyon tamamlandığında çağırılacak <see cref="Action"/> referansı.
    /// Her animasyon işleminde gerekmediğinden varsayılan değer olarak
    /// <c>null</c> tanımlandı.
    /// </param>
    public Tween HidePanel(Action onComplete = null)
    {
        panelTransform.DOKill();
        canvasGroup.DOKill();

        canvasGroup.interactable = false;

        canvasGroup.blocksRaycasts = false;

        void callback()
        {
            ResetInitialTransform();
            
            onComplete?.Invoke();   
        }

        switch (outAnimation)
        {
            case PanelAnimationType.Slide:
            {
                return SlideOut().OnComplete(callback);
            }
            case PanelAnimationType.Scale:
            {
                return ScaleOut().OnComplete(callback);
            }
            case PanelAnimationType.Fade:
            {
                return FadeOut().OnComplete(callback);
            }
        }

        Debug.LogError("Invalid animation type: " + outAnimation);

        onComplete?.Invoke();

        return null;
    }

    /// <summary>
    /// Panel açılışlarında
    /// kayma animasyonunu uygulayan metot.
    /// </summary>
    /// <remarks>
    /// Animasyon başlatılmadan önce panel
    /// animasyona hazır hale getirilir.
    /// </remarks>
    /// <returns>
    /// Animasyon sonucuna göre <see cref="Tween"/> verisi döndürülür.
    /// </returns>
    private Tween SlideIn()
    {
        canvasGroup.alpha = 1;

        panelTransform.localScale = visibleScale;
        
        return panelTransform.DOAnchorPos(visiblePosition, duration).SetEase(inEase);
    }

    /// <summary>
    /// Panel kapanışlarında
    /// kayma animasyonunu uygulayan metot.
    /// </summary>
    /// <returns>
    /// Animasyon sonucuna göre <see cref="Tween"/> verisi döndürülür.
    /// </returns>
    private Tween SlideOut()
    {
        return panelTransform.DOAnchorPos(visiblePosition + hiddenOutOffset, duration).SetEase(outEase);
    }

    /// <summary>
    /// Panel açılışlarında
    /// boyutlandırma animasyonunu uygulayan metot.
    /// </summary>
    /// <remarks>
    /// Animasyon başlatılmadan önce panel
    /// animasyona hazır hale getirilir.
    /// </remarks>
    /// <returns>
    /// Animasyon sonucuna göre <see cref="Tween"/> verisi döndürülür.
    /// </returns>
    private Tween ScaleIn()
    {
        canvasGroup.alpha = 1;

        panelTransform.anchoredPosition = visiblePosition;

        return panelTransform.DOScale(visibleScale, duration).SetEase(inEase);
    }

    /// <summary>
    /// Panel kapanışlarında
    /// boyutlandırma animasyonunu uygulayan metot.
    /// </summary>
    /// <returns>
    /// Animasyon sonucuna göre <see cref="Tween"/> verisi döndürülür.
    /// </returns>
    private Tween ScaleOut()
    {
        return panelTransform.DOScale(visibleScale * hiddenScale, duration).SetEase(outEase);
    }

    /// <summary>
    /// Panel açılışlarında
    /// solma animasyonunu uygulayan metot.
    /// </summary>
    /// <remarks>
    /// Animasyon başlatılmadan önce panel
    /// animasyona hazır hale getirilir.
    /// </remarks>
    /// <returns>
    /// Animasyon sonucuna göre <see cref="Tween"/> verisi döndürülür.
    /// </returns>
    private Tween FadeIn()
    {
        panelTransform.anchoredPosition = visiblePosition;

        panelTransform.localScale = visibleScale;
        
        return canvasGroup.DOFade(1f, duration);
    }

    /// <summary>
    /// Panel kapanışlarında
    /// solma animasyonunu uygulayan metot.
    /// </summary>
    /// <returns>
    /// Animasyon sonucuna göre <see cref="Tween"/> verisi döndürülür.
    /// </returns>
    private Tween FadeOut()
    {
        return canvasGroup.DOFade(0f, duration);
    }

    /// <summary>
    /// Paneli <see cref="inAnimation"/> değerine göre
    /// açılış animasyonlarına hazırlayan metot.
    /// </summary>
    private void ResetInitialTransform()
    {
        canvasGroup.alpha = 0f;

        canvasGroup.interactable = false;

        canvasGroup.blocksRaycasts = false;

        switch (inAnimation)
        {
            case PanelAnimationType.Slide:
            {
                panelTransform.anchoredPosition = visiblePosition + hiddenInOffset;

                break;
            }
            case PanelAnimationType.Scale:
            {
                panelTransform.localScale = visibleScale * hiddenScale;
                
                break;
            }
        }
    }
}
