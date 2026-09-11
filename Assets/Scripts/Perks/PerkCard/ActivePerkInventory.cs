using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActivePerkInventory : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private float hiddenY = -200f;
    [SerializeField] private float shownY = 0f;
    [SerializeField] private float slideDuration = 0.25f;
    [SerializeField] private Ease slideEase = Ease.OutQuad;

    private void OnEnable()
    {
        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, hiddenY);
    }

    private void OnDisable()
    {
        panelRect.DOKill();

        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, hiddenY);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        panelRect.DOKill();

        panelRect.DOAnchorPosY(shownY, slideDuration).SetEase(slideEase);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        panelRect.DOKill();
        
        panelRect.DOAnchorPosY(hiddenY, slideDuration).SetEase(slideEase);
    }
}
