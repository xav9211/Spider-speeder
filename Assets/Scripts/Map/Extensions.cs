using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Map {
    static class ListExtensions {
        public static T Random<T>(this IList<T> list, System.Random rng) {
            Assert.IsTrue(list.Count > 0);
            return list[rng.Next(list.Count)];
        }

        public static T RandomFromSlice<T>(this IList<T> list, System.Random rng, int min, int max) {
            Assert.IsTrue(list.Count > 0);
            Assert.IsTrue(0 <= min && min <= list.Count);
            Assert.IsTrue(0 <= max && max <= list.Count);
            Assert.IsTrue(min < max);
            return list[min + rng.Next(max - min)];
        }
    }

    static class IntExtensions {
        public static bool IsStrictlyBetween(this int val, int min, int max) {
            return min < val && val < max;
        }
    }

    static class TileExtensions {
        public static Sprite ToSprite(this Tile tile, Point2i pos) {
            return Sprite.Create(
                tile.isOccupied ? Texture2D.blackTexture : Texture2D.whiteTexture,
                Rect.MinMaxRect((float)pos.x - 0.5f, (float)pos.y - 0.5f, (float)pos.x + 0.5f, (float)pos.y + 0.5f),
                Vector2.zero);
        }
    }

    static class PointExtensions {
        public static Vector2 ToVector2(this Point2i p) {
            return new Vector2(p.x, p.y);
        }
    }
}