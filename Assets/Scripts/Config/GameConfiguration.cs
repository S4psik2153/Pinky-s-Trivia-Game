using UnityEngine;

/// <summary>
/// Oyun yapılandırması verilerinin tutulduğu <see cref="ScriptableObject"/> sınıfı.
/// Oyun verilerinin asset olarak değiştirilebilmesi için sınıf <see cref="ScriptableObject"/> olarak oluşturuldu.
/// Proje penceresinde "Trivia/Game Config" yolundan üretilebilir.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Trivia/Game Config")]
public class GameConfiguration : ScriptableObject
{
    /// <summary>
    /// Oyuncunun <see cref="Question.choices"/> seçenekleri arasından
    /// doğru cevap seçeneğini seçmesi durumunda
    /// <see cref="GameplayManager.Score"/> verisine eklenen ham değer.
    /// </summary>
    public int      correctScore        = 10;
    /// <summary>
    /// Oyuncunun <see cref="Question.choices"/> seçenekleri arasından
    /// doğru cevap seçeneği dışındaki bir seçeneğini seçmesi durumunda
    /// <see cref="GameplayManager.Score"/> verisine eklenen ham değer.
    /// </summary>
    public int      wrongScore          = -5;
    /// <summary>
    /// Oyuncunun <see cref="questionDuration"/> süresi içerisinde
    /// herhangi bir <see cref="Question.choices"/> seçeneği seçememesi durumunda
    /// <see cref="GameplayManager.Score"/> verisine eklenen ham değer.
    /// </summary>
    public int      timeoutScore        = -3;
    /// <summary>
    /// Oyuncunun bir soruyu cevaplayabilmesi için ayrılmış saniye türünden süre.
    /// </summary>
    public float    questionDuration    = 20;

    /// <summary>
    /// Oyuncuyu heyecanlandırmak için zamanlayıcı sayacının
    /// ondalık değerler göstermeye başlayacağı eşik değeri.
    /// </summary>
    public float    recklessTime        = 5f;

    /// <summary>
    /// Oyunun soru aşamasında sorulacak soru sayısını belirler.
    /// Sorulacak sorular soru havuzundan rastgele seçilir.
    /// Sorulacak soru sayısı havuzdan fazlaysa bu değer sınırlanır.
    /// </summary>
    public int      questionCount       = 10;

    public int      baseChoiceCount     = 4;
}
