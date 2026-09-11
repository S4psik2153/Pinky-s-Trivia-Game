using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Durum sınıflarının <see cref="GameplayManager"/> aracılığı ile
/// oyun arayüzüne müdahale edebilmelerini sağlayan sınıf.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera canvasCamera;

    [Header("UI Elements")]
    [SerializeField]    private             TextMeshProUGUI             question_Text;
    [SerializeField]    private             TextMeshProUGUI             questionTitle_Text;
    [SerializeField]    private             TextMeshProUGUI             category_Text;
    [SerializeField]    private             TextMeshProUGUI             difficulty_Text;
    [SerializeField]    private             ChoiceButton                choiceButton_Prefab;
    [SerializeField]    private             RectTransform               choiceButton_Parent;
                        private             ObjectPool<ChoiceButton>    choiceButtonPool;
                        private readonly    List<ChoiceButton>          activeChoiceButtons = new();
    [SerializeField]    private             Button                      nextQuestion_Button;
    [SerializeField]    private             CanvasGroup                 nextQuestionButton_CanvasGroup;
    [SerializeField]    private             TextMeshProUGUI             score_Text;
    [SerializeField]    private             TextMeshProUGUI             scoreMultiplier_Text;
    [SerializeField]    private             RectTransform               score_Text_Parent;
    [SerializeField]    private             TextMeshProUGUI             timerCounter_Text;
    [SerializeField]    private             TextMeshProUGUI             timerSpeed_Text;
    [SerializeField]    private             RectTransform               timerCounter_Text_Parent;
    [SerializeField]    private             Image                       timerFill_Image;

    [SerializeField]    private             AnimatedPanel               animatedResultScreenPanel;
    [SerializeField]    private             TMP_InputField              userNicknameEntry_InputField;
    [SerializeField]    private             Button                      saveScore_Button;
    [SerializeField]    private             TextMeshProUGUI             saveFeedback_Text;
    [SerializeField]    private             Button                      backToMainMenu_Button;
    [SerializeField]    private             TextMeshProUGUI             resultScore_Displayer;

    [SerializeField]    private             Color                       defaultButtonColor          = Color.white;
    [SerializeField]    private             Color                       correctAnswerButtonColor    = Color.green;
    [SerializeField]    private             Color                       incorrectAnswerButtonColor  = Color.red;

    [SerializeField]    private             Color                       difficultyEasyTextColor     = Color.green;
    [SerializeField]    private             Color                       difficultyMediumTextColor   = Color.yellow;
    [SerializeField]    private             Color                       difficultyHardTextColor     = Color.red;

    /// <summary>
    /// Bir cevap seçeneği seçildiğinde uyarılan metot aksiyonu.
    /// <see langword="int"/> parametresi yapılan seçimde seçilen buton index'ini iletir.
    /// </summary>
    public event Action<int> OnAnswerSelected;

    /// <summary>
    /// Sonraki soruya gidilmek istendiğinde uyarılan metot aksiyonu.
    /// </summary>
    public event Action NextQuestionRequested;
    /// <summary>
    /// <see cref="NextQuestionRequested"/> aksiyonunun lambda notasyonu olarak kullanımı.
    /// <see cref="UnityEvent.RemoveListener"/> metodu referans karşılaştırması yaptığından
    /// lambda notasyonu olarak verilen aksiyonlar yeni bir referans yaratır.
    /// Bu yeni referans bizim kullandığımız aksiyona karşılık gelmediği için
    /// aksiyonu <see cref="Button.onClick"/> metodundan silebilmek için bu referans ile
    /// <see cref="UnityEvent.RemoveListener"/> ile kayıt olarak kullanıyoruz.
    /// </summary>
    private UnityAction nextQuestionRequested_Lambda;

    /// <summary>
    /// Oyun sonucu yerel Liderlik Tablosuna kaydedilmek istendiğinde uyarılan metot aksiyonu.
    /// </summary>
    /// <remarks>
    /// Parametre alanı kullanıcıdan alınan takma adını iletmek için kullanılıyor.
    /// </remarks>
    public event Action<string> SaveScoreRequested;
    /// <summary>
    /// <see cref="SaveScoreRequested"/> aksiyonunun lambda notasyonu olarak kullanımı.
    /// <see cref="UnityEvent.RemoveListener"/> metodu referans karşılaştırması yaptığından
    /// lambda notasyonu olarak verilen aksiyonlar yeni bir referans yaratır.
    /// Bu yeni referans bizim kullandığımız aksiyona karşılık gelmediği için
    /// aksiyonu <see cref="Button.onClick"/> metodundan silebilmek için bu referans ile
    /// <see cref="UnityEvent.RemoveListener"/> ile kayıt olarak kullanıyoruz.
    /// </summary>
    private UnityAction saveScoreRequested_Lambda;

    public event Action PerkPanelToggled;
    private UnityAction perkPanelToggled_Lambda;

    /// <summary>
    /// Ana menüye dönülmek istendiğinde uyarılan metot aksiyonu.
    /// </summary>
    public event Action BackToMainMenu;
    /// <summary>
    /// <see cref="BackToMainMenu"/> aksiyonunun lambda notasyonu olarak kullanımı.
    /// <see cref="UnityEvent.RemoveListener"/> metodu referans karşılaştırması yaptığından
    /// lambda notasyonu olarak verilen aksiyonlar yeni bir referans yaratır.
    /// Bu yeni referans bizim kullandığımız aksiyona karşılık gelmediği için
    /// aksiyonu <see cref="Button.onClick"/> metodundan silebilmek için bu referans ile
    /// <see cref="UnityEvent.RemoveListener"/> ile kayıt olarak kullanıyoruz.
    /// </summary>
    private UnityAction backToMainManu_Lambda;

    public event Action<Perk> PerkSelected;

    public event Action<Perk> PerkUsed;

    [Header("Entry Messages")]
    [SerializeField] private string textTooLongError        = "Nickname you entered is too long (Max {0} character)!";
    [SerializeField] private string textIsWhiteSpaceError   = "Nickname you entered is empty or white space!";
    [SerializeField] private string textIsValid             = "Your nickname is valid. To save your score to local Leader Board, click the save button.";
    [SerializeField] private string saveSuccess             = "Score saved successfully to local Leader Board.";
    [SerializeField] private string saveFailure             = "Error occurred while saving. Please try again.";

    [SerializeField] private Color  default_SaveTextColor   = Color.black;
    [SerializeField] private Color  error_SaveTextColor     = Color.red;
    [SerializeField] private Color  success_SaveTextColor   = Color.green;

    [SerializeField] private int nicknameEntryLengthLimit = 20;

    [Header("Score Animations")]
    [SerializeField]    private AnimatedFloatingScore   floatingScore_Prefab;
    [SerializeField]    private RectTransform           floatingScore_Parent;

    [SerializeField]    private Vector2 floatingScoreStartOffset    = new(200f, 0f);
    [SerializeField]    private float   scoreGrowScale              = 1.2f;
    [SerializeField]    private float   scorePunchScale             = 0.2f;
    [SerializeField]    private float   scoreCountDuration          = 0.4f;
    [SerializeField]    private float   scoreScaleDuration          = 0.15f;
    [SerializeField]    private float   scorePunchDuration          = 0.2f;

    private ObjectPool<AnimatedFloatingScore>   floatingScorePool;
    private int                                 displayedScore;
    private Vector3                             scoreTextOriginalScale;

    [Header("Result Score Animations")]
    [SerializeField] private ScoreColorTier[] scoreColorTiers;
    [SerializeField] private Color defaultScoreColor = Color.black;
    [SerializeField] private float resultScoreAnimationDuration = 1f;
    [SerializeField] private float resultScorePunchScale = 0.2f;
    [SerializeField] private float resultScorePunchDuration = 0.25f;

    private Tweener resultScoreTween;
    private Color lastTierColor;
    private int resultScoreAnimationCounter;

    [SerializeField] private int nthIsWinner = 5;
    [SerializeField] private float infinityWidth = 60f;
    [SerializeField] private float infinityHeight = 30f;
    [SerializeField] private float infinityDuration = 1f;

    private float infinityAngle;
    private Tweener infinityTween;
    private Vector2 resultScoreOriginalPosition;
    private Vector3 resultScoreOriginalScale;
    private Tweener rainbowTween;
    private float rainbowHue;

    [SerializeField] private float rainbowColorDuration = 1f;
    [SerializeField] private float impossibleScoreMultiplier = 100f;
    [SerializeField] private float rainbowColorSaturation = 1f;
    [SerializeField] private float rainbowColorValue = 1f;

    [Header("Timer Animations")]
    [SerializeField] private float timerPunchScale          = 0.25f;
    [SerializeField] private float timerPunchAngle          = 12f;
    [SerializeField] private float timerPunchDuration       = 0.25f;
    [SerializeField] private float timerPulseScale          = 1.15f;

    [SerializeField] private float timerPulseGrowDuration   = 0.15f;
    [SerializeField] private float timerPulseShrinkDuration = 0.5f;
    [SerializeField] private Ease  timerPulseGrowEase       = Ease.OutQuad;
    [SerializeField] private Ease  timerPulseShrinkEase     = Ease.InOutSine;

    [SerializeField] private Color timeoutColor             = Color.red;
    [SerializeField] private float timeoutPunchScale        = 0.5f;
    [SerializeField] private float timeoutShakeAngle        = 25f;
    [SerializeField] private float timeoutDuration          = 0.6f;

    private Color timerTextOriginalColor;

    private Sequence timerPulseSequence;

    [Header("Answer Button Animations")]
    [SerializeField] private float choiceStaggerDelay = 0.06f;

    private Vector3 timerTextOriginalScale;
    private Quaternion timerTextOriginalRotation;

    [Header("Perk Selection")]
    [SerializeField] private AnimatedPanel animatedPerkSelectionPanel;
    [SerializeField] private PerkCard perkCard_Prefab;
    [SerializeField] private RectTransform perkCard_Parent;
    [SerializeField] private float perkCardStaggerDelay = 0.08f;
    [SerializeField] private Button showPerkPanel_Button;
    [SerializeField] private Button hidePerkPanel_Button;

    private PerkConfiguration perkConfig;

    private ObjectPool<PerkCard> perkCardPool;
    private readonly List<PerkCard> perkCardList = new();

    [SerializeField] private RectTransform flightLayer;

    [Header("Active Perk Cards")]
    [SerializeField] private ActivePerkCard activePerkCardPrefab;
    [SerializeField] private RectTransform activePerkCardParent;

    private ObjectPool<ActivePerkCard> activePerkCardPool;
    private readonly List<ActivePerkCard> activePerkCardList = new();
    
    [Header("Active Perk Inventory")]
    [SerializeField] private AnimatedPanel activeCardInventoryPanel;
    [SerializeField] private CanvasGroup activePerkCard_CanvasGroup;

    [SerializeField] private float maxSpacing = 180f;
    [SerializeField] private float arcDepth = 30f;
    [SerializeField] private float maxRotation = 10f;

    [Header("Passive Perk Display")]
    [SerializeField] private PassivePerkDisplay passivePerkDisplayPrefab;
    [SerializeField] private RectTransform passivePerkDisplayParent;

    private ObjectPool<PassivePerkDisplay> passivePerkDisplayPool;
    private readonly List<PassivePerkDisplay> passivePerkDisplayList = new();

    [Header("Passive Perk Tooltip")]
    [SerializeField] private CanvasGroup perkTooltip_CanvasGroup;
    [SerializeField] private RectTransform perkTooltip_Rect;
    [SerializeField] private TextMeshProUGUI tooltipName_Text;

    private ObjectPool<EffectRow> passivePerkDisplayEffectRowPool;

    [SerializeField] private EffectRow passivePerkDisplayEffectRowPrefab;
    [SerializeField] private RectTransform passivePerkDisplayEffectRowParent;

    public int ChoiceCount => activeChoiceButtons.Count;

    [Header("Mascot")]
    [SerializeField] private Pinky mascot;

    /// <summary>
    /// Oyun akışına tepki veren maskot karakterin referansı.
    /// </summary>
    public Pinky Mascot => mascot;

    [Header("Hint Messages")]
    [Tooltip("Sorunun ipucu bulunmadığında maskotun söyleyeceği metin.")]
    [SerializeField] private string noHintMessage = "Hmm... bu soru hakkında hiçbir fikrim yok!";

    /// <summary>
    /// Maskota tıklandığında uyarılan metot aksiyonu.
    /// </summary>
    public event Action MascotHintRequested;

    [Header("Introduction")]
    [Tooltip("Maskotun oyun başında sırayla söyleyeceği tanıtım metinleri.")]
    [SerializeField, TextArea] private string[] introductionMessages;

    public string[] IntroductionMessages => introductionMessages;

    public void ShowMascotIntroduction() => mascot.ShowHint(introductionMessages);

    [Header("Countdown")]
    [SerializeField] private AnimatedPanel countdownPanel;
    [SerializeField] private TextMeshProUGUI countdown_Text;

    [Tooltip("Geri sayımın başlayacağı sayı.")]
    [SerializeField] private int countdownStart = 3;

    [Tooltip("Her sayının ekranda kalma süresi (saniye).")]
    [SerializeField] private float countdownStepDuration = 1f;

    [Tooltip("Geri sayım sayılarının vurgu animasyonu şiddeti.")]
    [SerializeField] private float countdownPunchScale = 0.4f;

    private Vector3 countdownTextOriginalScale;

    /// <summary>
    /// Maskotun ipucu baloncuğu kapandığında uyarılan metot aksiyonu.
    /// </summary>
    public event Action MascotBubbleClosed;

    [Header("Tap Catcher")]
    [SerializeField] private FullScreenTapCatcher tapCatcher;

    /// <summary>
    /// Ekranın herhangi bir yerine dokunulduğunda uyarılan metot aksiyonu.
    /// </summary>
    public event Action ScreenTapped;

    /// <summary>
    /// Tam ekran dokunma yakalayıcısını açıp kapatan metot.
    /// </summary>
    public void SetTapCatcherActive(bool active) => tapCatcher.SetActive(active);

    /// <summary>
    /// Maskotun baloncuğunu bir sonraki aşamaya ilerleten metot.
    /// </summary>
    public void AdvanceMascotBubble() => mascot.AdvanceHint();

    private void OnScreenTapped() => ScreenTapped?.Invoke();

    void Start()
    {
        choiceButtonPool = new ObjectPool<ChoiceButton>(choiceButton_Prefab, choiceButton_Parent, 4);

        floatingScorePool = new ObjectPool<AnimatedFloatingScore>(floatingScore_Prefab, floatingScore_Parent);

        perkCardPool = new ObjectPool<PerkCard>(perkCard_Prefab, perkCard_Parent, 3);

        activePerkCardPool = new ObjectPool<ActivePerkCard>(activePerkCardPrefab, activePerkCardParent);

        passivePerkDisplayPool = new ObjectPool<PassivePerkDisplay>(passivePerkDisplayPrefab, passivePerkDisplayParent);

        passivePerkDisplayEffectRowPool = new ObjectPool<EffectRow>(passivePerkDisplayEffectRowPrefab, passivePerkDisplayEffectRowParent);

        scoreTextOriginalScale = score_Text_Parent.transform.localScale;

        timerTextOriginalScale = timerCounter_Text_Parent.transform.localScale;
        timerTextOriginalRotation = timerCounter_Text_Parent.transform.localRotation;

        timerTextOriginalColor = timerCounter_Text.color;

        saveScore_Button.interactable = false;

        nextQuestionRequested_Lambda = () => NextQuestionRequested?.Invoke();
        nextQuestion_Button.onClick.AddListener(nextQuestionRequested_Lambda);

        userNicknameEntry_InputField.onValueChanged.AddListener(ValidateNickname);
        saveScoreRequested_Lambda = () => SaveScoreRequested?.Invoke(userNicknameEntry_InputField.text);
        saveScore_Button.onClick.AddListener(saveScoreRequested_Lambda);

        backToMainManu_Lambda = () => BackToMainMenu?.Invoke();
        backToMainMenu_Button.onClick.AddListener(backToMainManu_Lambda);

        perkPanelToggled_Lambda = () => PerkPanelToggled?.Invoke();
        hidePerkPanel_Button.onClick.AddListener(perkPanelToggled_Lambda);
        showPerkPanel_Button.onClick.AddListener(perkPanelToggled_Lambda);

        mascot.HintRequested += OnMascotHintRequested;
        mascot.HintBubbleClosed += OnMascotBubbleClosed;
        tapCatcher.Tapped += OnScreenTapped;

        resultScoreOriginalPosition = resultScore_Displayer.rectTransform.anchoredPosition;
        resultScoreOriginalScale = resultScore_Displayer.transform.localScale;

        countdownTextOriginalScale = countdown_Text.transform.localScale;
    }

    void OnDestroy()
    {
        foreach (ChoiceButton button in activeChoiceButtons) if (button != null) button.OnClicked -= OnChoiceButtonClicked;

        foreach (PerkCard card in perkCardList) if (card != null) card.OnClicked -= OnPerkCardSelected;

        foreach (ActivePerkCard activePerkCard in activePerkCardList) if (activePerkCard != null) activePerkCard.OnClicked -= OnActivePerkCardSelected;

        foreach (PassivePerkDisplay display in passivePerkDisplayList)
        {
            if (display == null) continue;

            display.OnHoverEnter -= ShowPassivePerkTooltip;
            display.OnHoverExit -= HidePassivePerkTooltip;
        }

        if (mascot != null)
        {
            mascot.HintRequested -= OnMascotHintRequested;
            mascot.HintBubbleClosed -= OnMascotBubbleClosed;
        }

        if (tapCatcher != null)
        {
            tapCatcher.Tapped += OnScreenTapped;
        }

        if (nextQuestion_Button != null) nextQuestion_Button.onClick.RemoveListener(nextQuestionRequested_Lambda);

        if (userNicknameEntry_InputField != null) userNicknameEntry_InputField.onValueChanged.RemoveListener(ValidateNickname);
        if (saveScore_Button != null) saveScore_Button.onClick.RemoveListener(saveScoreRequested_Lambda);

        if (backToMainMenu_Button != null) backToMainMenu_Button.onClick.RemoveListener(backToMainManu_Lambda);

        if (countdown_Text != null) countdown_Text.transform.DOKill();

        if (score_Text_Parent != null) score_Text_Parent.DOKill();
        if (timerCounter_Text_Parent != null) timerCounter_Text_Parent.DOKill();
        if (timerCounter_Text != null) timerCounter_Text.DOKill();
        if (countdown_Text != null) countdown_Text.transform.DOKill();
        if (resultScore_Displayer != null) resultScore_Displayer.transform.DOKill();
    }

    /// <summary>
    /// Soru verilerini ekrana yazdıran metot.
    /// <see cref="QuestionManager"/> sınıfından gelen soru verileri ve
    /// seçenek butonları bu metotta ayarlanır.
    /// </summary>
    /// <remarks>
    /// <see cref="Question"/> verisinde <see cref="Question.choices"/> dizisi için <c>0</c> index'i
    /// her zaman doğru cevap olarak ayarlanır.
    /// </remarks>
    /// <param name="question">
    /// <see cref="QuestionManager"/> sınıfından gelen soru verileri.
    /// </param>
    /// <param name="questionNumber">
    /// <see cref="GameplayManager"/> sınıfından gelen hangi soruda olunduğunu belirten veri.
    /// </param>
    /// <param name="shuffledChoices">
    /// Butonlara atanacak seçenek metinlerinin karıştırılarak iletildiği veri dizisi.
    /// </param>
    public void ShowQuestion(Question question, int questionNumber, string[] shuffledChoices)
    {
        if (question == null || question.choices == null)
        {
            Debug.LogError("Null or invalid question.");
            return;
        }

        if (shuffledChoices == null)
        {
            Debug.LogError("Null shuffled choice array.");
            return;
        }

        ResetTimerVisual();

        foreach (ChoiceButton button in activeChoiceButtons) button.OnClicked -= OnChoiceButtonClicked;
        
        choiceButtonPool.ReturnAllObjects();

        activeChoiceButtons.Clear();

        for (int i = 0; i < shuffledChoices.Length; i++)
        {
            ChoiceButton button = choiceButtonPool.GetObject();
            button.SetChoiceText(shuffledChoices[i]);
            button.SetButtonColor(defaultButtonColor);
            button.OnClicked += OnChoiceButtonClicked;
            activeChoiceButtons.Add(button);

            button.PlayEntrance(i * choiceStaggerDelay);
        }

        question_Text.text = question.question;

        questionTitle_Text.text = $"Question number: {questionNumber}";

        category_Text.text = question.category;

        if (question.difficulty == QuestionDifficulty.easy)
        {
            difficulty_Text.color = difficultyEasyTextColor;
        }
        else if (question.difficulty == QuestionDifficulty.medium)
        {
            difficulty_Text.color = difficultyMediumTextColor;
        }
        else if (question.difficulty == QuestionDifficulty.hard)
        {
            difficulty_Text.color = difficultyHardTextColor;
        }

        difficulty_Text.text = $"Difficulty: {question.difficulty}";
    }

    /// <summary>
    /// Parametre olarak gönderilen buton referansının
    /// <see cref="activeChoiceButtons"/> listesindeki yerine göre
    /// index değerini <see cref="OnAnswerSeleceted"/> aksiyonuna ileten metot.
    /// </summary>
    /// <remarks>
    /// Hatalı index değerleri dizi sınırlarını korumak amacıyla elenir.
    /// </remarks>
    /// <param name="button">
    /// Index değeri <see cref="activeChoiceButtons"/> listesindeki yerine göre
    /// bilinmesi istenen <see cref="ChoiceButton"/> referansı.
    /// </param>
    private void OnChoiceButtonClicked(ChoiceButton button)
    {
        int index = activeChoiceButtons.IndexOf(button);
        
        if (index == -1)
        {
            Debug.LogError("Invalid button index.");
            return;
        }
        
        OnAnswerSelected?.Invoke(index);
    }

    /// <summary>
    /// Seçenek butonlarının erişilebilirliğini kontrol eden metot.
    /// </summary>
    /// <param name="interactable">
    /// <see langword="false"/> seçenek butonlarının etkileşime geçilebilirliğini kapatır,
    /// <see langword="true"/> seçenek butonlarının etkileşime geçilebilirliğini açar.
    /// </param>
    public void SetChoiceButtonsInteractable(bool interactable)
    {
        foreach (ChoiceButton choice in activeChoiceButtons)
        {
            choice.SetButtonInteractable(interactable);
        }
    }

    /// <summary>
    /// Sonraki soru butonunun erişilebilirliğini kontrol eden metot.
    /// </summary>
    /// <param name="active">
    /// <see langword="false"/> soru butonunun görünürlüğünü, etkileşime geçilebilirliğini ve raycast geçirgenliğini kapatır,
    /// <see langword="true"/> soru butonunun görünürlüğünü, etkileşime geçilebilirliğini ve raycast geçirgenliğini açar.
    /// </summary>
    public void SetNextQuestionButtonActive(bool active)
    {
        nextQuestionButton_CanvasGroup.alpha = active ? 1f : 0f;
        nextQuestionButton_CanvasGroup.interactable = active;
        nextQuestionButton_CanvasGroup.blocksRaycasts = active;
    }

    /// <summary>
    /// Seçilen cevap index'ine göre
    /// doğru cevabın index'ini karşılaştırır.
    /// Sonuca göre buton renklerini ayarlar.
    /// </summary>
    /// <param name="selectedIndex">
    /// Oyuncunun seçtiği seçenek butonun index değeri.
    /// <c>-1</c> değeri sorunun zaman aşımına uğradığı anlamına gelir.
    /// </param>
    /// <param name="correctIndex">
    /// Sorunun doğru seçeneğinin index değeri.
    /// </param>
    public void ShowAnswerResult(int selectedIndex, int correctIndex)
    {
        if (correctIndex == -1 || correctIndex >= activeChoiceButtons.Count || selectedIndex >= activeChoiceButtons.Count)
        {
            // Geçersiz index.
            Debug.LogError("Invalid correct answer or choice index.");
            return;
        }
        
        if (selectedIndex == correctIndex || selectedIndex == -1)
        {
            // Doğru cevap seçeneği seçildi veya zaman aşımına uğrandı.
            activeChoiceButtons[correctIndex].SetButtonColor(correctAnswerButtonColor);

            activeChoiceButtons[correctIndex].PlayCorrectFeedback();
        }
        else
        {
            // Yanlış cevap seçeneği seçildi.
            activeChoiceButtons[selectedIndex].SetButtonColor(incorrectAnswerButtonColor);
            activeChoiceButtons[correctIndex].SetButtonColor(correctAnswerButtonColor);

            activeChoiceButtons[selectedIndex].PlayWrongFeedback();
            activeChoiceButtons[correctIndex].PlayCorrectFeedback();
        }
    }

    /// <summary>
    /// Sonuç ekranını gösteren metot.
    /// </summary>
    public void ShowResultScreen()
    {
        animatedResultScreenPanel.ShowPanel();
    }

    /// <summary>
    /// Sonuç ekranını gizleyen metot.
    /// </summary>
    public void HideResultScreen()
    {
        resultScoreTween?.Kill();
        infinityTween?.Kill();
        rainbowTween?.Kill();

        resultScore_Displayer.rectTransform.anchoredPosition = resultScoreOriginalPosition;
        resultScore_Displayer.transform.localScale = resultScoreOriginalScale;

        animatedResultScreenPanel.HidePanel();
    }
    
    /// <summary>
    /// Sonuç skorunu belirleyen metot.
    /// </summary>
    /// <remarks>
    /// <paramref name="score"/> verisinin
    /// ne kadar yüksek bir skor olduğuna göre bir animasyon oynatılır.
    /// </remarks>
    /// <param name="score">
    /// Sonuç skoruna yazılması için iletilen skor puanı verisi.
    /// </param>
    public void SetResultScore(int score)
    {
        resultScoreTween?.Kill();
        infinityTween?.Kill();
        rainbowTween?.Kill();

        resultScore_Displayer.rectTransform.anchoredPosition = resultScoreOriginalPosition;
        resultScore_Displayer.transform.localScale = resultScoreOriginalScale;

        resultScoreAnimationCounter = 0;

        lastTierColor = defaultScoreColor;
        resultScore_Displayer.color = defaultScoreColor;

        void ScoreAnimation(int scoreCounter)
        {
            resultScoreAnimationCounter = scoreCounter;
            resultScore_Displayer.text = scoreCounter.ToString();

            Color currentColor = GetColorForScore(scoreCounter);

            if (lastTierColor != currentColor)
            {
                resultScore_Displayer.transform.DOKill();

                resultScore_Displayer.color = currentColor;

                lastTierColor = resultScore_Displayer.color;

                resultScore_Displayer.transform.localScale = resultScoreOriginalScale;

                resultScore_Displayer.transform.DOPunchScale(GetTierIndexForScore(scoreCounter) * resultScorePunchScale * Vector3.one, resultScorePunchDuration);
            }
        }

        resultScoreTween = DOTween.To(() => resultScoreAnimationCounter, ScoreAnimation, score, Mathf.Max(1, GetTierIndexForScore(score) * resultScoreAnimationDuration));

        if (scoreColorTiers.Length >= nthIsWinner && score >= scoreColorTiers[^nthIsWinner].colorThreshold)
        {
            resultScoreTween.OnComplete(() => PlayWinnerAnimation(score));
        }
    }

    /// <summary>
    /// <paramref name="score"/> verisinin seviyesine göre
    /// skorun <see cref="scoreColorTiers"/> renk karşılığı.
    /// </summary>
    /// <param name="score">
    /// Kazanılan skor verisi.
    /// </param>
    /// <returns>
    /// <paramref name="score"/> verisinin renk seviyesi karşılığı.
    /// </returns>
    private Color GetColorForScore(int score)
    {
        for (int i = scoreColorTiers.Length - 1; i >= 0; i--)
        {
            if (score >= scoreColorTiers[i].colorThreshold)
            {
                return scoreColorTiers[i].color;
            }
        }

        return defaultScoreColor;
    }

    /// <summary>
    /// <paramref name="score"/> verisinin seviyesine göre
    /// skorun <see cref="scoreColorTiers"/> index'i karşılığı.
    /// </summary>
    /// <param name="score">
    /// Kazanılan skor verisi.
    /// </param>
    /// <returns>
    /// <paramref name="score"/> verisinin
    /// <see cref="scoreColorTiers"/> index'i karşılığı.
    /// </returns>
    private int GetTierIndexForScore(int score)
    {
        for (int i = scoreColorTiers.Length - 1; i >= 0; i--)
        {
            if (score >= scoreColorTiers[i].colorThreshold)
            {
                return i + 1;
            }
        }

        return 0;
    }

    /// <summary>
    /// Belirli bir eşik puanın üzerinde bir skor kazanılması durumunda
    /// özel kazanma animasyonunu oynatan metot.
    /// </summary>
    /// <param name="score">
    /// Kazanılan skor verisi.
    /// </param>
    private void PlayWinnerAnimation(int score)
    {
        infinityAngle = 0f;

        void InfinityMovementAnimation(float t)
        {
            infinityAngle = t;

            float x = infinityWidth * Mathf.Sin(t);
            float y = infinityHeight * Mathf.Sin(t) * Mathf.Cos(t);
            resultScore_Displayer.rectTransform.anchoredPosition = resultScoreOriginalPosition + new Vector2(x, y);
        }

        infinityTween = DOTween.To(() => infinityAngle, InfinityMovementAnimation, 2 * Mathf.PI, infinityDuration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);

        if (scoreColorTiers.Length > 0 && score >= scoreColorTiers[^1].colorThreshold && score < scoreColorTiers[^1].colorThreshold * impossibleScoreMultiplier)
        {
            rainbowHue = 0f;

            void RainbowColorAnimation(float t)
            {
                rainbowHue = t;
                resultScore_Displayer.color = Color.HSVToRGB(rainbowHue, rainbowColorSaturation, rainbowColorValue);
            }

            rainbowTween = DOTween.To(() => rainbowHue, RainbowColorAnimation, 1f, rainbowColorDuration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
        }
    }

    /// <summary>
    /// <see cref="userNicknameEntry_InputField"/> metin kutusunun
    /// içeriği her güncellendiğinde çağırılan ve
    /// girilen metnin geçerliliğini kontrol eden metot.
    /// </summary>
    /// <remarks>
    /// Girilen metne göre <see cref="saveFeedback_Text"/> ve
    /// <see cref="saveScore_Button"/> özellikleri ayarlanır.
    /// </remarks>
    /// <param name="nicknameEntry">
    /// <see cref="userNicknameEntry_InputField"/> metin kutusu
    /// içerisine yazılan metin verisi.
    /// </param>
    private void ValidateNickname(string nicknameEntry)
    {
        // Girilen metin boş veya sadece boşluktan mı oluşuyor?
        if (string.IsNullOrWhiteSpace(nicknameEntry))
        {
            saveFeedback_Text.color = error_SaveTextColor;
            saveFeedback_Text.text = textIsWhiteSpaceError;
            SetSaveResultScoreButtonInteractable(false);
        }
        else
        {
            // Girilen metin harf sınırını aşıyor mu?
            if (nicknameEntry.Length > nicknameEntryLengthLimit)
            {
                saveFeedback_Text.color = error_SaveTextColor;
                saveFeedback_Text.text = string.Format(textTooLongError, nicknameEntryLengthLimit);
                SetSaveResultScoreButtonInteractable(false);
            }
            else
            {
                // Girilen metin geçerli.
                saveFeedback_Text.color = default_SaveTextColor;
                saveFeedback_Text.text = textIsValid;
                SetSaveResultScoreButtonInteractable(true);
            }
            
        }
    }

    /// <summary>
    /// <see cref="ValidateNickname"/> metodunun butonları
    /// aktif veya deaktif hale getirmesine yardımcı olmak için
    /// oluşturulmuş yardımcı metot.
    /// </summary>
    /// <param name="interactable">
    /// <see langword="false"/> sonuç kayıt butonlarının etkileşime geçilebilirliğini kapatır,
    /// <see langword="true"/> sonuç kayıt butonlarının etkileşime geçilebilirliğini açar.
    /// </param>
    private void SetSaveResultScoreButtonInteractable(bool interactable)
    {
        saveScore_Button.interactable = interactable;
    }

    /// <summary>
    /// Geçerli girdi sonrası Liderlik Tablosu dosyası güncellenirken başka bir girdi yazılamaması için
    /// <see cref="userNicknameEntry_InputField"/> girdi alanını kapatıp açmayı sağlayan metot.
    /// </summary>
    /// <remarks>
    /// Aynı zamanda <see cref="SetSaveResultScoreButtonInteractable"/> metodunu da
    /// gönderilen parametre ile çağırır.
    /// </remarks>
    /// <param name="interactable">
    /// <see langword="false"/> kullanıcı takma adı girdi kutusunun etkileşime geçilebilirliğini kapatır,
    /// <see langword="true"/> kullanıcı takma adı girdi kutusunun etkileşime geçilebilirliğini açar.
    /// </param>
    public void SetEntryInputFieldInteractable(bool interactable)
    {
        SetSaveResultScoreButtonInteractable(interactable);

        userNicknameEntry_InputField.interactable = interactable;
    }

    /// <summary>
    /// Girdi geri bildirimlerinin iletilmesi için kullanılan bir metot.
    /// </summary>
    /// <param name="isError">
    /// Geribildirimin bir hata olup olmadığını belirten bayrak verisi
    /// </param>
    public void SetSaveFeedback(bool isError)
    {
        if (isError)
        {
            saveFeedback_Text.color = error_SaveTextColor;
            saveFeedback_Text.text  = saveFailure;
        }
        else
        {
            saveFeedback_Text.color = success_SaveTextColor;
            saveFeedback_Text.text  = saveSuccess;
        }
    }

    /// <summary>
    /// Bir skor değişimi yaşandığında skoru güncelleyen metot.
    /// </summary>
    /// <param name="newScore">
    /// Güncellenecek skorun hesaplanmış verisi.
    /// </param>
    /// <param name="scoreMultiplier">
    /// Skor çarpanı verisi.
    /// </param>
    public void UpdateScore(int newScore, float scoreMultiplier)
    {
        scoreMultiplier_Text.text = $"x{scoreMultiplier:0.0}";

        int delta = newScore - displayedScore;

        if (delta == 0) return;
        
        score_Text_Parent.transform.DOKill();

        score_Text_Parent.transform.DOScale(scoreTextOriginalScale * scoreGrowScale, scoreScaleDuration);

        AnimatedFloatingScore deltaScore_Text = floatingScorePool.GetObject();

        Vector2 target = score_Text_Parent.anchoredPosition;

        Vector2 start = target + floatingScoreStartOffset;

        void onCountComplete() => score_Text_Parent.localScale = scoreTextOriginalScale;

        void onArrived()
        {
            score_Text_Parent.DOPunchScale(Vector3.one * scorePunchScale, scorePunchDuration);

            void SetDisplayedScore(int value)
            {
                displayedScore = value;
                score_Text.text = value.ToString();
            }

            DOTween.To(() => displayedScore, SetDisplayedScore, newScore, scoreCountDuration).OnComplete(onCountComplete);

            floatingScorePool.ReturnObject(deltaScore_Text);
        }

        deltaScore_Text.Play(delta, start, target, onArrived);
    }

    /// <summary>
    /// Zamanlayıcıda bir süre değişimi yaşandığında
    /// zamanlayıcıyı güncelleyen metot.
    /// </summary>
    /// <remarks>
    /// Zamanlayıcının süre değerini <see langword="int"/> veri tipinde gösteren aşırı yükleme.
    /// </remarks>
    /// <param name="remainingTime">
    /// Güncellenecek zamanlayıcı süresinin değer verisi.
    /// </param>
    /// <param name="timerSpeed">
    /// Zamanlayıcı hızı verisi.
    /// </param>
    public void UpdateTimer(int remainingTime, float timerSpeed)
    {
        PulseTimer();

        timerSpeed_Text.text = $"x{timerSpeed:0.0}";

        // Tek basamaklı, basmak sayısına göre olmayan gösterim formatı.
        timerCounter_Text.text = remainingTime.ToString("0");
    }

    /// <summary>
    /// Zamanlayıcıda bir süre değişimi yaşandığında
    /// zamanlayıcıyı güncelleyen metot.
    /// </summary>
    /// <remarks>
    /// Zamanlayıcının süre değerini <see langword="float"/> veri tipinde gösteren aşırı yükleme.
    /// </remarks>
    /// <param name="remainingTime">
    /// Güncellenecek zamanlayıcı süresinin değer verisi.
    /// </param>
    /// <param name="timerSpeed">
    /// Zamanlayıcı hızı verisi.
    /// </param>
    public void UpdateTimer(float remainingTime, float timerSpeed)
    {
        timerSpeed_Text.text = $"x{timerSpeed:0.0}";

        // tek basamaklı, virgülden sonra tek basamak gösteren gösterim formatı.
        timerCounter_Text.text = remainingTime.ToString("0.0");
    }

    /// <summary>
    /// Zamanlayıcıda kalan süreye göre,
    /// 1 saniyenin geçmesine kalan süreyi görselleştiren
    /// <see cref="timerFill_Image"/> resminin doluluk oranını
    /// ayarlayan metot.
    /// </summary>
    /// <param name="remainingTime">
    /// Zamanlayıcıda geriya kalan süre verisi.
    /// <see cref="timerFill_Image"/> dolum yüzdesini
    /// belirlemek için kullanılıyor.
    /// </param>
    public void UpdateTimerFill(float remainingTime)
    {
        timerFill_Image.fillAmount = remainingTime - Mathf.Floor(remainingTime);
    }

    /// <summary>
    /// Zamanlayıcı (kritik sürede değilken)
    /// animasyonunu uygulayan metot.
    /// </summary>
    public void PulseTimer()
    {
        timerCounter_Text_Parent.transform.DOKill();
        timerCounter_Text_Parent.transform.localScale = timerTextOriginalScale;
        timerCounter_Text_Parent.transform.localRotation = timerTextOriginalRotation;

        timerCounter_Text_Parent.transform.DOPunchScale(Vector3.one * timerPunchScale, timerPunchDuration);
        timerCounter_Text_Parent.transform.DOPunchRotation(new Vector3(0f, 0f, timerPunchAngle), timerPunchDuration);
    }

    /// <summary>
    /// Kritik süreye girildiğinde zamanlayıcının
    /// kritik bölge animasyonunu başlatan metot.
    /// </summary>
    public void StartTimerPulse()
    {
        timerPulseSequence?.Kill();

        timerCounter_Text_Parent.transform.DOKill();
        timerCounter_Text_Parent.transform.localScale = timerTextOriginalScale;
        timerCounter_Text_Parent.transform.localRotation = timerTextOriginalRotation;

        timerPulseSequence = DOTween.Sequence();

        timerPulseSequence.Append(timerCounter_Text_Parent.DOScale(timerTextOriginalScale * timerPulseScale, timerPulseGrowDuration).SetEase(timerPulseGrowEase));

        timerPulseSequence.Append(timerCounter_Text_Parent.DOScale(timerTextOriginalScale, timerPulseShrinkDuration).SetEase(timerPulseShrinkEase));

        timerPulseSequence.SetLoops(-1, LoopType.Restart);
    }

    /// <summary>
    /// Zamanlayıcının kritik bölge animasyonunu
    /// sonlandıran metot.
    /// </summary>
    public void StopTimerPulse()
    {
        timerPulseSequence?.Kill();
        timerPulseSequence = null;
    }

    /// <summary>
    /// Zamanlayıcı süresi sona erdiğinde,
    /// zaman aşımı animasyonunu uygulayan metot.
    /// </summary>
    public void PlayTimeoutFeedback()
    {
        timerCounter_Text_Parent.DOKill();
        timerCounter_Text_Parent.localScale = timerTextOriginalScale;
        timerCounter_Text_Parent.localRotation = timerTextOriginalRotation;

        timerCounter_Text_Parent.DOPunchScale(Vector3.one * timeoutPunchScale, timeoutDuration);
        timerCounter_Text_Parent.DOPunchRotation(new Vector3(0f, 0f, timeoutShakeAngle), timeoutDuration);
        timerCounter_Text.DOColor(timeoutColor, timeoutDuration * 0.3f);
    }

    /// <summary>
    /// Zamanlayıcı bileşenlerini sıfırlayan ve
    /// animasyonları sonlandıran metot.
    /// </summary>
    private void ResetTimerVisual()
    {
        timerCounter_Text_Parent.DOKill();
        timerCounter_Text.DOKill();
        timerCounter_Text_Parent.localScale = timerTextOriginalScale;
        timerCounter_Text_Parent.localRotation = timerTextOriginalRotation;
        timerCounter_Text.color = timerTextOriginalColor;
        UpdateTimerFill(0f);
    }

    public void ShowPerkSelectionScreen(List<Perk> perks, PerkConfiguration perkConfig)
    {
        if (perks == null || perks.Count == 0)
        {
            Debug.LogError("Invalid perk list.");
            return;
        }

        foreach (PerkCard perkCard in perkCardList)
        {
            perkCard.OnClicked -= OnPerkCardSelected;
        }

        perkCardPool.ReturnAllObjects();
        perkCardList.Clear();

        for (int i = 0; i < perks.Count; i++)
        {
            PerkCard perkCard = perkCardPool.GetObject();

            perkCard.SetPerk(perks[i], perkConfig);
            perkCard.SetInteractable(true);
            perkCard.OnClicked += OnPerkCardSelected;

            perkCardList.Add(perkCard);

            perkCard.PlayEntrance(i * perkCardStaggerDelay);
        }

        animatedPerkSelectionPanel.ShowPanel();
    }

    public void SetPerkCardsInteractable(bool interactable)
    {
        foreach (PerkCard card in perkCardList)
        {
            card.SetInteractable(interactable);
        }
    }

    public void SetPerkPanelToggleButtonsActive(bool showVisible, bool showButtons)
    {
        hidePerkPanel_Button.gameObject.SetActive(showVisible && showButtons);

        showPerkPanel_Button.gameObject.SetActive(!showVisible && showButtons);
    }

    public void HidePerkSelectionScreen()
    {
        foreach (PerkCard perkCard in perkCardList)
        {
            perkCard.OnClicked -= OnPerkCardSelected;
        }

        animatedPerkSelectionPanel.HidePanel(() => { perkCardPool.ReturnAllObjects(); perkCardList.Clear(); });
    }

    private void OnPerkCardSelected(PerkCard card)
    {
        if (card == null)
        { 
            Debug.LogError("Selected card is null.");

            return;
        }

        Perk perk = card.Perk;

        foreach (PerkCard c in perkCardList) c.SetInteractable(false);

        card.transform.SetParent(flightLayer, true);

        RectTransform targetRect = perk.Type == Perk.PerkType.Passive ? passivePerkDisplayParent : activePerkCardParent;

        Vector2 targetPosition = WorldToAnchoredPosition(flightLayer, targetRect.position);

        void OnArrived()
        {
            card.transform.SetParent(perkCard_Parent, false);
            perkCardPool.ReturnObject(card);
            PerkSelected?.Invoke(perk);
        }

        Vector2 centerPosition = WorldToAnchoredPosition(flightLayer, flightLayer.position);

        card.PlaySelectionFlight(centerPosition, targetPosition, () => animatedPerkSelectionPanel.HidePanel(), OnArrived);
    }

    private Vector2 WorldToAnchoredPosition(RectTransform target, Vector3 worldPosition)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)target.parent, screenPoint, canvasCamera, out Vector2 localPoint);
        return localPoint;
    }

    public void ShowActiveCardInventory(List<Perk> perks, PerkConfiguration perkConfig)
    {
        if (perks == null || perks.Count == 0)
        {
            HideActiveCardInventory();

            return;
        }

        foreach (ActivePerkCard card in activePerkCardList)
        {
            card.OnClicked -= OnActivePerkCardSelected;    
        }

        activePerkCardPool.ReturnAllObjects();
        activePerkCardList.Clear();

        foreach (Perk activePerk in perks)
        {
            if (activePerk.Type != Perk.PerkType.Active) continue;

            ActivePerkCard activePerkCard = activePerkCardPool.GetObject();

            activePerkCard.SetPerkCardElements(activePerk, perkConfig);

            activePerkCard.SetInteractable(activePerk.IsPerkActive);

            activePerkCard.SetHighlight(false);

            activePerkCard.OnClicked += OnActivePerkCardSelected;

            activePerkCardList.Add(activePerkCard);
        }

        LayoutActivePerkCards();

        activeCardInventoryPanel.ShowPanel();
    }

    public void HideActiveCardInventory()
    {
        foreach (ActivePerkCard card in activePerkCardList)
        {
            card.OnClicked -= OnActivePerkCardSelected;    
        }

        activePerkCardPool.ReturnAllObjects();
        activePerkCardList.Clear();

        activeCardInventoryPanel.HidePanel();
    }

    private void LayoutActivePerkCards()
    {
        int count = activePerkCardList.Count;

        if (count == 0) return;

        float availableWidth = activePerkCardParent.rect.width;

        float spacing = Mathf.Min(maxSpacing, availableWidth / count);
        float totalWidth = spacing * (count - 1);
        float startX = -totalWidth / 2;

        for (int i = 0; i < count; i++)
        {
            ActivePerkCard card = activePerkCardList[i];

            float t = count == 1 ? 0 : (i / (float)(count - 1)) * 2 - 1;
    
            float x = startX + i * spacing;
            float y = -Mathf.Abs(t) * arcDepth;
            float angle = -t * maxRotation;
            
            card.SetHandTransform(new Vector2(x, y), angle, i);
        }

    }

    public void UpdateActivePerkCards()
    {
        foreach (ActivePerkCard card in activePerkCardList) card.UpdateCooldownDisplay();
    }

    private void OnActivePerkCardSelected(ActivePerkCard card)
    {
        PerkUsed?.Invoke(card.Perk);
    }

    public void SetChoiceTargetingMode(bool active, Perk perk = null)
    {
        foreach (ActivePerkCard card in activePerkCardList)
        {
            if (perk != null && active && card.Perk == perk && card.Perk.IsPerkActive)
            {
                card.SetHighlight(true);
            }
            else
            {
                card.SetHighlight(false);
            }
        }
    }

    public void UpdatePassivePerkDisplay(List<Perk> perks, PerkConfiguration perkConfig)
    {
        foreach (PassivePerkDisplay display in passivePerkDisplayList)
        {
            display.OnHoverEnter -= ShowPassivePerkTooltip;
            display.OnHoverExit -= HidePassivePerkTooltip;
        }

        passivePerkDisplayPool.ReturnAllObjects();
        passivePerkDisplayList.Clear();

        if (perks == null || perks.Count == 0)
        {
            return;
        }

        this.perkConfig = perkConfig;

        foreach (Perk perk in perks)
        {
            if (perk.Type != Perk.PerkType.Passive) continue;

            PassivePerkDisplay passivePerkDisplay = passivePerkDisplayPool.GetObject();

            passivePerkDisplay.OnHoverEnter += ShowPassivePerkTooltip;
            passivePerkDisplay.OnHoverExit += HidePassivePerkTooltip;

            passivePerkDisplay.SetIcon(perk);

            passivePerkDisplayList.Add(passivePerkDisplay);
        }
    }

    public void ShowPassivePerkTooltip(PassivePerkDisplay display)
    {
        if (display.Perk == null)
        {
            Debug.LogError("Passive perk is null.");

            return;
        }

        tooltipName_Text.text = display.Perk.Name;

        FillEffectRows(display.Perk, passivePerkDisplayEffectRowPool, perkConfig);

        perkTooltip_Rect.position = display.transform.position;

        perkTooltip_CanvasGroup.alpha = 1f;
        perkTooltip_CanvasGroup.interactable = false;
        perkTooltip_CanvasGroup.blocksRaycasts = false;
    }

    public void HidePassivePerkTooltip()
    {
        perkTooltip_CanvasGroup.alpha = 0f;
        perkTooltip_CanvasGroup.interactable = false;
        perkTooltip_CanvasGroup.blocksRaycasts = false;
    }

    public void PlayPerkNotReadyFeedback(Perk perk)
    {
        foreach (ActivePerkCard card in activePerkCardList)
        {
            if (card.Perk == perk)
            {
                card.PlayNotReadyFeedback();

                break;
            }
        }
    }

    private void FillEffectRows(Perk perk, ObjectPool<EffectRow> pool, PerkConfiguration perkConfig)
    {
        pool.ReturnAllObjects();
        
        foreach (PerkEffectEntry effect in perk.Effects)
        {
            EffectRow row = pool.GetObject();

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

    public void RemoveChoiceButton(int index)
    {
        if (index < 0 || index >= activeChoiceButtons.Count) return;

        ChoiceButton button = activeChoiceButtons[index];

        button.OnClicked -= OnChoiceButtonClicked;
        activeChoiceButtons.RemoveAt(index);
        choiceButtonPool.ReturnObject(button);
    }

    public void SetChoiceFrost(int index, int hits)
    {
        if (index < 0 || index >= activeChoiceButtons.Count) return;
        activeChoiceButtons[index].SetFrost(hits);
    }

    public void PlayMascotCorrect()  => mascot.PlayCorrectReaction();
    public void PlayMascotWrong()    => mascot.PlayWrongReaction();
    public void ResetMascot()        => mascot.ResetToIdle();

    public void ShowMascotHint(string hint)
    {
        mascot.ShowHint(string.IsNullOrWhiteSpace(hint) ? noHintMessage : hint);
    }

    public void HideMascotHint() => mascot.HideHint();

    private void OnMascotHintRequested() => MascotHintRequested?.Invoke();

    private void OnMascotBubbleClosed() => MascotBubbleClosed?.Invoke();

    /// <summary>
    /// Oyun başlamadan önceki geri sayımı gösteren metot.
    /// </summary>
    /// <param name="onComplete">
    /// Geri sayım tamamlandığında çağırılacak referans.
    /// </param>
    public void PlayCountdown(Action onComplete)
    {
        StartCoroutine(CountdownRoutine(onComplete));
    }

    private IEnumerator CountdownRoutine(Action onComplete)
    {
        for (int i = countdownStart; i > 0; i--)
        {
            countdown_Text.text = i.ToString();

            countdown_Text.transform.DOKill();
            countdown_Text.transform.localScale = countdownTextOriginalScale;
            countdown_Text.transform.DOPunchScale(Vector3.one * countdownPunchScale, countdownStepDuration * 0.5f);

            yield return new WaitForSeconds(countdownStepDuration);
        }

        countdown_Text.transform.DOKill();
        countdown_Text.transform.localScale = countdownTextOriginalScale;

        countdownPanel.HidePanel(() => onComplete?.Invoke());
    }

    /// <summary>
    /// Geri sayım panelini boş olarak açan metot.
    /// Sayılar geri sayım başladığında yazılır.
    /// </summary>
    public void ShowCountdownPanel()
    {
        countdown_Text.text = string.Empty;

        countdownPanel.ShowPanel();
    }
}

/// <summary>
/// Skor verisinin seviye bilgilerini içeren veri sınıfı.
/// </summary>
[Serializable]
public class ScoreColorTier
{
    /// <summary>
    /// Alınabilecek yüksek skorlar için seviye eşiği verisi.
    /// </summary>
    public int colorThreshold;

    /// <summary>
    /// <see cref="colorThreshold"/> eşiğinin yazı rengi verisi.
    /// </summary>
    public Color color = Color.black;
}