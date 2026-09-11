using UnityEngine;
using UnityEngine.UI;

public class AnimatedBackground : MonoBehaviour
{
    [SerializeField] private RawImage background_RawImage;
    [SerializeField] private Vector2 scrollSpeed = new(0f, 0.01f);

    void Update()
    {
        Rect uv = background_RawImage.uvRect;
        uv.position += scrollSpeed * Time.deltaTime;
        background_RawImage.uvRect = uv;
    }
}
