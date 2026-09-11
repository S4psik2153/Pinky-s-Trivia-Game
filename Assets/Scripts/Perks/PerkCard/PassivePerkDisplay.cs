using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PassivePerkDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image passivePerk_MiniIcon;

    public event Action<PassivePerkDisplay> OnHoverEnter;

    public event Action OnHoverExit;

    private Perk perk;
    public Perk Perk => perk;

    public void SetIcon(Perk perk)
    {
        this.perk = perk;

        passivePerk_MiniIcon.sprite = perk.MiniIcon;
    }

    void OnDisable()
    {
        perk = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit?.Invoke();
    }
}
