using PluginConfig.API;
using PluginConfig.API.Fields;
using UnityEngine;
using UnityEngine.UI;

namespace JadeLib.PluginConfigurator.Fields
{
    public class SpriteField : CustomConfigField
    {
        private Image image;

        private Sprite sprite;
        public Sprite Sprite
        {
            get => sprite;
            set
            {
                sprite = value;
                SetSprite(value);
            }
        }

        public SpriteField(ConfigPanel parentPanel, float height = 60f, Sprite sprite = null) : base(parentPanel)
        {
            this.sprite = sprite;
            fieldHeight = height;
        }

        public override void OnCreateUI(RectTransform fieldUI)
        {
            image = fieldUI.AddComponent<Image>();
            image.preserveAspect = true;
            SetSprite(sprite);
        }

        private void SetSprite(Sprite sprite)
        {
            if (!image) { return; }
            image.gameObject.SetActive(sprite);
            image.sprite = sprite;
        }
    }
}