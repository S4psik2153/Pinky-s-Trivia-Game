using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Oyun akışına tepki veren maskot karakterin animasyonlarını yöneten sınıf.
/// </summary>
[RequireComponent(typeof(Animator))]
public class Pinky : MonoBehaviour
{
    private static readonly int PinkyIdleHash = Animator.StringToHash("Pinky_Idle");
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Button characterButton;

    [Header("Blink Settings")]
    [Tooltip("İki göz kırpma arasındaki minimum bekleme süresi (saniye).")]
    [SerializeField] private float minBlinkInterval = 2f;

    [Tooltip("İki göz kırpma arasındaki maksimum bekleme süresi (saniye).")]
    [SerializeField] private float maxBlinkInterval = 5f;

    [Header("Animation Speed")]
    [Tooltip("Tüm maskot animasyonlarının oynatma hızı çarpanı.")]
    [SerializeField, Range(0.1f, 2f)] private float animationSpeed = 0.5f;

    private static readonly int IdleHash  = Animator.StringToHash("Idle");
    private static readonly int BlinkHash = Animator.StringToHash("Blink");
    private static readonly int JumpHash  = Animator.StringToHash("Jump");
    private static readonly int SadHash   = Animator.StringToHash("Sad");

    private Coroutine blinkRoutine;
    private Coroutine reactionRoutine;

    /// <summary>
    /// Karakterin şu an bir tepki animasyonu oynatıp oynatmadığını belirten veri.
    /// Tepki sırasında göz kırpma tetiklenmez.
    /// </summary>
    private bool isReacting;

    [Header("Hint")]
    [SerializeField] private HintBubble hintBubble;

    /// <summary>
    /// Maskota tıklandığında uyarılan metot aksiyonu.
    /// </summary>
    public event Action HintRequested;

    private UnityAction hintRequested_Lambda;

    /// <summary>
    /// İpucu baloncuğu kapandığında uyarılan metot aksiyonu.
    /// </summary>
    public event Action HintBubbleClosed;

    private Action hintBubbleClosed_Lambda;

    /// <summary>
    /// İpucu baloncuğunu açan ve verilen metinleri sırayla yazdıran metot.
    /// </summary>
    public void ShowHint(string[] hints) => hintBubble.Show(hints);

    /// <summary>
    /// İpucu baloncuğunu bir sonraki aşamaya ilerleten metot.
    /// </summary>
    public void AdvanceHint() => hintBubble.Advance();

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (characterButton != null) characterButton.interactable = false;

        if (characterButton != null)
        {
            characterButton.interactable = true;

            hintRequested_Lambda = () => HintRequested?.Invoke();
            characterButton.onClick.AddListener(hintRequested_Lambda);
        }

        animator.speed = animationSpeed;

        hintBubbleClosed_Lambda = () => HintBubbleClosed?.Invoke();
        hintBubble.Closed += hintBubbleClosed_Lambda;
    }

    void OnEnable()
    {
        blinkRoutine = StartCoroutine(BlinkLoop());
    }

    void OnDisable()
    {
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        if (reactionRoutine != null) StopCoroutine(reactionRoutine);

        blinkRoutine = null;
        reactionRoutine = null;
        isReacting = false;
    }

    void OnDestroy()
    {
        if (hintBubble != null) hintBubble.Closed -= hintBubbleClosed_Lambda;
        if (characterButton != null) characterButton.onClick.RemoveListener(hintRequested_Lambda);
    }

    /// <summary>
    /// Bekleyen tüm Animator trigger'larını temizleyen metot.
    /// Tüketilmemiş trigger'ların sonraki animasyonları kesmesini engeller.
    /// </summary>
    private void ClearAllTriggers()
    {
        animator.ResetTrigger(IdleHash);
        animator.ResetTrigger(BlinkHash);
        animator.ResetTrigger(JumpHash);
        animator.ResetTrigger(SadHash);
    }

    /// <summary>
    /// Boşta durumdayken rastgele aralıklarla göz kırpma animasyonunu tetikleyen döngü.
    /// </summary>
    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            float wait = UnityEngine.Random.Range(minBlinkInterval, Mathf.Max(minBlinkInterval, maxBlinkInterval));

            yield return new WaitForSeconds(wait);

            if (!isReacting) animator.SetTrigger(BlinkHash);
        }
    }

    /// <summary>
    /// Doğru cevap verildiğinde sevinç (zıplama) animasyonunu oynatan metot.
    /// </summary>
    public void PlayCorrectReaction() => PlayReaction(JumpHash);

    /// <summary>
    /// Yanlış cevap verildiğinde veya zaman aşımında üzülme animasyonunu oynatan metot.
    /// </summary>
    public void PlayWrongReaction() => PlayReaction(SadHash);

    /// <summary>
    /// Tepki animasyonunu başlatan ve süre sonunda boşta durumuna dönen metot.
    /// </summary>
    /// <param name="triggerHash">
    /// Oynatılacak tepki animasyonunun Animator trigger karşılığı.
    /// </param>
    private void PlayReaction(int triggerHash)
    {
        if (reactionRoutine != null) StopCoroutine(reactionRoutine);

        reactionRoutine = StartCoroutine(ReactionRoutine(triggerHash));
    }

    private IEnumerator ReactionRoutine(int triggerHash)
    {
        isReacting = true;

        ClearAllTriggers();
        animator.SetTrigger(triggerHash);

        yield return null;
        while (animator.IsInTransition(0)) yield return null;

        reactionRoutine = null;

        yield break;
    }

    /// <summary>
    /// Karakteri boşta durumuna döndüren metot.
    /// </summary>
    public void ResetToIdle()
    {
        if (reactionRoutine != null) StopCoroutine(reactionRoutine);

        reactionRoutine = null;
        isReacting = false;

        ClearAllTriggers();

        animator.Play(PinkyIdleHash, 0, 0f);
    }

    /// <summary>
    /// İpucu baloncuğunu açan ve verilen metni yazdıran metot.
    /// </summary>
    /// <param name="hint">
    /// Gösterilecek ipucu metni.
    /// </param>
    public void ShowHint(string hint)
    {
        if (hintBubble.IsOpen) return;

        hintBubble.Show(hint);
    }

    /// <summary>
    /// İpucu baloncuğunu kapatan metot.
    /// </summary>
    public void HideHint()
    {
        hintBubble.Hide();
    }
}