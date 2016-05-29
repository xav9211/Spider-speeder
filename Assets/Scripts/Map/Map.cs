using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Map {
    public class Map: MonoBehaviour {
        private Chamber startChamber;
		private Chamber exitChamber;
		private Point2i exitPos;

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
            startChamber = new Chamber(0, 0, cfg.maxChamberSize + 1, cfg.maxChamberSize + 1);

            for (int i = 0; i < cfg.maxIterations && chambers.Count < cfg.numChambers; ++i) {
                Chamber newChamber = Chamber.Random(rng, cfg.mapSize.x, cfg.mapSize.y, cfg.minChamberSize, cfg.maxChamberSize);

                if (!chambers.Any(c => c.CollidesWith(newChamber, 5)) && newChamber.IsNotBorder(this.mapSize)) {
                    chambers.Add(newChamber);

                    if (newChamber.height + newChamber.width < startChamber.height + startChamber.width) {
                        startChamber = newChamber;
                    }
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
                    corridors.Add(Corridor.JoiningChambers(rng, chambers[i], chambers[j]));
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

        private void addRandomCorridors(System.Random rng, IList<Chamber> chambers, IList<Corridor> corridors, int maxRandRange) {
            int iter = 0;
            int chamberNr = 0;
            while (iter < corridors.Count / 5) {
                if (addedCorridor(rng, chambers, corridors, maxRandRange, chamberNr))
                    iter++;
                chamberNr++;
            }
        }

        private bool addedCorridor(System.Random rng, IList<Chamber> chambers, IList<Corridor> corridors, int maxRandRange, int firstChammber) {
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

        private bool lessThanMaxRange(Chamber ch1, Chamber ch2, int maxRange) {
            return Math.Abs(ch1.center.x - ch2.center.x) < maxRange && Math.Abs(ch1.center.y - ch2.center.y) < maxRange;
        }

        private IList<Corridor> GenerateCorridors(System.Random rng,
                                                  IList<Chamber> chambers,
                                                  int numExtraCorridors,
                                                  int maxRandRange) {
            IList<Corridor> corridors = CorridorsByKruskal(rng, chambers);
            addRandomCorridors(rng, chambers, corridors, maxRandRange);

            return corridors;
        }

        private struct Layout {
            public Point2i mapSize;
            public Tile[,] tiles;
            public IList<Chamber> chambers;
            public IList<Corridor> corridors;
        }

        private Layout Generate(System.Random rng, int width, int height) {
            Tile[,] tiles = Empty(width, height);
            ChamberGeneratorConfig chamberCfg = new ChamberGeneratorConfig() {
                mapSize = new Point2i(width, height),
                minChamberSize = 10,
                maxChamberSize = 20,
                numChambers = width * height / 500,
                maxIterations = 10000
            };
            IList<Chamber> chambers = GenerateChambers(rng, chamberCfg);
            IList<Corridor> corridors = GenerateCorridors(rng, chambers, chambers.Count, 2*chamberCfg.maxChamberSize);

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

            List<Point2i> chamberTiles = new List<Point2i>();
            foreach (Chamber c in chambers) {
                for (int y = c.bottom; y < c.top; ++y) {
                    for (int x = c.left; x < c.right; ++x) {
                        tiles[x, y] = new Tile(Tile.Type.Chamber, false);
                        chamberTiles.Add(new Point2i(x, y));
                    }
                }
            }
				
            do {
                exitPos = chamberTiles.Random(rng);
            } while (startChamber.Contains(exitPos));

			exitChamber = chambers.Where (c => c.Contains (exitPos)).First ();

            return new Layout() {
                mapSize = mapSize,
                tiles = tiles,
                chambers = chambers,
                corridors = corridors
            };
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
        private Layout layout;
		public Dictionary<Tile.Type, List<GameObject>> tilesets;
        private System.Random rng;

        private int level = -1;
        public int Level {
            get { return level; }
            private set {
                level = value;
                GameObject.Find("/Canvas/LevelText").GetComponent<Text>().text = "Level: " + level;
            }
        }

        private int levelSeed = -1;
        public int LevelSeed {
            get { return levelSeed; }
            private set {
                levelSeed = value;
                GameObject.Find("/Canvas/LevelText/SeedText").GetComponent<Text>().text = "Seed: " + levelSeed;
            }
        }

        public PlayerControl Player { get; private set; }

        // Returns a random tile that's not a wall
        public Point2i GetSpiderStartPos() {
            Point2i pos;

            do {
                pos.x = rng.Next(startChamber.left, startChamber.right);
                pos.y = rng.Next(startChamber.bottom, startChamber.top);
            } while (layout.tiles[pos.x, pos.y].isOccupied);

            return pos;
        }

        private static Dictionary<Tile.Type, List<GameObject>> LoadTilesets() {
            Dictionary<Tile.Type, List<GameObject>> tilesets = new Dictionary<Tile.Type, List<GameObject>>();

            foreach (Tile.Type tilesetType in (Tile.Type[])Enum.GetValues(typeof(Tile.Type))) {
                DirectoryInfo tilesetDir = new DirectoryInfo("Assets/Resources/Tiles/" + tilesetType);
                List<GameObject> tileset = new List<GameObject>();

                foreach (FileInfo tileImage in tilesetDir.GetFiles("*.*")) {
                    if (!tileImage.Name.EndsWith(".meta")) {
                        string gameObjectResourcePath = "Tiles/" + tilesetType;
                        string imgResourcePath = "Tiles/" + tilesetType + "/" + Path.GetFileNameWithoutExtension(tileImage.Name);

                        GameObject obj = Instantiate(Resources.Load<GameObject>(gameObjectResourcePath));
                        obj.name = tilesetType.ToString();
                        obj.hideFlags = HideFlags.HideInHierarchy;
                        obj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(imgResourcePath);

                        tileset.Add(obj);
                    }
                }

                tilesets.Add(tilesetType, tileset);
            }
            return tilesets;
        }

        private void ClearClones() {
            foreach (Transform t in transform) {
                Destroy(t.gameObject);
            }
        }

        private void SpawnTiles() {
            GameObject tilesObj = new GameObject("Tiles");
            tilesObj.transform.parent = transform;

            for (int y = 0; y < mapSize.y; ++y) {
                for (int x = 0; x < mapSize.x; ++x) {
                    Tile tile = layout.tiles[x, y];
                    GameObject tileObj = (GameObject)Instantiate(tilesets[tile.type].Random(rng),
                                                                 new Vector3(x, y, 0.0f),
                                                                 Quaternion.identity);
                    tileObj.transform.parent = tilesObj.transform;
                }
            }
        }

        private void SpawnEnemies() {
            GameObject enemiesObj = new GameObject("Enemies");
            enemiesObj.transform.parent = transform;

			GameObject bossObj = (GameObject)Instantiate(Resources.Load<Object>("Boss"),
                                                         new Vector3(exitPos.x, exitPos.y , 0.0f),
                                                         Quaternion.identity);
			var boss = (BossAI)bossObj.GetComponent ("BossAI");
			boss.initialize (level, rng);
            bossObj.transform.parent = enemiesObj.transform;

            for (int y = 0; y < mapSize.y; ++y) {
                for (int x = 0; x < mapSize.x; ++x) {
                    Tile tile = layout.tiles[x, y];

                    if (tile.type == Tile.Type.Chamber && rng.Next(100) < 2 + level && 
						!startChamber.Contains(new Point2i(x, y)) &&
						!exitChamber.Contains(new Point2i(x, y)))
                    {
						var creature = (GameObject)Instantiate(Resources.Load<Object>("Enemy"),
															   new Vector3(x, y, 0.0f),
															   Quaternion.identity);
                        creature.transform.parent = enemiesObj.transform;

						var ai = (EnemyAI)creature.GetComponent("EnemyAI");
						ai.createMonster (level, rng);
                    }
                }
            }
        }

        private static bool TileTypeInRange(Tile[,] tiles,
                                            Point2i mapSize,
                                            Tile.Type type,
                                            Point2i centerTile,
                                            int range) {
            Assert.IsTrue(range >= 0);

            int xBegin = Math.Max(0, centerTile.x - range);
            int xEnd = Math.Min(centerTile.x + range + 1, mapSize.x);
            int yBegin = Math.Max(0, centerTile.y - range);
            int yEnd = Math.Min(centerTile.y + range + 1, mapSize.y);

            for (int x = xBegin; x < xEnd; ++x) {
                for (int y = yBegin; y < yEnd; ++y) {
                    if (tiles[x, y].type == type) {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SpawnCornerWebs(GameObject parentObject) {
            const double WEB_IN_CORNER_CHANCE = 0.2f;
            const float MIN_WEB_SIZE = 0.5f;
            const float MAX_WEB_SIZE = 1.5f;

            foreach (Chamber chamber in layout.chambers) {
                Point2i[] corners = {
                    new Point2i(chamber.left, chamber.top),
                    new Point2i(chamber.left, chamber.bottom),
                    new Point2i(chamber.right, chamber.bottom),
                    new Point2i(chamber.right, chamber.top),
                };

                for (int i = 0; i < corners.Length; ++i) {
                    if (!TileTypeInRange(layout.tiles, layout.mapSize, Tile.Type.Corridor, corners[i], 2)
                        // don't spawn webs in corners next to corridors to avoid hanging ones
                        && rng.NextDouble() < WEB_IN_CORNER_CHANCE) {
                        GameObject web = (GameObject) Instantiate(Resources.Load<Object>("Decoration"),
                                                                  new Vector3(corners[i].x - 0.5f,
                                                                              corners[i].y - 0.5f,
                                                                              0.0f),
                                                                  Quaternion.identity);
                        web.transform.parent = parentObject.transform;
                        web.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f*i);

                        float scale = rng.NextFromRange(MIN_WEB_SIZE, MAX_WEB_SIZE);
                        web.transform.localScale = new Vector3(scale, scale, 1.0f);

                        Sprite sprite = Resources.Load<Sprite>("Decorations/WebCorner/spider_web_corner_0");
                        SpriteRenderer renderer = web.GetComponent<SpriteRenderer>();
                        renderer.sprite = sprite;
                    }
                }
            }
        }

        private void SpawnDecorations(Point2i spiderStartPos) {
            GameObject decorationsObj = new GameObject("Decorations");
            decorationsObj.transform.parent = transform;
            
            for (int y = 0; y < mapSize.y; ++y) {
                for (int x = 0; x < mapSize.x; ++x) {
                    Tile tile = layout.tiles[x, y];
                    // TODO: place lights more intelligently
                    if (tile.type == Tile.Type.Chamber && rng.Next(100) < 1) {
                        GameObject fireObj = (GameObject)Instantiate(Resources.Load<Object>("Fire"),
                                                                     new Vector3(x, y, 0.0f),
                                                                     Quaternion.identity);
                        fireObj.transform.parent = decorationsObj.transform;
                    }
                }
            }

            // TODO: this might place the light inside a wall
            GameObject fireObjBesidePlayer = (GameObject)Instantiate(Resources.Load<Object>("Fire"),
                                                                     new Vector3(spiderStartPos.x + 2, spiderStartPos.y, 0.0f),
                                                                     Quaternion.identity);
            fireObjBesidePlayer.transform.parent = decorationsObj.transform;

            SpawnCornerWebs(decorationsObj);
        }

        internal void Regenerate(int level,
                                 int? rngSeed = null) {
            Level = level;
            LevelSeed = rngSeed ?? rng.Next();
            rng = new System.Random(LevelSeed);

            ClearClones();

            mapSize = new Point2i(110, 110);
            layout = Generate(rng, mapSize.x, mapSize.y);

            Point2i spiderStartPos = GetSpiderStartPos();

            SpawnTiles();
            SpawnEnemies();
            SpawnDecorations(spiderStartPos);

            if (Player == null) {
                GameObject playerObj = (GameObject) GameObject.Instantiate(Resources.Load("SpiderBody"));
                Player = playerObj.GetComponent<PlayerControl>();
                GameStatistics.Reset();
            }

            Player.StopMovement();
            Player.transform.position = new Vector3(spiderStartPos.x, spiderStartPos.y);
        }

        // Use this for initialization
        void Start() {
            tilesets = LoadTilesets();
            mapSize = new Point2i(100, 100);

            int level = 1;
            int rngSeed = 0;
            Regenerate(level, rngSeed);

            AudioUtils.BackgroundMusic.Play();
        }

        // Update is called once per frame
        void Update() {
            if (Player == null
                    && (Input.GetKeyDown(KeyCode.Joystick1Button0)
                        || Input.GetKeyDown(KeyCode.Joystick2Button0))) {
                int level = 1;
                Regenerate(level);
            }
        }
    }
}