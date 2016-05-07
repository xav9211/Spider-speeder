using System;
using System.Collections.Generic;

namespace Assets.Map {
    internal struct Corridor {
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
    }
}