using NeonDrift.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonDrift.UI
{
    public sealed class JuicyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Image image;
        private AudioManager audioManager;
        private Color normal;
        private Color hover;
        private Vector3 targetScale = Vector3.one;

        public void Configure(Image targetImage, AudioManager audio, Color normalColor, Color hoverColor)
        {
            image = targetImage;
            audioManager = audio;
            normal = normalColor;
            hover = hoverColor;
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 16f);
            if (image != null)
                image.color = Color.Lerp(image.color, targetScale.x > 1f ? hover : normal, Time.unscaledDeltaTime * 10f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = Vector3.one * 1.05f;
            audioManager.PlayButton();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = Vector3.one;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            targetScale = Vector3.one * 0.96f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            targetScale = Vector3.one * 1.04f;
        }
    }
}
