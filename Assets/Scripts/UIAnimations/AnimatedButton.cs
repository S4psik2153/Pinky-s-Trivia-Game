using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Menü butonlarının animasyonlarını uygulayan sınıf.
/// Bağlandığı objede o objenin <see cref="Transform"/>
/// elementlerine müdahale eden bir bileşen bulunmamalıdır.
/// </summary>
/// <remarks>
/// Buton etkinliklerinin yakalanabilmesi için bazı
/// Interface eklemeleri yapılmıştır.
/// </remarks>
public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Button button;
    [SerializeField] float  hoverScale  = 1.05f;
    [SerializeField] float  pressScale  = 0.95f;
    [SerializeField] float  duration    = 0.15f;
    [SerializeField] Ease   easeType    = Ease.OutQuad;

    Vector3 buttonLocalScale;

    private void Awake()
    {
        buttonLocalScale = transform.localScale;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = buttonLocalScale;
    }

    /// <summary>
    /// İmleç buton üzerine geldiğinde
    /// <see cref="AnimateTo"/> metodunu çağıran metot.
    /// </summary>
    /// <remarks>
    /// Mobil dokunma etkinlikleri ile çalışmaz.
    /// </remarks>
    /// <param name="eventData">
    /// <see cref="IPointerEnterHandler"/> etkinliğini yakalayan
    /// etkinlik verisi.
    /// </param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        AnimateTo(hoverScale);
    }

    /// <summary>
    /// İmleç buton üzerinden çekildiğinde
    /// <see cref="AnimateTo"/> metodunu çağıran metot.
    /// </summary>
    /// <remarks>
    /// Mobil dokunma etkinlikleri ile çalışmaz.
    /// </remarks>
    /// <param name="eventData">
    /// <see cref="IPointerExitHandler"/> etkinliğini yakalayan
    /// etkinlik verisi.
    /// </param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        AnimateTo(1f);
    }

    /// <summary>
    /// Buton üzerindeki imleç tıklamalarında
    /// <see cref="AnimateTo"/> metodunu çağıran metot.
    /// </summary>
    /// <param name="eventData">
    /// <see cref="IPointerDownHandler"/> etkinliğini yakalayan
    /// etkinlik verisi.
    /// </param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        AnimateTo(pressScale);
    }

    /// <summary>
    /// Buton üzerindeki imleç tıklama bırakmalarında
    /// <see cref="AnimateTo"/> metodunu çağıran metot.
    /// </summary>
    /// <remarks>
    /// İmlecin butonun üzerinde olup olmadığı kontrol edilir ve
    /// imleç buton üzerinden çekildiyse animasyon uygulanmaz.
    /// </remarks>
    /// <param name="eventData">
    /// <see cref="IPointerUpHandler"/> etkinliğini yakalayan
    /// etkinlik verisi.
    /// </param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        bool stillOver = eventData.pointerCurrentRaycast.gameObject == gameObject;
        
        AnimateTo(stillOver ? hoverScale : 1f);
    }

    /// <summary>
    /// Buton boyutlandırmasına animasyon uygulayan metot.
    /// </summary>
    /// <param name="scaleMultiplier"></param>
    private void AnimateTo(float scaleMultiplier)
    {
        transform.DOKill();
        transform.DOScale(buttonLocalScale * scaleMultiplier, duration).SetEase(easeType);
    }
}
