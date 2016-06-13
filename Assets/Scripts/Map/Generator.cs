using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts.Map {
    class Layout {
        public Point2i MapSize { get; internal set; }
        public Tile[,] Tiles { get; internal set; }
        public IList<Chamber> Chambers { get; internal set; }
        public IList<Corridor> Corridors { get; internal set; }
        public Chamber StartChamber { get; internal set; }
        public Chamber ExitChamber { get; internal set; }

        public Point2i StartPos { get { return StartChamber.center; } }
        public Point2i ExitPos { get { return ExitChamber.center; } }
    }

    static class Generator {
        private static void PlaceCorridor(Tile[,] tiles,
                                          Point2i center,
                                          int width) {
            int left = width / 2;
            int right = width - left;

            for (int x = -left; x < right; ++x) {
                for (int y = -left; y < right; ++y) {
                    tiles[center.x + x, center.y + y] = new Tile(Tile.Type.Corridor, false);
                }
            }
        }

        private static Tile[,] GenerateTiles(int width,
                                             int height,
                                             IList<Chamber> chambers,
                                             IList<Corridor> corridors) {
            Tile[,] tiles = new Tile[width, height];
            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    tiles[x, y] = new Tile(Tile.Type.Wall, true);
                }
            }

            const int CORRIDOR_WIDTH = 3;
            foreach (Corridor c in corridors) {
                for (int i = 0; i < c.intermediatePoints.Count - 1; ++i) {
                    Point2i src = c.intermediatePoints[i];
                    Point2i dst = c.intermediatePoints[i + 1];

                    Point2i pointer = src;
                    Point2i delta = pointer.DeltaTowards(dst);
                    Assert.IsTrue((delta.x == 0) || (delta.y == 0));

                    while (pointer != c.intermediatePoints[i + 1]) {
                        PlaceCorridor(tiles, pointer, CORRIDOR_WIDTH);
                        pointer += delta;
                    }
                    PlaceCorridor(tiles, pointer, CORRIDOR_WIDTH);
                }
            }

            List<Point2i> chamberTiles = new List<Point2i>();
            foreach (Chamber c in chambers) {
                for (int y = c.bottom; y < c.top; ++y) {
                    for (int x = c.left; x < c.right; ++x) {
                        tiles[x, y] = new Tile(Tile.Type.Chamber, false);
                        chamberTiles.Add(new Point2i(x, y));
                    }
                }
            }

            return tiles;
        }

        static Chamber ChooseExitChamber(System.Random rng,
                                         IList<Chamber> chambers,
                                         Chamber startChamber) {
            while (true) {
                Chamber c = chambers.Random(rng);
                if (c != startChamber) {
                    return c;
                }
            }
        }

        struct ChamberGeneratorConfig {
            public Point2i mapSize;
            public int numChambers;
            public int minChamberSize;
            public int maxChamberSize;
            public int maxIterations;
        }

        private static IList<Chamber> GenerateChambers(System.Random rng,
                                                       ChamberGeneratorConfig cfg) {
            IList<Chamber> chambers = new List<Chamber>();

            for (int i = 0; i < cfg.maxIterations && chambers.Count < cfg.numChambers; ++i) {
                Chamber newChamber = Chamber.Random(rng, cfg.mapSize.x, cfg.mapSize.y, cfg.minChamberSize, cfg.maxChamberSize);

                if (!chambers.Any(c => c.CollidesWith(newChamber, 5)) && newChamber.IsNotBorder(cfg.mapSize)) {
                    chambers.Add(newChamber);
                }
            }
            return chambers;
        }

        private static Chamber ChooseStartChamber(IList<Chamber> chambers) {
            // select chamber with smallest area
            return chambers.Aggregate((c1, c2) => c1.area < c2.area ? c1 : c2);
        }

        class Tree {
            public List<Point2i> verticles = new List<Point2i>();
            public List<Corridor> corridors = new List<Corridor>();

            public static Tree Merge(Tree t1, Tree t2, Corridor corr) {
                Tree tree = new Tree();
                tree.verticles.AddRange(t1.verticles);
                tree.verticles.AddRange(t2.verticles);
                tree.corridors.AddRange(t1.corridors);
                tree.corridors.AddRange(t2.corridors);
                tree.corridors.Add(corr);
                return tree;
            }
        }

        private static IList<Corridor> CorridorsByKruskal(System.Random rng, IList<Chamber> chambers) {
            List<Tree> forest = new List<Tree>();

            List<Corridor> corridors = new List<Corridor>();
            for (int i = 0; i < chambers.Count; i++) {
                for (int j = i + 1; j < chambers.Count; j++) {
                    corridors.Add(Corridor.JoiningChambers(rng, chambers[i], chambers[j]));
                }
                Tree t = new Tree();
                t.verticles.Add(chambers[i].center);
                forest.Add(t);
            }
            corridors.Sort((x, y) => x.cost.CompareTo(y.cost));

            int count = 0;
            while (corridors.Count > 0) {
                Corridor corr = corridors[count];
                corridors.Remove(corr);

                Tree t1 = forest.Find(x => x.verticles.Contains(corr.intermediatePoints[0]));
                Tree t2 = forest.Find(x => x.verticles.Contains(corr.intermediatePoints[2]));

                if (t1 == t2)
                    continue;

                Tree tFinal = Tree.Merge(t1, t2, corr);
                forest.Remove(t1);
                forest.Remove(t2);
                forest.Add(tFinal);
            }

            return forest.First().corridors;
        }

        private static void addRandomCorridors(System.Random rng, IList<Chamber> chambers, IList<Corridor> corridors, int maxRandRange) {
            int iter = 0;
            int chamberNr = 0;
            while (iter < corridors.Count / 5) {
                if (addedCorridor(rng, chambers, corridors, maxRandRange, chamberNr))
                    iter++;
                chamberNr++;
            }
        }

        private static bool addedCorridor(System.Random rng, IList<Chamber> chambers, IList<Corridor> corridors, int maxRandRange, int firstChammber) {
            for (int i = firstChammber; i < chambers.Count; i++) {
                for (int j = i + 1; j < chambers.Count; j++) {
                    if (lessThanMaxRange(chambers[i], chambers[j], maxRandRange)) {
                        Corridor corr = Corridor.JoiningChambers(rng, chambers[i], chambers[j]);
                        if (!corridors.Contains(corr)) {
                            corridors.Add(corr);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool lessThanMaxRange(Chamber ch1, Chamber ch2, int maxRange) {
            return Math.Abs(ch1.center.x - ch2.center.x) < maxRange && Math.Abs(ch1.center.y - ch2.center.y) < maxRange;
        }

        private static IList<Corridor> GenerateCorridors(System.Random rng,
                                                         IList<Chamber> chambers,
                                                         int numExtraCorridors,
                                                         int maxRandRange) {
            IList<Corridor> corridors = CorridorsByKruskal(rng, chambers);
            addRandomCorridors(rng, chambers, corridors, maxRandRange);

            return corridors;
        }

        public static Layout Generate(System.Random rng,
                                      int width,
                                      int height) {
            ChamberGeneratorConfig chamberCfg = new ChamberGeneratorConfig() {
                mapSize = new Point2i(width, height),
                minChamberSize = 10,
                maxChamberSize = 20,
                numChambers = width * height / 500,
                maxIterations = 10000
            };

            IList<Chamber> chambers = GenerateChambers(rng, chamberCfg);
            IList<Corridor> corridors = GenerateCorridors(rng, chambers, chambers.Count, 2 * chamberCfg.maxChamberSize);
            Chamber startChamber = ChooseStartChamber(chambers);

            return new Layout() {
                MapSize = new Point2i(width, height),
                Chambers = chambers,
                Corridors = corridors,
                StartChamber = startChamber,
                ExitChamber = ChooseExitChamber(rng, chambers, startChamber),
                Tiles = GenerateTiles(width, height, chambers, corridors)
            };
        }
    }
}
