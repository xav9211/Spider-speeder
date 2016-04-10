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

            if (!chambers.Any(c => c.CollidesWith(newChamber))) {
                chambers.Add(newChamber);
            }
        }
        return chambers;
    }

    private IList<Corridor> GenerateCorridors(System.Random rng,
                                              IList<Chamber> chambers,
                                              int numExtraCorridors) {
        IList<Corridor> corridors = new List<Corridor>();
        for (int i = 1; i < chambers.Count; ++i) {
            corridors.Add(Corridor.JoiningChambers(rng, chambers.RandomFromSlice(rng, 0, i), chambers[i]));
        }
        for (int i = 0; i < numExtraCorridors; ++i) {
            corridors.Add(Corridor.JoiningChambers(rng, chambers.Random(rng), chambers.Random(rng)));
        }

        return corridors;
    }

    private Tile[,] Generate(System.Random rng, int width, int height) {
        Tile[,] tiles = Empty(width, height);
        ChamberGeneratorConfig chamberCfg = new ChamberGeneratorConfig() {
            mapSize = new Point2i(width, height),
            minChamberSize = 5,
            maxChamberSize = 10,
            numChambers = width * height / 150,
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

                while (pointer != c.intermediatePoints[i + 1]) {
                    tiles[pointer.x, pointer.y] = new Tile(Tile.Type.Corridor, false);
                    pointer += delta;
                }
                tiles[pointer.x, pointer.y] = new Tile(Tile.Type.Corridor, false);
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

    private Point2i mapSize;
    private Tile[,] tiles;
    private GameObject[] tileTypes;

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

        System.Random rng = new System.Random(42);
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