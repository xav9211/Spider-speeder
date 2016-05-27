using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
    public class BuffStackDisplay : MonoBehaviour {
        public void Refresh(List<Buff> buffs) {
            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>()) {
                Destroy(renderer.gameObject);
            }

            RectTransform stackTransform = GetComponentInParent<RectTransform>();
            float localSize = stackTransform.rect.height;

            for (int idx = 0; idx < buffs.Count; ++idx) {
                Buff buff = buffs[idx];

                GameObject icon = new GameObject("BuffIcon", typeof(SpriteRenderer), typeof(RectTransform));
                SpriteRenderer renderer = icon.GetComponent<SpriteRenderer>();
                RectTransform transform = icon.GetComponent<RectTransform>();

                Texture2D texture = Resources.Load<Texture2D>(buff.icon);
                Rect textureRect = Rect.MinMaxRect(0.0f, 0.0f, texture.width, texture.height);
                renderer.sprite = Sprite.Create(texture, textureRect, Vector2.zero, texture.height / localSize);
                renderer.sortingLayerName = "UI";
                renderer.color = new Color(1.0f, 1.0f, 1.0f, 0.75f);

                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.zero;
                transform.pivot = Vector2.zero;
                transform.sizeDelta = new Vector2(localSize, localSize);
                transform.anchoredPosition = new Vector2(localSize * 1.1f * idx, 0.0f);
                transform.SetParent(gameObject.transform, false);
            }
        }
    }


}
