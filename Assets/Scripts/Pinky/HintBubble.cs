using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Maskotun ipucu metnini daktilo etkisiyle gösteren konuşma baloncuğu sınıfı.
/// </summary>
/// <remarks>
/// Baloncuk üç aşamalı çalışır: yazma sırasında tıklanırsa metin anında tamamlanır,
/// metin tamamlandıktan sonra tıklanırsa baloncuk kapanır.
/// </remarks>
[RequireComponent(typeof(AnimatedPanel))]
public class HintBubble : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private AnimatedPanel animatedPanel;
    [SerializeField] private TextMeshProUGUI hint_Text;

    [Header("Typewriter Settings")]
    [Tooltip("İki harf arasındaki bekleme süresi (saniye).")]
    [SerializeField] private float characterDelay = 0.03f;

    [Tooltip("Noktalama işaretlerinden sonra eklenen ek bekleme süresi (saniye).")]
    [SerializeField] private float punctuationDelay = 0.15f;

    [Tooltip("Ek bekleme uygulanacak noktalama karakterleri.")]
    [SerializeField] private string punctuationCharacters = ".,!?;:";

    /// <summary>
    /// Baloncuğun o an görünür olup olmadığını belirten veri.
    /// </summary>
    public bool IsOpen { get; private set; }

    /// <summary>
    /// Yazma animasyonunun devam edip etmediğini belirten veri.
    /// </summary>
    private bool isTyping;

    private Coroutine typeRoutine;

    private string fullText;

    /// <summary>
    /// Baloncuk kapandığında uyarılan metot aksiyonu.
    /// </summary>
    public event Action Closed;

    /// <summary>
    /// Baloncukta sırayla gösterilecek metinlerin dizisi.
    /// </summary>
    private string[] pages;

    /// <summary>
    /// Gösterilmekte olan metnin dizi içindeki index'i.
    /// </summary>
    private int pageIndex;

    void Awake()
    {
        if (animatedPanel == null) animatedPanel = GetComponent<AnimatedPanel>();
    }

    /// <summary>
    /// Baloncuğu açan ve verilen metni yazmaya başlayan metot.
    /// </summary>
    /// <param name="text">
    /// Baloncukta gösterilecek metin verisi.
    /// </param>
    public void Show(string text) => Show(new[] { text });

    /// <summary>
    /// Baloncuğu açan ve verilen metinleri sırayla yazan metot.
    /// </summary>
    /// <remarks>
    /// Son metin tamamlandıktan sonra yapılan tıklama baloncuğu kapatır.
    /// </remarks>
    /// <param name="texts">
    /// Sırayla gösterilecek metin dizisi.
    /// </param>
    public void Show(string[] texts)
    {
        if (texts == null || texts.Length == 0)
        {
            Debug.LogError("Invalid bubble text array.");
            return;
        }

        if (typeRoutine != null) StopCoroutine(typeRoutine);

        pages = texts;
        pageIndex = 0;

        hint_Text.text = string.Empty;

        IsOpen = true;

        animatedPanel.ShowPanel(() => StartPage());
    }

    /// <summary>
    /// Sıradaki metni yazmaya başlayan metot.
    /// </summary>
    private void StartPage()
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);

        fullText = pages[pageIndex] ?? string.Empty;

        hint_Text.text = string.Empty;

        typeRoutine = StartCoroutine(TypeRoutine());
    }

    /// <summary>
    /// Baloncuğu kapatan metot.
    /// </summary>
    public void Hide()
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);

        typeRoutine = null;
        isTyping = false;
        IsOpen = false;

        animatedPanel.HidePanel(() => { hint_Text.text = string.Empty; Closed?.Invoke(); });
    }

    /// <summary>
    /// Metni harf harf yazan animasyon döngüsü.
    /// </summary>
    private IEnumerator TypeRoutine()
    {
        isTyping = true;

        StringBuilder builder = new();

        foreach (char character in fullText)
        {
            builder.Append(character);

            hint_Text.text = builder.ToString();

            yield return new WaitForSeconds(characterDelay);

            if (punctuationCharacters.IndexOf(character) >= 0)
            {
                yield return new WaitForSeconds(punctuationDelay);
            }
        }

        isTyping = false;
        typeRoutine = null;
    }

    /// <summary>
    /// Yazma animasyonunu sonlandırıp metnin tamamını gösteren metot.
    /// </summary>
    private void CompleteTyping()
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);

        typeRoutine = null;
        isTyping = false;

        hint_Text.text = fullText;
    }

    /// <summary>
    /// Baloncuğu bir sonraki aşamaya ilerleten metot.
    /// Yazma sürerken metni tamamlar, tamamlanmışsa sıradaki metne geçer,
    /// son metinde ise baloncuğu kapatır.
    /// </summary>
    public void Advance()
    {
        if (!IsOpen) return;

        if (isTyping)
        {
            CompleteTyping();

            return;
        }

        if (pageIndex < pages.Length - 1)
        {
            pageIndex++;

            StartPage();

            return;
        }

        Hide();
    }

    public void OnPointerClick(PointerEventData eventData) => Advance();
}