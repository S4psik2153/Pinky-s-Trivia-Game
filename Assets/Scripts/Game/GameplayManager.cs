using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oynanış akışını yöneten sınıf.
/// </summary>
public class GameplayManager : MonoBehaviour
{
    private PerkManager perkManager;
    public PerkManager Perks => perkManager;
    [SerializeField] private PerkEffectPool perkEffectPool;
    public PerkEffectPool EffectPool => perkEffectPool;

    [SerializeField] private PerkPool perkPool;

    [Header("References")]
    [Tooltip("Oyun sonunda ana menüye dönmeyi sağlayan sahne bilgilerini tutan veri. Inspectordan ayarlanmalıdır.")]
    [SerializeField] private SceneLoader sceneLoader;

    [SerializeField] private GameplayUI gameplayUI;
    /// <summary>
    /// UI işlemlerini yöneten <see cref="GameplayUI"/> sınıfının referansı.
    /// Oyun durumlarının UI üzerinde yaptıkları değişikliklerin uygulandığı alan.
    /// </summary>
    public GameplayUI UI => gameplayUI;

    [SerializeField] private GameConfiguration gameConfiguration;
    /// <summary>
    /// Yapılandırma işlemlerini yöneten <see cref="GameConfiguration"/> sınıfının referansı.
    /// Oyun durumlarının yapılandırma verilerine erişmesi gereken durumlarda kullanılan alan.
    /// </summary>
    public GameConfiguration Config => gameConfiguration;

    [SerializeField] private PerkConfiguration perkConfig;
    public PerkConfiguration PerkConfig => perkConfig;

    [SerializeField] private LocalLeaderBoardStorage storage;
    /// <summary>
    /// Yerel Liderlik Tabloları işlemlerini uygulayan <see cref="LocalLeaderBoardStorage"/> sınıfının referansı.
    /// Oyun sonucunun yerel Liderlik Tablosu dosyasına kaydedilmesi gereken durumlarda kullanılan alan.
    /// </summary>
    public LocalLeaderBoardStorage Storage => storage;

    [SerializeField] private QuestionManager questionManager;

    /// <summary>
    /// Oyunun hangi oyun durumunda olduğunu belirten veri.
    /// </summary>
    private GameState currentGameState;

    /// <summary>
    /// Soru listesinin tutulduğu ve kullanıldığı veri.
    /// </summary>
    private List<Question> questions;
    /// <summary>
    /// <see cref="currentQuestionIndex"/> verisine göre o index'teki soruyu getiren alan.
    /// </summary>
    public Question CurrentQuestion
    {
        get
        {
            if (questions == null || currentQuestionIndex < 0 || currentQuestionIndex >= questions.Count)
            {
                Debug.LogError("Invalid question.");
                return null;
            }

            return questions[currentQuestionIndex];
        }
    }
    public int TotalQuestionCount => questions?.Count ?? 0;

    /// <summary>
    /// Oyunun hangi soruda olduğunu index olarak tutan veri.
    /// </summary>
    private int currentQuestionIndex;
    /// <summary>
    /// Kaçıncı soruda olduğumuz bilgisini
    /// <see cref="currentQuestionIndex"/>
    /// değeri üzerinden ileten alan.
    /// </summary>
    public int CurrentQuestionNumber => currentQuestionIndex + 1;

    /// <summary>
    /// Oyuncunun anlık skorunu tutan veri.
    /// </summary>
    private int score;

    /// <summary>
    /// Oyun durumlarının <see cref="score"/> değerine
    /// ihtiyacı olduğu durumlarda oyuncu skorunu ileten alan.
    /// </summary>
    public int Score => score;

    /// <summary>
    /// <see cref="score"/> değerine bir değer eklemeyi sağlayan metot.
    /// </summary>
    /// <param name="amount">
    /// <see cref="score"/> değerine eklenecek değer.
    /// </param>
    public void AddScore(int amount)
    {
        score += amount;
    }

    /// <summary>
    /// Soru durumlarında geçen toplam süreyi tutan veri.
    /// </summary>
    private float elapsedTime;

    /// <summary>
    /// Oyun durumlarının <see cref="elapsedTime"/> değerine
    /// ihtiyacı olduğu durumlarda geçen süreyi ileten alan.
    /// </summary>
    public float ElapsedTime => elapsedTime;

    /// <summary>
    /// <see cref="elapsedTime"/> değerini arttırmayı sağlayan metot.
    /// </summary>
    /// <param name="amount">
    /// <see cref="elapsedTime"/> değerine eklenecek değer.
    /// </param>
    public void IncreaseElapsedTime(float amount)
    {
        elapsedTime += Mathf.Clamp(amount, 0f, Config.questionDuration);
    }

    private float timerSpeed = 1f;
    public float TimerSpeed { get => timerSpeed; set => timerSpeed = value; }

    private float scoreMultiplier = 1f;
    public float ScoreMultiplier { get => scoreMultiplier; set => scoreMultiplier = value; }

    /// <summary>
    /// <see cref="currentQuestionIndex"/> değerine göre
    /// <see cref="questions"/> listesinde daha fazla soru olup olmadığını
    /// <see cref="bool"/> türünden ileten alan.
    /// </summary>
    /// <remarks>
    /// Bu alan <see cref="currentQuestionIndex"/> değerine göre
    /// son soruda olup olunmadığını hesaplar.
    /// Eğer <see cref="currentQuestionIndex"/> son sorunun index'i ile
    /// aynı değere sahipse <see langword="false"/>
    /// farklı bir değere sahipse <see langword="true"/> değerini verir.
    /// </remarks>
    public bool HasMoreQuestions => currentQuestionIndex < questions.Count - 1;

