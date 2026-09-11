using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using DG.Tweening;


#if UNITY_EDITOR
// Scene Asset alanı sadece editörde çalıştığından builde dahil edilmeyen kütüphane.
using UnityEditor;
#endif

/// <summary>
/// Sahne yükleme işlemlerini içeren sınıf.
/// </summary>
/// <remarks>
/// <see cref="targetSceneName"/> verisini <see cref="Scene"/> referansı üzerinden
/// hesaplayabilmek için <see cref="OnValidate"/> metodunda <see cref="Path.GetFileNameWithoutExtension"/> metodu kullanılır.
/// <see cref="OnValidate"/> metodu ve <see cref="UnityEditor"/> işlemleri builde dahil edilmez.
/// </remarks>
public class SceneLoader : MonoBehaviour
{
    [Tooltip("Scene Asset => GetAssetPath() yolu ile belirlenen sahne adı verisi. OnValidate() metodu ile otomatik ayarlanır kesinlikle elle ayarlanmamalıdır. Sahne değişikliği için targetScene sahne referansı verisi kullanılabilir. Bir değişiklik halinde targetSceneName verisinin değerinin doğruluğunu kontrol ediniz.")]
    [SerializeField] private string targetSceneName;

    #if UNITY_EDITOR
    [Tooltip("Geçilmek istenen sahnenin sahne referansı. Sahne adı bu verinin değerine göre otomatik ayarlanır.")]
    [SerializeField] private SceneAsset targetScene;
    
    /// <summary>
    /// <see cref="targetScene"/> referansı üzerinden
    /// <see cref="targetSceneName"/> verisinin
    /// otomatik olarak belirlenmesini sağlayan bölüm.
    /// </summary>
    void OnValidate()
    {
        // Sahne referansı boş değilse sahne dosyasının bulunduğu directory üzerinden sahne ismi her Unity Inspector güncellemesinde otomatik olarak ayarlanır.
        if (targetScene != null) targetSceneName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(targetScene));
    }
    #endif

    /// <summary>
    /// <see cref="targetSceneName"/> ismi ile eşleşen
    /// sahnenin yüklenmeye başlaması için
    /// <see cref="LoadScene"/> coroutine'ini çağıran metot.
    /// </summary>
    /// <remarks>
    /// Bir hata meydana gelmesi durumunda coroutine çağırılmaz ve
    /// konsola bir hata mesajı yazılır.
    /// </remarks>
    public void LoadTargetScene()
    {
        // Sahne adı kontrol ediliyor...
        if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            // Sahne adı geçersiz.
            Debug.LogError("Scene:" + targetSceneName + ", Build Settings does not contain this scene name or scene not assigned project.");
            return;
        }

        // Sahne yüklemesi başlatılıyor...
        StartCoroutine(LoadScene());
    }

    /// <summary>
    /// Yüklenmeye hazır olan sahnenin yüklemesini başlatan coroutine.
    /// </summary>
    IEnumerator LoadScene()
    {

        // Hazırsa yükleme ekranı gösteriliyor.
        if (LoadingScreenManager.Instance != null) yield return LoadingScreenManager.Instance.ShowLoadingScreen()?.WaitForCompletion();
        
        // Yükleme ekranı gösterildiğkten sonra sahne yüklemesi başlatılıyor...
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);

        // Sahnenin yüklemesi tamamlandığı anda sahne aktif hale gelmeyecek.
        // Bu bayrak true olarak kullanılırsa yükleme tamamlandığı anda
        // isDone true olur ve sahne, yükleme bittiği anda aktifleşir.
        // Bu bayrak yükleme barı dolmadan sahnenin aktifleşmesini önler.
        operation.allowSceneActivation = false;

        // Unity yüklenme oranının son %10'luk kısmını sahneyi aktive etmeye ayırdığı için
        // yüklenme oranının sadece %90'lık kısmı iletiliyor.
        while (operation.progress < 0.9f)
        {
            // Yüklenme oranı hazırsa sahne ekranına iletiliyor.
            if (LoadingScreenManager.Instance != null)
            {
                float loadingProgress = operation.progress / 0.9f * LoadingScreenManager.Instance.SceneLoadProgressWeight;

                LoadingScreenManager.Instance.SetLoadingProgress(loadingProgress);
            }

            yield return null;
        }

        // Yükleme tamamlandı floating point hassasiyeti oluşmaması adına yükleme barının sahne yükleme aşaması tamamen dolduruluyor.
        if (LoadingScreenManager.Instance != null) yield return LoadingScreenManager.Instance.SetLoadingProgress(LoadingScreenManager.Instance.SceneLoadProgressWeight)?.WaitForCompletion();

        // Sahne yüklendi ve hazır. Bu yüzden artık sahneyi aktive edebiliriz.
        // False olarak bıraksaydık isDone asla true olamazdı ve sonsuz döngü oluşurdu.
        operation.allowSceneActivation = true;

        // Sahne yüklemesi tamamlandı. Sahneye geçiş için kalan hazırlıklar tamamlanıyor...
        while (!operation.isDone) yield return null;
    }
}
