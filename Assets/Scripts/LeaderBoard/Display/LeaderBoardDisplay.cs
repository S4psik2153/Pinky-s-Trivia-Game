using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// <see cref="LeaderBoardManager"/> sınıfında hazırlanan
/// <see cref="LeaderBoard_Data"/> listesindeki verilerin
/// ekrana yansıtılmasını sağlayan sınıf.
/// </summary>
public class LeaderBoardDisplay : MonoBehaviour
{
    private LeaderBoardManager leaderBoardManager;

    [Tooltip("Liderlik tablosunda bir sayfada kaç adet satır bulunacağını belirleyen veri.")]
    [SerializeField] private int leaderBoard_DisplayRowCount = 10;

    /// <summary>
    /// Satır objelerinin tekrar tekrar
    /// oluşturulup yok edilmesini önlemek
    /// amacıyla kullanılan obje havuzu.
    /// </summary>
    /// <remarks>
    /// Satırlar ilk kullanımda ihtiyaç
    /// miktarı kadar üretilir ve
    /// tekrar eden işlerde aynı objeler
    /// yeniden kullanılır.
    /// </remarks>
    private ObjectPool<LeaderBoardRow> rowPool;

    [Header("Leader Board UI Elements")]
    [SerializeField] private AnimatedPanel      animatedLeaderBoardPanel;

    [Header("Leader Board Panel Buttons")]
    [SerializeField] private Button             closeLeaderBoard_Button;
    [SerializeField] private Button             nextPage_Button;
    [SerializeField] private Button             previousPage_Button;

    [Header("Page Number Display")]
    [SerializeField] private TextMeshProUGUI    pageNumber_Text;

    [Header("Leader Board Scroll Rect Reference")]
    [SerializeField] private ScrollRect         page_ScrollRect;

    [Header("Leader Board Rows")]
    [SerializeField] private RectTransform      leaderBoard_DisplayRowParent;
    [SerializeField] private LeaderBoardRow     displayRow_Prefab;
    [SerializeField] private float rowStaggerDelay = 0.04f;

    [Header("Local Leader Board UI Elements")]
    [SerializeField] private Button             deleteLocalLeaderBoard_Button;
    [SerializeField] private TextMeshProUGUI    deleteLocalLeaderBoard_ButtonText;

    [Header("Delete Local Leader Board")]
    [SerializeField] private CanvasGroup        deletionWarning_CanvasGroup;
    [SerializeField] private TextMeshProUGUI    deleteAreYouSure_Text;
    [SerializeField] private Button             deleteAreYouSure_Yes_Button;
    [SerializeField] private Button             deleteAreYouSure_No_Button;
    [SerializeField] private Button             deletionComplited_Button;

    [SerializeField] private string             buttonTextDeleteData    = "Delete Local Data";
    [SerializeField] private string             buttonTextDeletionFail  = "Local Leader Board file has no entries!";
    [SerializeField] private string             textDeletionWarning     = "Are you sure you want to delete all local Leader Board data? Deleted datas are not reversable!";
    [SerializeField] private string             textDeletionSuccess     = "All local Leader Board data deleted successfully.";
    [SerializeField] private string             textDeletionFailure     = "Error occurred while deleting. Please try again.";

    [SerializeField] private Color              default_DeleteTextColor = Color.black;
    [SerializeField] private Color              error_DeleteTextColor   = Color.red;
    [SerializeField] private Color              success_DeleteTextColor = Color.green;

    private int currentPage = 0;

    void Start()
    {
        leaderBoardManager = LeaderBoardManager.Instance;

        if(leaderBoardManager == null)
        {
            Debug.LogError("LeaderBoardManager instance not found.");
            return;
        }

        closeLeaderBoard_Button.onClick.AddListener(HideLeaderBoard);
        nextPage_Button.onClick.AddListener(GoToNextPage);
        previousPage_Button.onClick.AddListener(GoToPreviousPage);

        deleteLocalLeaderBoard_Button.onClick.AddListener(ShowDeletionWarningPanel);
        deleteAreYouSure_Yes_Button.onClick.AddListener(DeleteRequested);
        deleteAreYouSure_No_Button.onClick.AddListener(HideDeletionWarningPanel);
        deletionComplited_Button.onClick.AddListener(HideDeletionWarningPanel);

        rowPool = new ObjectPool<LeaderBoardRow>(displayRow_Prefab, leaderBoard_DisplayRowParent, leaderBoard_DisplayRowCount);
    }

