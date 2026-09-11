public enum QuestionDifficulty : byte
{
    easy = 0,
    medium = 1,
    hard = 2
}

/// <summary>
/// Web üzerinden elde edilecek soru verileri için hazırlanmış veri sınıfı.
/// Verilen URL'deki JSON dosyasının formatına uygun olarak tasarlandı.
/// </summary>
[System.Serializable]
public class Question_WebList
{
    public Question_Web[] questions;
}

/// <summary>
/// <see cref="Question_WebList"/> sınıfı tarafından kullanılan web verilerini
/// indirebilmek için JSON dosyasına uygun formatlanmış veri sınıfı.
/// </summary>
/// <remarks>
/// <see cref="category"/> verisi sorunun kategorisini,
/// <see cref="question"/> verisi soru metnini,
/// <see cref="choices"/> verisi sorunun barındırdığı seçenekleri,
/// <see cref="answer"/> verisi sorunun doğru seçeneğini
/// temsil eder.
/// </remarks>
[System.Serializable]
public class Question_Web
{
    public string   category;

    public string   question;

    public string[] choices;

    public string   answer;

    /// <value>
    /// Sorunun doğru cevabının <see cref="answer"/> verisine göre index karşılığını üretir.
    /// Boş veya hatalı <see cref="answer"/> değeri için <c>-1</c> üretilir.
    /// </value>
    public int CorrectAnswerIndex
    {
        get
        {
            if (string.IsNullOrEmpty(answer)) return -1;

            int result = answer.ToUpper()[0] - 'A';

            if(choices != null && result >= 0 && result < choices.Length)   return result;
            else                                                            return -1;
        }
    }
}

/// <summary>
/// Yerel dosyalar üzerinden elde edilecek soru verileri için hazırlanmış veri sınıfı.
/// JSON dosyasındaki verinin formatına uygun olarak tasarlandı.
/// </summary>
[System.Serializable]
public class Question_LocalWrapper
{
    public Question[] questions;
}

/// <summary>
/// Oynanışta sorulacak soruların verilerini tutmak için hazırlanmış veri sınıfı.
/// <see cref="choices"/> dizisinde bulunan seçenek metinlerinden
/// index'i <c>0</c> olan seçenek (<c>choices[0]</c>) doğru cevap kabul edilir.
/// </summary>
/// <remarks>
/// <see cref="error"/> verisi sorunun hatalı olup olmamasını,
/// <see cref="difficulty"/> verisi sorunun zorluk seviyesini,
/// <see cref="category"/> verisi sorunun kategorisini,
/// <see cref="question"/> verisi soru metnini,
/// <see cref="choices"/> verisi sorunun barındırdığı seçenekleri,
/// <see cref="image"/> sorunun (varsa) resminin bağlantı linkini
/// temsil eder.
/// </remarks>
[System.Serializable]
public class Question
{
    public bool                 error;

    public QuestionDifficulty   difficulty;

    public string               category;

    public string               question;

    public string[]             choices;

    public string               imageURL;

    public string               hint;
}
