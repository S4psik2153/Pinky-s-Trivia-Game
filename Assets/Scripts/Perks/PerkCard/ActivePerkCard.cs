using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActivePerkCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button perkCard_Button;
    [SerializeField] private TextMeshProUGUI perkCardName_Text;

    private ObjectPool<EffectRow> effectRowPool;
    [SerializeField] private EffectRow effectRowPrefab;
    [SerializeField] private RectTransform effectRowParent;

    [SerializeField] private TextMeshProUGUI useCount_Text;
    [SerializeField] private TextMeshProUGUI remainingCooldown_Text;
    [SerializeField] private TextMeshProUGUI remainingQuestionCooldown_Text;

    [SerializeField] private Image remainingCooldown_Image;
    [SerializeField] private Image remainingQuestionCooldown_Image;

    [SerializeField] private Outline perkCard_Outline;
    [SerializeField] private Image perkCard_Background;
    [SerializeField] private Color highlightColor = Color.black;
    private Color originalBackgroundColor;

    [SerializeField] private float notReadyShakeStrength = 10f;
    [SerializeField] private float notReadyShakeDuration = 0.4f;
    [SerializeField] private int notReadyShakeVibrato = 20;

    [SerializeField] private Color notReadyColor = Color.red;
    [SerializeField] private int notReadyBlinkCount = 2;

    [SerializeField] private float hoverLift = 30f;
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float hoverDuration = 0.15f;

    private RectTransform rectTransform;

    private int handSiblingIndex;
    private Vector3 originalLocalScale;

    private Vector2 handPosition;
    private float handAngle;

    private Perk perk;
    public Perk Perk => perk;

    public event Action<ActivePerkCard> OnClicked;
    private UnityAction onClicked_Lambda;

    void Awake()
    {
        effectRowPool = new ObjectPool<EffectRow>(effectRowPrefab, effectRowParent, 1);

        rectTransform = (RectTransform)transform;

        originalLocalScale = transform.localScale;

        originalBackgroundColor = perkCard_Background.color;

        onClicked_Lambda = () => OnClicked?.Invoke(this);
        perkCard_Button.onClick.AddListener(onClicked_Lambda);
    }

    void OnDestroy()
    {
        perkCard_Button.onClick.RemoveListener(onClicked_Lambda);
    }

    void OnDisable()
    {
        perkCard_Background.DOKill();

        effectRowPool.ReturnAllObjects();

        perk = null;

        SetHighlight(false);

        ((RectTransform)transform).anchoredPosition = Vector2.zero;

        transform.localRotation = Quaternion.identity;
    }

    public void SetPerkCardElements(Perk perk, PerkConfiguration perkConfig)
    {
        if (perk == null)
        {
            Debug.LogError("Active perk is null. Cannot set active perk card.", this);

            return;
        }

        this.perk = perk;

        effectRowPool.ReturnAllObjects();

        perkCardName_Text.text = perk.Name;

        foreach (PerkEffectEntry effect in perk.Effects)
        {
            EffectRow row = effectRowPool.GetObject();

            Sprite perkProfileIcon;

            float effectMultiplier = perk.GetEffectMultiplier(effect);

            if (effect.effect.TokenValue * effectMultiplier < 0)
            {
                perkProfileIcon = perkConfig.perkNegativeProfileIcon;
            }
            else if (effect.effect.TokenValue * effectMultiplier > 0)
            {
                perkProfileIcon = perkConfig.perkPositiveProfileIcon;
            }
            else
            {
                perkProfileIcon = perkConfig.perkNeutralProfileIcon;
            }

            row.SetEffectElements(effect.effect.GetDescription(effectMultiplier), perkProfileIcon, perk.PerkMultiplier);
        }
    }

    public void SetInteractable(bool interactable)
    {
        perkCard_Button.interactable = interactable;
    }

    public void UpdateCooldownDisplay()
    {
        if (perk == null)
        {
            return;
        }

        useCount_Text.text = perk.UseCount == -1 ? "" : perk.RemainingUses.ToString();

        if (perk.CooldownDuration != -1)
        {
            remainingCooldown_Text.text = Mathf.CeilToInt(perk.CooldownRemaining) <= 0 ? "" : Mathf.CeilToInt(perk.CooldownRemaining).ToString();

            remainingCooldown_Image.fillAmount = perk.CooldownDuration <= 0 ? 0f : perk.CooldownRemaining / perk.CooldownDuration;
        }
        else
        {
            remainingCooldown_Image.fillAmount = 0;
        }

        if (perk.CooldownQuestion != -1)
        {
            remainingQuestionCooldown_Text.text = perk.QuestionRemaining <= 0 ? "" : perk.QuestionRemaining.ToString();

            remainingQuestionCooldown_Image.fillAmount = perk.CooldownQuestion <= 0 ? 0f : perk.QuestionRemaining / (float)perk.CooldownQuestion;
        }
        else
        {
            remainingQuestionCooldown_Image.fillAmount = 0;
        }

        SetInteractable(perk.IsPerkActive);
    }

    public void SetHighlight(bool highlight)
    {
        perkCard_Outline.enabled = highlight;
        perkCard_Background.color = highlight ? highlightColor : originalBackgroundColor;
    }

    public void SetHandTransform(Vector2 position, float angle, int siblingIndex)
    {
        handPosition = position;
        
        ((RectTransform)transform).anchoredPosition = position;

        handAngle = angle;

        transform.localRotation = Quaternion.Euler(0, 0, angle);

        handSiblingIndex = siblingIndex;

        transform.SetSiblingIndex(siblingIndex);
    }

    public void PlayNotReadyFeedback()
    {
        transform.DOKill();
        perkCard_Background.DOKill();

        perkCard_Background.color = originalBackgroundColor;

        rectTransform.anchoredPosition = handPosition;

        transform.DOShakeRotation(notReadyShakeDuration, new Vector2(notReadyShakeStrength, 0f), notReadyShakeVibrato);

        perkCard_Background.DOColor(notReadyColor, notReadyShakeDuration / (notReadyBlinkCount * 2)).SetLoops(notReadyBlinkCount * 2, LoopType.Yoyo);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.SetAsLastSibling();

        transform.DOKill();

        rectTransform.DOAnchorPos(handPosition + Vector2.up * hoverLift, hoverDuration);

        transform.DOLocalRotate(Vector3.zero, hoverDuration);

        transform.DOScale(originalLocalScale * hoverScale, hoverDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();

        rectTransform.DOAnchorPos(handPosition, hoverDuration);

        transform.DOLocalRotate(new Vector3(0f, 0f, handAngle), hoverDuration);
        
        transform.DOScale(originalLocalScale, hoverDuration);
        
        transform.SetSiblingIndex(handSiblingIndex);
    }
}