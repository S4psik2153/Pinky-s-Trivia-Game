using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Garbage Collector'ın devreye girmesini azaltmak ve
/// oyunun performansının artmasını sağlamak için
/// obje havuzu oluşturmayı sağlayan yardımcı sınıf.
/// </summary>
/// <typeparam name="T">
/// Havuza eklenecek obje tipi.
/// Component üyeleri ile işlem yapabilmemizi sağlayan
/// generic tip parametresi.
/// </typeparam>
public class ObjectPool<T> where T : Component
{
    readonly T prefab;
    readonly Transform parent;

    /// <summary>
    /// Obje havuzunda birikecek objelerin saklanacağı yığın yapısı.
    /// </summary>
    readonly Stack<T> objectPool;
    /// <summary>
    /// Obje havuzunda sadece aktif olarak tutulan objelerin listesi.
    /// </summary>
    readonly List<T> activeObjects;

    /// <summary>
    /// Obje havuzu için gerekli referansları atayan constructor.
    /// Tercihe bağlı olarak önceden oluşacak obje sayısını belirleyerek
    /// prewarm yapılabilir.
    /// </summary>
    /// <param name="prefab">
    /// Kopyaları oluşturulacak obje örneği.
    /// </param>
    /// <param name="parent">
    /// Oluşacak objelerin bağlı olacağı parent transform'u.
    /// </param>
    /// <param name="initalPoolSize">
    /// Prewarm için başlangıçta oluşturulacak obje sayısı.
    /// Varsayılan değeri: <c>0</c>.
    /// </param>
    public ObjectPool(T prefab, Transform parent, int initalPoolSize = 0)
    {
        this.prefab = prefab;
        this.parent = parent;

        objectPool = new();
        activeObjects = new();

        //Prewarm: Objeler hazırlık için belirlenen miktarda baştan üretiliyor.
        for (int i = 0; i < initalPoolSize; i++)
        {
            T item = Object.Instantiate(prefab, parent, false);

            item.gameObject.SetActive(false);

            objectPool.Push(item);
        }
    }

    /// <summary>
    /// Obje havuzundan obje getiren,
    /// havuz boşsa yeni obje oluşturan metot.
    /// Alınmış obje <see cref="objectPool"/> havuzundan çıkarılır ve
    /// <see cref="activeObjects"/> listesine eklenir.
    /// </summary>
    /// <returns>
    /// Havuzdan çıkarılan kullanıma hazır obje.
    /// </returns>
    public T GetObject()
    {
        T item;

        if (objectPool.Count != 0)
        {
            item = objectPool.Pop();
        }
        else
        {
            item = Object.Instantiate(prefab, parent, false);
        }

        activeObjects.Add(item);

        item.gameObject.SetActive(true);

        // Yığın yapısı bir sıralama vaadetmediğinden
        // verilen objeleri hiyerarşinin sonuna taşıyarak,
        // objelerin hiyararşi sırasına göre geri alınmasını sağlar.
        item.transform.SetAsLastSibling();

        return item;
    }

    /// <summary>
    /// Daha önce alınmış objeleri
    /// havuza geri iade eden metot.
    /// </summary>
    /// <param name="item">
    /// Obje havuzuna geri iade edilmek
    /// istenen obje.
    /// </param>
    public void ReturnObject(T item)
    {
        if (item == null) return;

        item.gameObject.SetActive(false);
        
        objectPool.Push(item);
        activeObjects.Remove(item);
    }

    /// <summary>
    /// Daha önce alınmış tüm objeleri
    /// havuza geri iade eden metot.
    /// </summary>
    public void ReturnAllObjects()
    {
        if (activeObjects == null) return;

        foreach (T item in activeObjects)
        {
            if (item == null) continue;

            item.gameObject.SetActive(false);

            objectPool.Push(item);
        }
        activeObjects.Clear();
    }
}
