using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.Map;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class Map: MonoBehaviour {
    private Tile[,] Empty(int width, int height) {
        Tile[,] tiles = new Tile[width, height];
        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {
                tiles[x, y] = new Tile(Tile.Type.Wall, true);
            }
        }
        return tiles;
    }


    struct ChamberGeneratorConfig {
        public Point2i mapSize;
        public int numChambers;
        public int minChamberSize;
        public int maxChamberSize;
        public int maxIterations;
    }

    private IList<Chamber> GenerateChambers(System.Random rng,
                                            ChamberGeneratorConfig cfg) {
        IList<Chamber> chambers = new List<Chamber>();
        for (int i = 0; i < cfg.maxIterations && chambers.Count < cfg.numChambers; ++i) {
            Chamber newChamber = Chamber.Random(rng, cfg.mapSize.x, cfg.mapSize.y, cfg.minChamberSize, cfg.maxChamberSize);

            if (!chambers.Any(c => c.CollidesWith(newChamber, 5))) {
                chambers.Add(newChamber);
            }
        }
        return chambers;
    }

    private bool CorridorColideWithChamber(Corridor corridor, Chamber src, Chamber dst, IList<Chamber> chambers)
    {
        Point2i start = corridor.intermediatePoints[0];
        Point2i corner = corridor.intermediatePoints[1];
        Point2i end = corridor.intermediatePoints[2];
        foreach (var chamber in chambers)
        {
            if (!chamber.Equals(src) && !chamber.Equals(dst)) {
                if (Point2i.InRange(corner.x, chamber.left, chamber.right)) {
                    Point2i checkPoint = corner.y == start.y ? end : start;
                    if (Point2i.InRange(chamber.bottom - 2, checkPoint.y, corner.y) || Point2i.InRange(chamber.top + 2, checkPoint.y, corner.y)) {
                        return true;
                    }
                }

                if (Point2i.InRange(corner.y, chamber.bottom, chamber.top)) {
                    Point2i checkPoint = corner.x == start.x ? end : start;
                    if (Point2i.InRange(chamber.left - 2, checkPoint.x, corner.x) || Point2i.InRange(chamber.right + 2, checkPoint.x, corner.x)) {
                        return true;
                    }
                }
            }
        }
        return false;
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

    private IList<Corridor> CorridorsByKruskal(System.Random rng, IList<Chamber> chambers) {
        List<Tree> forest = new List<Tree>();

        List<Corridor> corridors = new List<Corridor>();
        for (int i = 0; i < chambers.Count; i++) {
            for (int j = i + 1; j < chambers.Count; j++) {
                if (!chambers[j].Equals(chambers[i])) {
                    corridors.Add(Corridor.JoiningChambers(rng, chambers[i], chambers[j]));
                }
            }
            Tree t = new Tree();
            t.verticles.Add(chambers[i].center);
            forest.Add(t);
        }
        corridors.Sort((x,y) => x.cost.CompareTo(y.cost));

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

    private IList<Corridor> GenerateCorridors(System.Random rng,
                                              IList<Chamber> chambers,
                                              int numExtraCorridors) {
        IList<Corridor> corridors = CorridorsByKruskal(rng, chambers);

        return corridors;
    }

    private Tile[,] Generate(System.Random rng, int width, int height) {
        Tile[,] tiles = Empty(width, height);
        ChamberGeneratorConfig chamberCfg = new ChamberGeneratorConfig() {
            mapSize = new Point2i(width, height),
            minChamberSize = 10,
            maxChamberSize = 20,
            numChambers = width * height / 500,
            maxIterations = 10000
        };
        IList<Chamber> chambers = GenerateChambers(rng, chamberCfg);
        IList<Corridor> corridors = GenerateCorridors(rng, chambers, chambers.Count);

        foreach (Corridor c in corridors) {
            for (int i = 0; i < c.intermediatePoints.Count - 1; ++i) {
                Point2i src = c.intermediatePoints[i];
                Point2i dst = c.intermediatePoints[i + 1];

                Point2i pointer = src;
                Point2i delta = pointer.DeltaTowards(dst);
                Assert.IsTrue((delta.x == 0) || (delta.y == 0));

                while (pointer != c.intermediatePoints[i + 1] + delta) {
                    addTiles(tiles, pointer, delta);
                    pointer += delta;
                }
                addTiles(tiles, pointer, delta);
            }
        }

        foreach (Chamber c in chambers) {
            for (int y = c.bottom; y < c.top; ++y) {
                for (int x = c.left; x < c.right; ++x) {
                    tiles[x, y] = new Tile(Tile.Type.Chamber, false);
                }
            }
        }

        return tiles;
    }

    private void addTiles(Tile[,] tiles, Point2i pointer, Point2i delta) {
        for (int j = -1; j <= 1; j++) {
            if (delta.x == 0)
                tiles[pointer.x + j, pointer.y] = new Tile(Tile.Type.Corridor, false);
            else
                tiles[pointer.x, pointer.y + j] = new Tile(Tile.Type.Corridor, false);
        }
    }

    private Point2i mapSize;
    private Tile[,] tiles;
    private GameObject[] tileTypes;
    private System.Random rng;

    // Returns a random tile that's not a wall
    public Point2i GetSpiderStartPos() {
        System.Random rng = new System.Random(42);
        Point2i pos;

        do {
            pos.x = rng.Next(mapSize.x);
            pos.y = rng.Next(mapSize.y);
        } while (tiles[pos.x, pos.y].isOccupied);

        return pos;
    }

    // Use this for initialization
    void Start() {
        tileTypes = new GameObject[Enum.GetNames(typeof(Tile.Type)).Length];
        for (int i = 0; i < tileTypes.Length; ++i) {
            tileTypes[i] = GameObject.Find("/Map/Tiles/" + Enum.GetNames(typeof(Tile.Type))[i]);
        }

        rng = new System.Random(40);
        mapSize = new Point2i(100, 100);

        tiles = Generate(rng, mapSize.x, mapSize.y);

        for (int y = 0; y < mapSize.y; ++y) {
            for (int x = 0; x < mapSize.x; ++x) {
                Tile tile = tiles[x, y];
                GameObject.Instantiate(tileTypes[(int)tile.type],
                                       new Vector3(x, y, 0.0f),
                                       Quaternion.identity);
                if(tileTypes[(int)tile.type].gameObject.name == Enum.GetName(typeof(Tile.Type),Tile.Type.Chamber) && rng.Next(100) < 2)
                {

                    GameObject.Instantiate(GameObject.Find("Enemy"),
                                           new Vector3(x, y, 0.0f),
                                           Quaternion.identity);
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {
    }
}