    void OnDestroy()
    {
        // Obje yok edildiğinde buton dinleyicileri de temizlenir.
        closeLeaderBoard_Button.onClick.RemoveListener(HideLeaderBoard);
        nextPage_Button.onClick.RemoveListener(GoToNextPage);
        previousPage_Button.onClick.RemoveListener(GoToPreviousPage);

        deleteLocalLeaderBoard_Button.onClick.RemoveListener(ShowDeletionWarningPanel);
        deleteAreYouSure_Yes_Button.onClick.RemoveListener(DeleteRequested);
        deleteAreYouSure_No_Button.onClick.RemoveListener(HideDeletionWarningPanel);
        deletionComplited_Button.onClick.RemoveListener(HideDeletionWarningPanel);
    }

    /// <summary>
    /// Liderlik Tablosu panelini açan metot.
    /// </summary>
    public void ShowLeaderBoard()
    {
        currentPage = 0;

        animatedLeaderBoardPanel.ShowPanel();

        closeLeaderBoard_Button.interactable = false;
        nextPage_Button.interactable = false;
        previousPage_Button.interactable = false;

        leaderBoardManager.Start_LoadingLeaderBoardPages(OnEntriesLoaded);

        deleteLocalLeaderBoard_ButtonText.text = buttonTextDeleteData;
    }

    /// <summary>
    /// Liderlik Tablosu panelini kapatan metot.
    /// </summary>
    private void HideLeaderBoard()
    {
        animatedLeaderBoardPanel.HidePanel();
    }

    /// <summary>
    /// <see cref="LeaderBoardManager"/> sınıfında elde edilen
    /// <see cref="LeaderBoard_Data"/> sayfa verisini
    /// ekrana yansıtmak için hazırlayan metot.
    /// Verinin yüklendiğinden emin olunabilmesi için
    /// <see cref="LeaderBoardManager"/> sınıfında
    /// callback olarak kullanılıyor.
    /// </summary>
    /// <param name="success">
    /// Yüklenen sayfanın başarı durumunu ileten veri.
    /// </param>
    private void OnEntriesLoaded(bool success)
    {
        closeLeaderBoard_Button.interactable = true;

        if (success)
        {
            GoToPage(0);
        }
        else
        {
            Debug.LogError("Error has occurred while Leader Board loading.");
        }
    }

    /// <summary>
    /// Verilen sayfa numarasına gidilmesini sağlayan metot.
    /// </summary>
    /// <param name="pageIndex">
    /// Gösterilmek istenen sayfa numarası.
    /// </param>
    private void GoToPage(int pageIndex)
    {
        List<LeaderBoard_Data> board = leaderBoardManager.GetPage(pageIndex, leaderBoard_DisplayRowCount);

        if (board.Count == 0)
        {
            return;
        }

        currentPage = pageIndex;

        DisplayPage(board);
        
        UpdatePageButtons();

        pageNumber_Text.text = (pageIndex + 1).ToString();
    }

    /// <summary>
    /// Sonraki sayfaya gidilmesini sağlayan metot.
    /// Sayfa butonları için kullanılıyor.
    /// </summary>
    private void GoToNextPage()
    {
        GoToPage(currentPage + 1);
    }

    /// <summary>
    /// Önceki sayfaya gidilmesini sağlayan metot.
    /// Sayfa butonları için kullanılıyor.
    /// </summary>
    private void GoToPreviousPage()
    {
        GoToPage(currentPage - 1);
    }

    /// <summary>
    /// Sayfa butonlarının geçerli olup olmadığına göre
    /// sayfa butonlarını güncelleyen metot.
    /// </summary>
    /// <remarks>
    /// <see cref="currentPage"/> <c>0</c>'dan büyükse geriye gidilecek,
    /// <see cref="LeaderBoardManager.IsLastPage"/> metodu sonucuna göre
    /// <see cref="currentPage"/> son sayfa değilse
    /// ileriye gidilecek bir sayfa bulunuyor demektir.
    /// </remarks>
    private void UpdatePageButtons()
    {
        previousPage_Button.interactable = currentPage > 0;
        nextPage_Button.interactable = !leaderBoardManager.IsLastPage(currentPage, leaderBoard_DisplayRowCount);
    } 

