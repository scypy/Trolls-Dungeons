using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TrollsAndDungeons
{
	public class Pathfinding : MonoBehaviour
	{
        public Tilemap tilemap;

        private void Start()
        {
            tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        }
        //basic BFS search
        public List<Vector3> FindPath(Vector3 startPosition, Vector3 targetPosition)
        {
            //Vector3 startworldPos = tilemap.WorldToCell(startPosition);
            //Vector3 endworldPos = tilemap.WorldToCell(targetPosition);
            Vector3Int startCell = tilemap.WorldToCell(startPosition);
            Vector3Int targetCell = tilemap.WorldToCell(targetPosition);

            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

            queue.Enqueue(startCell);
            cameFrom[startCell] = startCell;
            visited.Add(startCell);

            while (queue.Count > 0)
            {
                Vector3Int currentCell = queue.Dequeue();

                if (currentCell == targetCell)
                {
                    List<Vector3> path = new List<Vector3>();
                    Vector3Int pathCell = targetCell;

                    while (pathCell != startCell)
                    {
                        path.Add(tilemap.CellToWorld(pathCell)); // + tilemap.cellSize * 0.5f
                        pathCell = cameFrom[pathCell];
                    }

                    path.Reverse();
                    return path;
                }

                //Get neighboring cells
                Vector3Int[] neighbors = new Vector3Int[]
                {
                    currentCell + Vector3Int.up,
                    currentCell + Vector3Int.down,
                    currentCell + Vector3Int.left,
                    currentCell + Vector3Int.right,
                    currentCell + Vector3Int.up + Vector3Int.right,
                    currentCell + Vector3Int.up + Vector3Int.left,
                    currentCell + Vector3Int.down + Vector3Int.right,
                    currentCell + Vector3Int.down + Vector3Int.left
                };

                foreach (Vector3Int neighbor in neighbors)
                {
                    if (IsWalkable(neighbor) && !visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        cameFrom[neighbor] = currentCell;
                        visited.Add(neighbor);
                    }
                }
            }

            //If no path found
            return new List<Vector3>();
        }

        private bool IsWalkable(Vector3Int cell)
        {
            if (tilemap.HasTile(cell))
            {
                return true;
            }

            return false;
        }

        public Vector3 CalculateFleePosition(Vector3 agentPosition, Vector3 playerPosition, float fleeDistance)
        {
            Vector3 fleeDirection = (agentPosition - playerPosition).normalized;


            Vector3 targetFleePosition = agentPosition + fleeDirection * fleeDistance;

            Vector3Int minCell = tilemap.cellBounds.min;
            Vector3Int maxCell = tilemap.cellBounds.max;
            Vector3 minWorld = tilemap.CellToWorld(minCell);
            Vector3 maxWorld = tilemap.CellToWorld(maxCell);
            targetFleePosition.x = Mathf.Clamp(targetFleePosition.x, minWorld.x, maxWorld.x);
            targetFleePosition.y = Mathf.Clamp(targetFleePosition.y, minWorld.y, maxWorld.y);

            return targetFleePosition;
        }
    }
}
