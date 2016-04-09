using System;

namespace Assets {
    public struct Point2i {
        public int x;
        public int y;

        public Point2i(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Point2i DeltaTowards(Point2i dst) {
            return new Point2i(Math.Sign(dst.x - x), Math.Sign(dst.y - y));
        }

        public override string ToString() {
            return string.Format("({0}, {1})", x, y);
        }

        public static Point2i operator +(Point2i a, Point2i b) {
            return new Point2i(a.x + b.x, a.y + b.y);
        }

        public static Point2i operator -(Point2i a, Point2i b) {
            return new Point2i(a.x - b.x, a.y - b.y);
        }

        public static bool operator ==(Point2i a, Point2i b) {
            return a.x == b.x && a.y == b.y;
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            return this == (Point2i)obj;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static bool operator !=(Point2i a, Point2i b) {
            return !(a == b);
        }
    }
}