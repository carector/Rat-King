using SuperTiled2Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace noisemaker
{
    // Level saved as data.json
    // Collision intgrid saved to Collision.csv always

    public class LDTKImporter : MonoBehaviour
    {
        [System.Serializable]
        class Level
        {
            public int x;
            public int y;
            public int width;
            public int height;
            public string[] layers;     // Filenames of layer .pngs
            public Entity[] entities;
        }

        class IntGrid
        {
            public int width;
            public int height;

            public int[,] grid;

            public IntGrid(string path)
            {
                // Parse .csv file
                string[] lines = File.ReadAllLines(path);
                lines[lines.Length - 1] += ",";     // missing final comma

                height = lines.Length;
                width = lines[0].Split(',').Length - 1;
                grid = new int[height, width];

                for (int i = 0; i < height; i++)
                {
                    string[] nums = lines[i].Split(',');
                    for (int j = 0; j < nums.Length - 1; j++)
                        grid[i, j] = int.Parse(nums[j]);
                }
            }
        }

        [System.Serializable]
        class Entity
        {
            public int x;
            public int y;
            public string[] customFields;
        }

        public int pixelsPerUnit = 16;
        public bool useStreamingAssets = true;

        [Header("Refs")]
        [SerializeField]
        TileBase collisionTile;

        [InspectorButton("GenerateLevelTest")]
        public bool generateLevel;

        public void GenerateLevelTest()
        {
            GenerateLevel("Level_0");
        }

        public void GenerateLevel(string levelName)
        {
            string path = "";
            if (useStreamingAssets)
                path = Path.Combine(Application.streamingAssetsPath, "LDtk/world/simplified", levelName);
            else
                path = ""; // Path.Combine(Application.path)

            string json = File.ReadAllText(path + "/data.json");

            Level w = JsonUtility.FromJson<Level>(json);
            IntGrid collision = new IntGrid(path + "/Collision.csv");

            GameObject oldLevel = GameObject.Find(levelName);
            if (oldLevel != null)
                DestroyImmediate(oldLevel);

            // Add components
            GameObject grid = new GameObject(levelName);
            grid.AddComponent<Grid>();
            GameObject tilemap = new GameObject("Tilemap");
            tilemap.transform.parent = grid.transform;
            tilemap.transform.tag = "Ground";

            Tilemap t = tilemap.AddComponent<Tilemap>();
            //tilemap.AddComponent<TilemapRenderer>().sortingLayerName = "Background";
            TilemapCollider2D col1 = tilemap.AddComponent<TilemapCollider2D>();
            col1.usedByComposite = true;
            Rigidbody2D rb = tilemap.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            tilemap.AddComponent<CompositeCollider2D>();

            // Place collision tiles
            for (int i = 0; i < collision.height; i++)
            {
                for (int j = 0; j < collision.width; j++)
                {
                    if (collision.grid[i, j] == 1)
                        t.SetTile(new Vector3Int(j, collision.height-i-1, 0), collisionTile);
                }
            }

            // Place baked layer pngs
            for(int i = 0; i < w.layers.Length; i++)
            {
                Sprite s;
                if (useStreamingAssets)
                {
                    Texture2D tex = new Texture2D(w.width, w.height);
                    ImageConversion.LoadImage(tex, File.ReadAllBytes(Path.Combine(path, w.layers[i])));
                    tex.filterMode = FilterMode.Point;
                    s = Sprite.Create(tex, new Rect(0, 0, w.width, w.height), Vector2.zero, pixelsPerUnit);
                }
                else
                    s = Resources.Load<Sprite>(Path.Combine(path, w.layers[i]));

                GameObject layer = new GameObject(w.layers[i]);
                SpriteRenderer spr = layer.AddComponent<SpriteRenderer>();
                spr.sprite = s;
                spr.sortingLayerName = "Background";
                spr.sortingOrder = i;
                layer.transform.parent = grid.transform;
            }
        }
    }
}