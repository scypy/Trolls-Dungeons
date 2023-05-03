using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEditor.Toolbars;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;
using Random = UnityEngine.Random;

namespace TrollsAndDungeons
{
    public class GeneratorManager : MonoBehaviour
    {
        [Header("Level Dimensions")]
        public int Width;
        public int Height;
        public int SmoothingIterations = 5;

        [Header("Tiles")]
        public Tile GroundTile;
        public Tile WallTile;
        public Tile SpawnPoint;
        public Tile TrapTile;
        public Tile GemTile;
        public Tile TorchTile;

        [Header("Probabilities")]
        public int WallChance = 4;
        public int TrapChance = 2;
        public int TorchChance = 1;
        public int GemChance = 1;

        [Header("Map")]
        public Tilemap tilemap;


        [Header("Object Parameters")]
        public GameObject Wall;
        public GameObject Trap;
        public GameObject Torch;
        public GameObject Gem;
        public int GemsInGame;
        public int TorchesInGame;
        public int TrapsInGame;

        [Header("Agents")]
        public GameObject Thief;
        public GameObject Troll;
        public GameObject TrollChief;

        public GameObject Player;
        public bool IsPaused;
        

        private readonly Vector3Int[] neighbourPositions =
        {
            Vector3Int.up,
            Vector3Int.right,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.up + Vector3Int.right,
            Vector3Int.up + Vector3Int.left,
            Vector3Int.down + Vector3Int.right,
            Vector3Int.down + Vector3Int.left
        };

        public static GeneratorManager Instance;

        private Vector3Int playerSpawnPosition = new Vector3Int(2, 2, 0);

        public Dictionary<Vector3Int, GameObject> ObjectPositionDict = new();
        public Dictionary<Vector3Int, GameObject> InteractablesDict = new();

        void Start()
        {
            Player.transform.position = playerSpawnPosition;
            GenerateLevel();
            ApplySmoothing();
            DestroyWallsNearPlayerSpawn();
            SpawnAgents();
        }

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
                Instance = this;
        }

        private void Update()
        {
            if (IsPaused)
                Time.timeScale = 0f;
            else
                Time.timeScale = 1f;
        }

