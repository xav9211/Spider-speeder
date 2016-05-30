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
            return layout.StartPos;
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
            tilesObj.transform.SetParent(transform);

            for (int y = 0; y < layout.MapSize.y; ++y) {
                for (int x = 0; x < layout.MapSize.x; ++x) {
                    Tile tile = layout.Tiles[x, y];
                    GameObject tileObj = (GameObject)Instantiate(tilesets[tile.type].Random(rng),
                                                                 new Vector3(x, y, 0.0f),
                                                                 Quaternion.identity);
                    tileObj.transform.SetParent(tilesObj.transform);
                }
            }
        }

        private void SpawnEnemies() {
            GameObject enemiesObj = new GameObject("Enemies");
            enemiesObj.transform.SetParent(transform);

			GameObject bossObj = (GameObject)Instantiate(Resources.Load<Object>("Boss"),
                                                         new Vector3(layout.ExitPos.x, layout.ExitPos.y , 0.0f),
                                                         Quaternion.identity);
			var boss = (BossAI)bossObj.GetComponent ("BossAI");
			boss.initialize (level, rng);
            bossObj.transform.SetParent(enemiesObj.transform);

            for (int y = 0; y < layout.MapSize.y; ++y) {
                for (int x = 0; x < layout.MapSize.x; ++x) {
                    Tile tile = layout.Tiles[x, y];

                    if (tile.type == Tile.Type.Chamber && rng.Next(100) < 2 + level && 
						!layout.StartChamber.Contains(new Point2i(x, y)) &&
						!layout.ExitChamber.Contains(new Point2i(x, y)))
                    {
						var creature = (GameObject)Instantiate(Resources.Load<Object>("Enemy"),
															   new Vector3(x, y, 0.0f),
															   Quaternion.identity);
                        creature.transform.SetParent(enemiesObj.transform);

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

            foreach (Chamber chamber in layout.Chambers) {
                Point2i[] corners = {
                    new Point2i(chamber.left, chamber.top),
                    new Point2i(chamber.left, chamber.bottom),
                    new Point2i(chamber.right, chamber.bottom),
                    new Point2i(chamber.right, chamber.top),
                };

                for (int i = 0; i < corners.Length; ++i) {
                    if (!TileTypeInRange(layout.Tiles, layout.MapSize, Tile.Type.Corridor, corners[i], 2)
                        // don't spawn webs in corners next to corridors to avoid hanging ones
                        && rng.NextDouble() < WEB_IN_CORNER_CHANCE) {
                        GameObject web = (GameObject) Instantiate(Resources.Load<Object>("Decoration"),
                                                                  new Vector3(corners[i].x - 0.5f,
                                                                              corners[i].y - 0.5f,
                                                                              0.0f),
                                                                  Quaternion.identity);
                        web.transform.SetParent(parentObject.transform);
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
            decorationsObj.transform.SetParent(transform);
            
            for (int y = 0; y < layout.MapSize.y; ++y) {
                for (int x = 0; x < layout.MapSize.x; ++x) {
                    Tile tile = layout.Tiles[x, y];
                    // TODO: place lights more intelligently
                    if (tile.type == Tile.Type.Chamber && rng.Next(100) < 1) {
                        GameObject fireObj = (GameObject)Instantiate(Resources.Load<Object>("Fire"),
                                                                     new Vector3(x, y, 0.0f),
                                                                     Quaternion.identity);
                        fireObj.transform.SetParent(decorationsObj.transform);
                    }
                }
            }

            // TODO: this might place the light inside a wall
            GameObject fireObjBesidePlayer = (GameObject)Instantiate(Resources.Load<Object>("Fire"),
                                                                     new Vector3(spiderStartPos.x + 2, spiderStartPos.y, 0.0f),
                                                                     Quaternion.identity);
            fireObjBesidePlayer.transform.SetParent(decorationsObj.transform);

            SpawnCornerWebs(decorationsObj);
        }

        internal void Regenerate(int level,
                                 int? rngSeed = null) {
            Level = level;
            LevelSeed = rngSeed ?? rng.Next();
            rng = new System.Random(LevelSeed);

            ClearClones();

            Point2i mapSize = new Point2i(110, 110);
            layout = Generator.Generate(rng, mapSize.x, mapSize.y);

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

            int level = 1;
            int rngSeed = 0;
            Regenerate(level, rngSeed);

            AudioUtils.BackgroundMusic.Play();
        }

        // Update is called once per frame
        void Update() {
            if (Player == null
                    && (Input.GetKeyDown(KeyCode.Joystick1Button3)
                        || Input.GetKeyDown(KeyCode.Joystick2Button3))) {
                int level = 1;
                Regenerate(level);
            }
        }
    }
}