    /// <summary>
    /// Obje havuzunu kullanarak yüklenen sayfa verilerini
    /// <see cref="displayRow_Prefab"/> örneğindeki satırlara
    /// yükleyip ekrana yansıtan metot.
    /// </summary>
    /// <param name="board">
    /// <see cref="GoToPage"/> metodunda elde edilen sıralama listesi.
    /// Veriler obje havuzunda yer alan satır objelerine yazılır.
    /// </param>
    private void DisplayPage(List<LeaderBoard_Data> board)
    {
        rowPool.ReturnAllObjects();

        for (int i = 0; i < board.Count; i++)
        {
            LeaderBoardRow row = rowPool.GetObject();
            row.SetDisplay(board[i]);
            row.PlayEntrance(i * rowStaggerDelay);
        }

        ResetPageScroll();
    }

    /// <summary>
    /// Sayfalar arası geçişlerde önceki sayfada
    /// <see cref="page_ScrollRect"/> ile oluşan
    /// kaymanın yeni sayfaya aktarılmasını önleyen metot.
    /// </summary>
    private void ResetPageScroll()
    {
        // Unity Layout sistemi boyutları frame sonunda hesapladığından,
        // layout güncellenmeden atama yapılırsa yükseklik eski yükseklik üzerinden hesaplanır.
        // ForceRebuildLayoutImmediate() metodu bu hesaplamanın anında yapılmasını sağlar.
        // Böylelikle sayfalar değiştiğinde yükseklik anında hesaplanmış olur ve kayma sıfırlanabilir.
        LayoutRebuilder.ForceRebuildLayoutImmediate(leaderBoard_DisplayRowParent);
        page_ScrollRect.verticalNormalizedPosition = 1;
    }

    /// <summary>
    /// Yerel Liderlik Tablosu dosyasını silme işlemi isteğinde
    /// istemsiz basmalara karşı uyarı panelini açan metot.
    /// </summary>
    private void ShowDeletionWarningPanel()
    {
        if (leaderBoardManager.IsLocalLeaderBoardHasEntries())
        {
            deleteAreYouSure_Text.color = default_DeleteTextColor;
            deleteAreYouSure_Text.text  = textDeletionWarning;

            deletionWarning_CanvasGroup.alpha = 1f;
            deletionWarning_CanvasGroup.interactable = true;
            deletionWarning_CanvasGroup.blocksRaycasts = true;

            deletionComplited_Button.interactable = false;
            deletionComplited_Button.gameObject.SetActive(false);

            deleteAreYouSure_Yes_Button.interactable = true;
            deleteAreYouSure_Yes_Button.gameObject.SetActive(true);
            deleteAreYouSure_No_Button.interactable = true;
            deleteAreYouSure_No_Button.gameObject.SetActive(true);
        }
        else
        {
            deleteLocalLeaderBoard_ButtonText.text = buttonTextDeletionFail;
        }
    }

    /// <summary>
    /// Silme işlemi uyarı panelini kapatan metot.
    /// </summary>
    private void HideDeletionWarningPanel()
    {
        deletionWarning_CanvasGroup.alpha = 0f;
        deletionWarning_CanvasGroup.interactable = false;
        deletionWarning_CanvasGroup.blocksRaycasts = false;        
    }

    /// <summary>
    /// Silme işlemi onaylandığında yerel Liderlik Tablosu dosyasını silmek için
    /// <see cref="LeaderBoardManager.DeleteLocalLeaderBoard"/> metodunu çağıran metot.
    /// Uyarı paneli metnini günceller ve butonların basılabilirliğini ayarlar.
    /// </summary>
    private void DeleteRequested()
    {
        if (leaderBoardManager.DeleteLocalLeaderBoard())
        {
            deleteAreYouSure_Text.color = success_DeleteTextColor;
            deleteAreYouSure_Text.text  = textDeletionSuccess;
        }
        else
        {
            deleteAreYouSure_Text.color = error_DeleteTextColor;
            deleteAreYouSure_Text.text  = textDeletionFailure;
        }

        deleteAreYouSure_Yes_Button.interactable = false;
        deleteAreYouSure_Yes_Button.gameObject.SetActive(false);
        deleteAreYouSure_No_Button.interactable = false;
        deleteAreYouSure_No_Button.gameObject.SetActive(false);

        deletionComplited_Button.interactable = true;
        deletionComplited_Button.gameObject.SetActive(true);

        nextPage_Button.interactable = false;
        previousPage_Button.interactable = false;

        currentPage = 0;
        leaderBoardManager.Start_LoadingLeaderBoardPages(OnEntriesLoaded);
    }
}