    public int MarkedChoiceIndex { get; set; } = -1;
    public float MarkedChoiceMultiplier { get; set; } = 1f;

    public float PerkMultiplier { get; set; } = 1f;

    public float TemporaryPerkMultiplier { get; set; } = 1f;

    public event Action ChoiceEliminationRequested;
    public void RequestChoiceElimination() => ChoiceEliminationRequested?.Invoke();

    private readonly List<int> frostHitCount = new();

    public List<int> FrozenChoices(int frostHit = 0)
    {   
        if (frostHitCount.Count > UI.ChoiceCount) return frostHitCount;
        
        if (frostHit > 0) frostHitCount.Add(frostHit);

        return frostHitCount;
    }

    private List<Question> questionPool;
    private int poolIndex;

    public int ExtraChoiceCount { get; set; }
    public int RemovedChoiceCount { get; set; }

    void Start()
    {
        ResetGameProgress();

        perkManager = new PerkManager(this, perkPool);

        // Oyun başladığı anda ilk durum olan LoadingState() oyun durumu başlatılır.
        ChangeGameState(new LoadingState(this));
    }

    void Update()
    {
        // Durumların tik kısımları (varsa) uygulanır.
        currentGameState?.Tick();
    }

    private void ResetGameProgress()
    {
        score = 0;
        elapsedTime = 0;
    }

    /// <summary>
    /// Oyun durumlarının sonraki soruya geçmesini sağlayan metot.
    /// </summary>
    /// <remarks>
    /// <see cref="currentQuestionIndex"/> değerine göre
    /// sonraki soruya geçilir veya oyun bitirilir ve
    /// ilgili oyun durumları ayarlanır.
    /// </remarks>
    public void MoveToNextQuestion()
    {
        if (HasMoreQuestions)
        {
            // Daha fazla soru varsa:
            // Sonraki soruya geç.
            currentQuestionIndex++;
            
            ChangeGameState(new QuestionState(this));
        }
        else
        {
            // Daha fazla soru yoksa:
            // Oyunu bitir.
            ChangeGameState(new FinishGameState(this));
        }

    }

    /// <summary>
    /// Oyun durumlarının yönetildiği metot.
    /// </summary>
    /// <remarks>
    /// Önce eski bir oyun durumu bulunuyorsa eski durumunun
    /// temizliğinin yapılması için o oyun durumunun <see cref="GameState.ExitState"/> metodu çağırılır.
    /// Sonra yeni geçilecek olan oyun durumunun
    /// hazırlığının yapılması için o oyun durumunun <see cref="GameState.EnterState"/> metodu çağırılır.
    /// </remarks>
    /// <param name="newState">
    /// Geçilecek yeni oyun durumunu belirten oyun durumu sınıfı referansı.
    /// </param>
    public void ChangeGameState(GameState newState)
    {
        if (newState == null)
        {
            Debug.LogError("State is Null.");
            return;
        }

        currentGameState?.ExitState();
        currentGameState = newState;
        currentGameState?.EnterState();
    }

    /// <summary>
    /// Soruların indirilmesini başlatan metot.
    /// </summary>
    /// <param name="callback">
    /// İndirilen <see cref="Question"/> verisini ileten callback.
    /// </param>
    public void LoadQuestions(Action<List<Question>> callback)
    {
        questionManager.Start_LoadingQuestions(callback);
    }

    /// <summary>
    /// Verilen soruları <see cref="questions"/> verisine
    /// korumalı şekilde aktaran metot.
    /// </summary>
    /// <remarks>
    /// Karıştırılımış listenin ilk 
    /// <see cref="GameConfiguration.questionCount"/> verisi
    /// kadar soru oyuna aktarılır.
    /// </remarks>
    /// <param name="questionPool">
    /// Aktarılacak <see cref="Question"/> verisi.
    /// </param>
    public void SetQuestions(List<Question> questionPool)
    {
        if (questionPool == null || questionPool.Count == 0)
        {
            Debug.LogError("Question List is empty or Invalid index.");
            return;
        }

        this.questionPool = questionPool;
        poolIndex = Mathf.Min(Config.questionCount, questionPool.Count);

        currentQuestionIndex = 0;

        List<Question> questionsForAsk = new();

        // Sorulacak soru kadar soru havuzdan çekiliyor.
        for (int i = 0; i < Mathf.Min(Config.questionCount,questionPool.Count); i++)
        {
            questionsForAsk.Add(questionPool[i]);
        }

        questions = questionsForAsk;
    }

    /// <summary>
    /// Çağırıldığında ana menü sahnesini yükleyen metot.
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (LoadingScreenManager.Instance != null) LoadingScreenManager.Instance.HideErrorScreen();

        sceneLoader.LoadTargetScene();
    }

    public bool AddQuestion()
    {
        if (questionPool == null || poolIndex >= questionPool.Count) return false;

        questions.Add(questionPool[poolIndex]);
        poolIndex++;
        return true;
    }

    public bool RemoveQuestion()
    {
        if (questions == null) return false;

        if (questions.Count <= currentQuestionIndex + 1) return false;

        questions.RemoveAt(questions.Count - 1);
        return true;
    }

    public string GetRandomWrongChoiceFromPool()
    {
        if (questionPool == null || questionPool.Count < 2) return null;

        Question current = CurrentQuestion;

        int rng = UnityEngine.Random.Range(0, questionPool.Count);
        if (questionPool[rng] == current) rng = (rng + 1) % questionPool.Count;

        Question source = questionPool[rng];

        if (source == null || source.choices == null || source.choices.Length < 2) return null;

        int choiceIndex = UnityEngine.Random.Range(1, source.choices.Length);
        return source.choices[choiceIndex];
    }
}