        private void GenerateLevel()
        {
            //Lay ground tiles and always generate walls around map
            for (int x = 0; x <= Width; x++)
            {
                for (int y = 0; y <= Height; y++)
                {
                    var gridPosition = new Vector3Int(x, y, 0);

                    var worldPos = tilemap.GetCellCenterWorld(gridPosition);
                    if (x == 0 || x == Width)
                    {
                        tilemap.SetTile(gridPosition, WallTile);
                        var obj = Instantiate(Wall, worldPos, Quaternion.identity);
                        //ObjectPositionDict[gridPosition] = obj; 
                    }
                    if (y == 0 || y == Height)
                    {
                        tilemap.SetTile(gridPosition, WallTile);
                        var obj = Instantiate(Wall, worldPos, Quaternion.identity);
                        //ObjectPositionDict[gridPosition] = obj;
                    }

                    if (tilemap.GetTile(gridPosition) != WallTile)
                        tilemap.SetTile(gridPosition, GroundTile);
                }
            }

            //Random spawning of objects
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {

                    Vector3Int gridPosition = new Vector3Int(x, y, 0);

                    if (Random.RandomRange(0, 100) <= WallChance && IsTileAvailable(gridPosition))
                    {
                        tilemap.SetTile(gridPosition, WallTile);
                        var wall = Instantiate(Wall, tilemap.GetCellCenterWorld(gridPosition), Quaternion.identity);
                        ObjectPositionDict[gridPosition] = wall;
                    }

                    if (Random.RandomRange(0, 100) <= GemChance && IsTileAvailable(gridPosition))
                    {
                        tilemap.SetTile(gridPosition, GemTile);
                        var gem = Instantiate(Gem, tilemap.WorldToCell(gridPosition), Quaternion.identity);
                        InteractablesDict[gridPosition] = gem;
                    }
                    if (Random.RandomRange(0, 100) <= TorchChance && IsTileAvailable(gridPosition))
                    {
                        tilemap.SetTile(gridPosition, TorchTile);
                        //REPLACE TORCH
                        var torch = Instantiate(Torch, tilemap.WorldToCell(gridPosition), Quaternion.identity);
                        InteractablesDict[gridPosition] = torch;
                    }
                    if (Random.RandomRange(0, 100) <= TrapChance && IsTileAvailable(gridPosition))
                    {
                        tilemap.SetTile(gridPosition, TrapTile);
                        //REPLACE TRAP
                        var trap = Instantiate(Trap, tilemap.WorldToCell(gridPosition), Quaternion.identity);
                        InteractablesDict[gridPosition] = trap;
                    }
                }
            }
        }

        private bool IsTileAvailable(Vector3Int position)
        {
            if (tilemap.GetTile(position).name == "Ground" && position != playerSpawnPosition)
                return true;
            else
                return false;
        }
        //Using B678/S345678 rule for cave generation
        //(https://generativelandscapes.wordpress.com/2019/04/11/cave-cellular-automaton-algorithm-12-3/)
        public void ApplySmoothing()
        {
            for (int i = 0; i < SmoothingIterations; i++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    for (int y = 0; y < Height - 1; y++)
                    {
                        var gridPos = new Vector3Int(x, y, 0);
                        var worldPos = tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));

                        int wallCount = GetSurroundingWallCount(worldPos);
                        //Wall is destroyed if less than 3 or more than 5 neighbours
                        if (tilemap.GetTile(gridPos).name == "WallTile" &&  wallCount < 3 || wallCount > 5)
                        {
                            if (ObjectPositionDict.TryGetValue(gridPos, out var wall))
                            {
                                Destroy(wall);
                                tilemap.SetTile(gridPos, GroundTile);
                            }
                        }
                        //New wall is born (if empty) when has exactly 6,7,8 neighbours
                        else if (wallCount == 6 || wallCount == 7 || wallCount == 8 && tilemap.GetTile(gridPos).name == "Ground")
                        {
                            tilemap.SetTile(gridPos, WallTile);
                            Instantiate(Wall, worldPos, Quaternion.identity);
                        }
                    }
                }
            }
        }

        public int GetSurroundingWallCount(Vector3 gameOjectPosition)
        {
            var grid = tilemap.GetComponentInParent<GridLayout>();
            var gridPosition = grid.WorldToCell(gameOjectPosition);

            if (!tilemap.HasTile(gridPosition))
            {
                throw new Exception("Invalid Tile");
            }

            var sTiles = new List<TileBase>();
            foreach (var neighbourPosition in neighbourPositions)
            {
                var position = gridPosition + neighbourPosition;
                var tile = tilemap.GetTile(position);
                if (tile == null)
                    continue;
                if (tile.name == "WallTile")
                {
                    var neighbour = tilemap.GetTile(position);
                    sTiles.Add(neighbour);
                }
            }
            return sTiles.Count;
        }
        //Ensures player is not blocked by generated objects
        public void DestroyWallsNearPlayerSpawn()
        {
            tilemap.SetTile(playerSpawnPosition, GroundTile);
            foreach (var pos in neighbourPositions)
            {
                if (ObjectPositionDict.TryGetValue(playerSpawnPosition + pos, out var obj))
                {
                    Destroy(ObjectPositionDict[playerSpawnPosition + pos]);
                    ObjectPositionDict[playerSpawnPosition + pos] = null;
                    tilemap.SetTile(playerSpawnPosition + pos, GroundTile);
                }
            }
        }

        public void SpawnAgents()
        {
            int agentID = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var gridPosition = new Vector3Int(x, y, 0);
                    var tile = tilemap.GetTile(gridPosition);
                    if (IsTileAvailable(gridPosition) && IsNotNearSpawn(gridPosition))
                    {
                        switch (agentID)
                        {
                            case 0:
                                Instantiate(Thief, tilemap.GetCellCenterWorld(gridPosition), Quaternion.identity);
                                break;
                            case 1:
                                Instantiate(Troll, tilemap.GetCellCenterWorld(gridPosition), Quaternion.identity);
                                break;
                            case 2:
                                Instantiate(Troll, tilemap.GetCellCenterWorld(gridPosition), Quaternion.identity);
                                break;
                            case 4:
                                Instantiate(TrollChief, tilemap.GetCellCenterWorld(gridPosition), Quaternion.identity);
                                break;
                            case 5:
                                Instantiate(Thief, tilemap.GetCellCenterWorld(gridPosition), Quaternion.identity);
                                break;
                            case 6:
                                Instantiate(Troll, tilemap.GetCellCenterWorld(gridPosition), Quaternion.identity);
                                break;
                            case 7:
                                Instantiate(Troll, tilemap.GetCellCenterWorld(gridPosition), Quaternion.identity);
                                break;
                            default:
                                break;
                        }
                        agentID++;
                    }
                }
            }
        }

        public bool IsNotNearSpawn(Vector3Int gridPos)
        {
            if (Vector3.Distance(playerSpawnPosition, gridPos) > 7f)
                return true;
            else 
                return false;
        }

        public void UseObject(Vector3 worldPosition)
        {
            var gridPos = tilemap.WorldToCell(worldPosition);
            Destroy(InteractablesDict[gridPos]);
            //InteractablesDict[objectPosition] = null;
        }
    }
}
