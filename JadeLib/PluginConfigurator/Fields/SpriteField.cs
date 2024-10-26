using PluginConfig.API;
using PluginConfig.API.Fields;
using UnityEngine;
using UnityEngine.UI;

namespace JadeLib.PluginConfigurator
{
    public class SpriteField(ConfigPanel parentPanel, Sprite sprite = null) : CustomConfigField(parentPanel)
    {
        private Image currentUI;

        private Sprite sprite = sprite;
        public Sprite Sprite
        {
            get => sprite;
            set => SetSprite(sprite = value);
        }

        protected override void OnCreateUI(RectTransform fieldUI)
        {
            currentUI = fieldUI.gameObject.AddComponent<Image>();
            SetSprite(sprite);
        }

        private void SetSprite(Sprite sprite)
        {
            currentUI?.gameObject.SetActive(sprite);
            if (!currentUI || !sprite) { return; }

            (currentUI.transform as RectTransform).sizeDelta = sprite.rect.size.x > 600f ? sprite.rect.size * (600f / sprite.rect.size.x) : sprite.rect.size;
            currentUI.sprite = sprite;
        }
    }
}
