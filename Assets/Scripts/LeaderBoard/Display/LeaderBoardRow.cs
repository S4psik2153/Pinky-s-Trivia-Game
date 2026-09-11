using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// <see cref="LeaderBoard_Data"/> kaydını
/// <see cref="LeaderBoardDisplay.displayRow_Prefab"/> objesinde bulunan
/// ilgili textbox'larda gösteren sınıf bileşeni.
/// </summary>
public class LeaderBoardRow : MonoBehaviour
{
    [SerializeField]    private CanvasGroup             row_CanvasGroup;
    [SerializeField]    private TMPro.TextMeshProUGUI   rank_Text;
    [SerializeField]    private TMPro.TextMeshProUGUI   nickname_Text;
    [SerializeField]    private Image                   sourceIcon_Image;
    [SerializeField]    private Image                   row_Image;
    [SerializeField]    private Sprite                  localIcon_Sprite;
    [SerializeField]    private Sprite                  webIcon_Sprite;
    [SerializeField]    private TMPro.TextMeshProUGUI   time_Text;
    [SerializeField]    private TMPro.TextMeshProUGUI   score_Text;

    [SerializeField]    private Color                   localDataColor = Color.cyan;
    [SerializeField]    private Color                   webDataColor = Color.red;

    [SerializeField]    private float                   hiddenScale = 0.7f;
    [SerializeField]    private float                   entranceDuration = 0.4f;
    [SerializeField]    private Ease                    entranceEase = Ease.OutBack;
                        private Vector3                 originalLocalScale;

    void Awake()
    {
        originalLocalScale = transform.localScale;
    }

    void OnDisable()
    {
        transform.DOKill();
        row_CanvasGroup.DOKill();

        transform.localScale = originalLocalScale;
        row_CanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// <see cref="LeaderBoardManager"/> sınıfından gelen
    /// <see cref="LeaderBoard_Data"/> verisi değerlerini
    /// ilgili textbox'a yerleştiren metot.
    /// </summary>
    /// <param name="data">
    /// Gösterilmek üzere textboxlara yerleştirilecek
    /// <see cref="LeaderBoard_Data"/> verisi.
    /// </param>
    public void SetDisplay(LeaderBoard_Data data)
    {
        if (data == null)
        {
            Debug.LogError("Leader Board data is null.");
            return;
        }

        row_Image.color = data.isLocal ? localDataColor : webDataColor;

        rank_Text.text      = data.rank.ToString();
        nickname_Text.text  = data.nickname;
        time_Text.text      = data.time < 0f ? "-" : $"{Mathf.FloorToInt(data.time / 60f):00}:{Mathf.FloorToInt(data.time % 60f):00}";
        score_Text.text     = data.score.ToString();

        sourceIcon_Image.sprite = data.isLocal ? localIcon_Sprite : webIcon_Sprite;
    }

    /// <summary>
    /// <see cref="LeaderBoardRow"/> objelerinin
    /// açılış animasyonunu oynatan metot.
    /// </summary>
    /// <param name="delay">
    /// Animasyonun gecikme miktarı.
    /// Her satır için ayrı bir gecikme verilirse
    /// sırayla açılma görüntüsü elde edilebilir.
    /// </param>
    public void PlayEntrance(float delay)
    {
        transform.DOKill();
        row_CanvasGroup.DOKill();

        transform.localScale = originalLocalScale * hiddenScale;
        row_CanvasGroup.alpha = 0f;

        transform.DOScale(originalLocalScale, entranceDuration).SetDelay(delay).SetEase(entranceEase);
        row_CanvasGroup.DOFade(1f, entranceDuration).SetDelay(delay);
    }
}
