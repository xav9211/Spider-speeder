using System;
using System.Collections.Generic;

namespace Assets.Map {
    internal class Corridor {
        public readonly IList<Point2i> intermediatePoints;

        public int cost { get { return Math.Abs(intermediatePoints[0].x - intermediatePoints[2].x) + Math.Abs(intermediatePoints[0].y - intermediatePoints[2].y); } }

        public Corridor(IList<Point2i> intermediatePoints) {
            this.intermediatePoints = intermediatePoints;
        }

        public static Corridor JoiningChambers(System.Random rng, Chamber src, Chamber dst) {
            IList<Point2i> intermediatePoints = new List<Point2i>();
            intermediatePoints.Add(src.center);

            Point2i delta = dst.center - src.center;
            if (rng.Next(2) == 0) {
                delta.x = 0;
            } else {
                delta.y = 0;
            }

            intermediatePoints.Add(src.center + delta);
            intermediatePoints.Add(dst.center);

            return new Corridor(intermediatePoints);
        }

        public override bool Equals(Object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            Corridor corridor = (Corridor)obj;
            return (this.intermediatePoints[0] == corridor.intermediatePoints[0] && this.intermediatePoints[2] == corridor.intermediatePoints[2])
                || (this.intermediatePoints[0] == corridor.intermediatePoints[2] && this.intermediatePoints[2] == corridor.intermediatePoints[0]);
        }

        public override int GetHashCode() {
            throw new NotImplementedException();
        }

        public override string ToString() {
            return string.Format("{0}, {1}", intermediatePoints[0].ToString(), intermediatePoints[2].ToString());
        }
    }
}