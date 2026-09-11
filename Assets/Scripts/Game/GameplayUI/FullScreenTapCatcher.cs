using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Ekranın tamamını kaplayarak herhangi bir dokunmayı yakalayan sınıf.
/// </summary>
/// <remarks>
/// Tanıtım gibi oyuncunun serbestçe ilerleyebilmesi gereken aşamalarda
/// kullanılır. Altındaki arayüz elemanlarının etkileşimini engeller.
/// </remarks>
[RequireComponent(typeof(CanvasGroup))]
public class FullScreenTapCatcher : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CanvasGroup canvasGroup;

    /// <summary>
    /// Ekrana dokunulduğunda uyarılan metot aksiyonu.
    /// </summary>
    public event Action Tapped;

    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        SetActive(false);
    }

    /// <summary>
    /// Yakalayıcının dokunma almasını açıp kapatan metot.
    /// </summary>
    /// <param name="active">
    /// <see langword="true"/> dokunmaları yakalamaya başlar,
    /// <see langword="false"/> dokunmaları geçirir.
    /// </param>
    public void SetActive(bool active)
    {
        canvasGroup.blocksRaycasts = active;
    }

    public void OnPointerClick(PointerEventData eventData) => Tapped?.Invoke();
}