using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PerkCard : MonoBehaviour
{
    [SerializeField] private Button perkCard_Button;
    [SerializeField] private TextMeshProUGUI perkName_Text;
    [SerializeField] private TextMeshProUGUI perkType_Text;
    private ObjectPool<EffectRow> effectRowPool;
    [SerializeField] private EffectRow effectRowPrefab;
    [SerializeField] private RectTransform effectRowParent;
    [SerializeField] private Image perkIcon_Image;

    [SerializeField] private CanvasGroup perkCard_CanvasGroup;
    [SerializeField] private float hiddenScale = 0.7f;
    [SerializeField] private float entranceDuration = 0.4f;
    [SerializeField] private Ease entranceEase = Ease.OutBack;
    private Vector3 originalLocalScale;
        
    public event Action<PerkCard> OnClicked;
    private UnityAction onClicked_Lambda;

    private Perk perk;
    public Perk Perk => perk;

    [SerializeField] private float focusScale = 1.4f;
    [SerializeField] private float riseDuration = 0.35f;
    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private float flyDuration = 0.45f;
    [SerializeField] private float flyEndScale = 0.2f;
    [SerializeField] private Ease riseEase = Ease.OutBack;
    [SerializeField] private Ease flyEase = Ease.InQuad;

    private Sequence flightSequence;
    private RectTransform rectTransform;

    void Awake()
    {
        effectRowPool = new ObjectPool<EffectRow>(effectRowPrefab, effectRowParent, 1);

        rectTransform = (RectTransform)transform;
        
        originalLocalScale = transform.localScale;

        onClicked_Lambda = () => OnClicked?.Invoke(this);
        perkCard_Button.onClick.AddListener(onClicked_Lambda);
    }

    void OnDisable()
    {
        transform.DOKill();
        perkCard_CanvasGroup.DOKill();
        flightSequence?.Kill();

        effectRowPool.ReturnAllObjects();

        transform.localScale = originalLocalScale;
        perkCard_CanvasGroup.alpha = 1f;

        perk = null;
    }

    void OnDestroy()
    {
        perkCard_Button.onClick.RemoveListener(onClicked_Lambda);
    }

    public void SetPerk(Perk perk, PerkConfiguration perkConfig)
    {
        if (perk == null)
        {
            Debug.LogError("Perk is null. Cannot set perk card.");

            return;
        }

        effectRowPool.ReturnAllObjects();

        this.perk = perk;

        perkName_Text.text = perk.Name;
        
        perkIcon_Image.sprite = perk.Icon;

        perkType_Text.text = perk.Type.ToString();

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

    public void PlayEntrance(float delay)
    {
        transform.DOKill();
        perkCard_CanvasGroup.DOKill();

        transform.localScale = originalLocalScale * hiddenScale;
        perkCard_CanvasGroup.alpha = 0f;

        transform.DOScale(originalLocalScale, entranceDuration).SetDelay(delay).SetEase(entranceEase);
        perkCard_CanvasGroup.DOFade(1f, entranceDuration).SetDelay(delay);
    }

    public void SetInteractable(bool interactable)
    {
        perkCard_Button.interactable = interactable;
    }

    public void PlaySelectionFlight(Vector2 centerPos, Vector2 targetPos, Action onPanelShouldClose, Action onArrived)
    {
        flightSequence?.Kill();
        transform.DOKill();
        perkCard_CanvasGroup.DOKill();

        perkCard_CanvasGroup.alpha = 1f;

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        flightSequence = DOTween.Sequence();

        flightSequence.Append(rectTransform.DOAnchorPos(centerPos, riseDuration).SetEase(riseEase));
        flightSequence.Join(transform.DOScale(originalLocalScale * focusScale, riseDuration).SetEase(riseEase));

        flightSequence.AppendCallback(() => onPanelShouldClose?.Invoke());

        flightSequence.AppendInterval(holdDuration);

        flightSequence.Append(rectTransform.DOAnchorPos(targetPos, flyDuration).SetEase(flyEase));
        flightSequence.Join(transform.DOScale(originalLocalScale * flyEndScale, flyDuration).SetEase(flyEase));

        flightSequence.OnComplete(() => onArrived?.Invoke());
    }
}