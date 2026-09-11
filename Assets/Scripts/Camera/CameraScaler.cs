using UnityEngine;

/// <summary>
/// Ekran ölçülerini sabit bir oranda tutan sınıf.
/// Doğru çalışabilmesi için tüm sahnelerdeki
/// oyun içeriğini yansıtan kamera objelerine eklenilmesi gerekir.
/// </summary>
public class CameraScaler : MonoBehaviour
{
    /// <summary>
    /// Kamera objesi referansı.
    /// </summary>
    [SerializeField] private Camera gameCamera;

    /// <summary>
    /// İstenen ekran oranı.
    /// </summary>
    [SerializeField] private float targetScreenRatio = 9f / 16f;

    /// <summary>
    /// Önceki framelerden gelen ekran oranı bilgisini taşıyan veri.
    /// İlk framede o framein ekran oranı verisinden farklı olması için
    /// <c>0</c> başlangıç değeri ile tanımlandı.
    /// </summary>
    private float previousScreenRatio = 0f;

    void Update()
    {
        float currentScreenRatio = Screen.width / (float)Screen.height;

        if (!Mathf.Approximately(currentScreenRatio, previousScreenRatio))
        {
            previousScreenRatio = currentScreenRatio;

            SetCameraRatio(currentScreenRatio);
        }
    }

    /// <summary>
    /// Ekran ölçülerinde bir değişiklik olması halinde
    /// ekran oranını ayarlayan metot.
    /// </summary>
    /// <param name="screenRatio">
    /// Değişen ekran oranı verisi.
    /// </param>
    private void SetCameraRatio(float screenRatio)
    {
        if (screenRatio > targetScreenRatio)
        {
            float new_Width = targetScreenRatio / screenRatio;
            float x_Offset = (1 - new_Width) / 2f;

            gameCamera.rect = new Rect(x_Offset, 0f, new_Width, 1f);
        }
        else if (screenRatio < targetScreenRatio)
        {
            float new_Height = screenRatio / targetScreenRatio;
            float y_Offset = (1 - new_Height) / 2f;

            gameCamera.rect = new Rect(0f, y_Offset, 1f, new_Height);
        }
        else
        {
            gameCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }
    }
}