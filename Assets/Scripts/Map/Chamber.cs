using UnityEngine.Assertions;

namespace Assets.Map {
    class Chamber {
        public readonly int left;
        public readonly int bottom;
        public readonly int width;
        public readonly int height;

        public int right { get { return left + width; } }
        public int top { get { return bottom + height; } }

        public Point2i center {
            get { return new Point2i(left + width / 2, bottom + height / 2); }
        }

        public Chamber(int left, int bottom, int width, int height) {
            this.left = left;
            this.bottom = bottom;
            this.width = width;
            this.height = height;
        }

        public static Chamber Random(System.Random rng, int maxX, int maxY, int minSize, int maxSize) {
            Assert.IsTrue(minSize > 0);
            Assert.IsTrue(maxSize > 0);
            Assert.IsTrue(maxSize > minSize);

            int width = rng.Next(minSize, maxSize);
            int height = rng.Next(minSize, maxSize);

            return new Chamber(rng.Next(0, maxX - width),
                               rng.Next(0, maxY - height),
                               width,
                               height);
        }

        public bool CollidesWith(Chamber other, int minDist) {
            return !(right <= other.left - minDist
                     || left >= other.right + minDist
                     || top <= other.bottom - minDist
                     || bottom >= other.top + minDist);
        }

        public bool Equals(Chamber chamber) {
            return left == chamber.left && right == chamber.right && bottom == chamber.bottom && top == chamber.top;
        }

        public override string ToString() {
            return string.Format("{0}, {1}, {2}, {3}", left, bottom, right, top);
        }
    }
}