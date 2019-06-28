using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { START,GOAL,WATER,GRASS,PATH}

public class Astar : MonoBehaviour
{
    private TileType tileType;

    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private Tile[] tiles;

    [SerializeField]
    private RuleTile water; 

    [SerializeField]
    private Camera camera;

    [SerializeField]
    private LayerMask layerMask;

    private Vector3Int startPos, goalPos;

    private Node current;

    private HashSet<Node> openList;

    private HashSet<Node> closedList;

    private Stack<Vector3Int> path;

    private HashSet<Vector3Int> changedTiles = new HashSet<Vector3Int>();

    private List<Vector3Int> waterTiles = new List<Vector3Int>();

    private bool start, goal;

    private Dictionary<Vector3Int, Node> allNodes = new Dictionary<Vector3Int, Node>();

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity,layerMask);
            if(hit.collider!=null)
            {
                Vector3 mouseworldpos = camera.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int clickPos = tilemap.WorldToCell(mouseworldpos);

                changeTile(clickPos);
            }
        } 
        
    }

    private void Initialize()
    {
        current = GetNode(startPos);

        openList = new HashSet<Node>();

        closedList = new HashSet<Node>();


        //adding start to openlist
        openList.Add(current);
    }

    private Node GetNode(Vector3Int position)
    {
        if (allNodes.ContainsKey(position))
        {
            return allNodes[position];
        }
        else
        {
            Node node = new Node(position);
            allNodes.Add(position, node);
            return node;
        }
    }

    public void Algorithm(bool step)
    {
        if (current==null)
        {
            Initialize(); 
        }

        while (openList.Count > 0 && path == null) 
        {
            List<Node> neighbors = FindNeighbors(current.Position);

            ExamineNeighbors(neighbors, current);

            UpdateCurrentTile(ref current);

            path = GeneratePath(current);

            if (step)
            {
                break;
            }
        }

        if (path != null) 
        {
            foreach (Vector3Int position  in path)
            {
                if (position != goalPos) 
                {
                    tilemap.SetTile(position, tiles[2]);
                }
            }
        }

        AstarDebugger.MyInstance.CreateTiles(openList, closedList, allNodes, startPos, goalPos, path);  
    }

    private List<Node> FindNeighbors(Vector3Int parentPosition)
    {
        

        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighborPos = new Vector3Int(parentPosition.x - x, parentPosition.y - y, parentPosition.z);

                if (y != 0 || x != 0)
                {
                    if (neighborPos != startPos && !waterTiles.Contains(neighborPos) && tilemap.GetTile(neighborPos)) 
                    {
                        Node neighbor = GetNode(neighborPos);
                        neighbors.Add(neighbor);
                    }   
                }
            }
        }
        return neighbors;
    }

    private void ExamineNeighbors(List<Node> neighbors,Node current)
    {
        for (int i = 0; i < neighbors.Count; i++)
        {
            Node neighbor = neighbors[i];

            if (!ConnectedDiagonally(current, neighbor))
            {
                continue;
            }

            int gScore = DetermineGScore(neighbors[i].Position, current.Position);

            if (openList.Contains(neighbor))
            {
                if (current.G + gScore < neighbor.G)
                {
                    CalcValues(current, neighbor, gScore);
                }
            }
            else if(!closedList.Contains(neighbor))
            {
                CalcValues(current, neighbor, gScore);

                openList.Add(neighbor);
            }
        }
    }

    private void CalcValues(Node parent,Node neighbor,int cost)
    {
        neighbor.Parent = parent;

        neighbor.G = parent.G + cost;

        neighbor.H = ((Math.Abs((neighbor.Position.x - goalPos.x)) + Math.Abs((neighbor.Position.y - goalPos.y))) * 10);

        neighbor.F = neighbor.G + neighbor.F;
    }

    private int DetermineGScore(Vector3Int neghbor,Vector3Int current)
    {
        int gScore = 0;

        int x = current.x - neghbor.x;
        int y = current.y - neghbor.y;

        if (Math.Abs(x - y) % 2 == 1) 
        {
            gScore = 10;
        }
        else
        {
            gScore = 14;
        }
        return gScore;
    }

    private void UpdateCurrentTile(ref Node current)
    {
        openList.Remove(current);

        closedList.Add(current);

        if (openList.Count > 0) 
        {
            current = openList.OrderBy(x => x.F).First();
        }
    }

    public void ChangeTileType(Tilebutons button)
    {
        tileType = button.MyTileType;
    }

    private void changeTile(Vector3Int clickPos)
    {
        if(tileType==TileType.WATER)
        {
            tilemap.SetTile(clickPos, water);
            waterTiles.Add(clickPos);
        }
        else
        {
            if(tileType==TileType.START)
            {
                if (start)
                {
                    tilemap.SetTile(startPos, tiles[3]);
                }
                start = true;
                startPos = clickPos;
            }
            else if(tileType==TileType.GOAL)
            {
                if (goal)
                {
                    tilemap.SetTile(goalPos, tiles[3]);
                }
                goal = true;

                goalPos = clickPos;
            }

            tilemap.SetTile(clickPos, tiles[(int)tileType]);
        }

        changedTiles.Add(clickPos);
    }

    private bool ConnectedDiagonally(Node currentNode, Node neighbor)
    {
        Vector3Int direct = currentNode.Position - neighbor.Position;

        Vector3Int first = new Vector3Int(current.Position.x + (direct.x * -1), current.Position.y, current.Position.z);
        Vector3Int second = new Vector3Int(current.Position.x, current.Position.y + (direct.y * -1), current.Position.z);

        if (waterTiles.Contains(first) || waterTiles.Contains(second))
        {
            return false;
        }

        return true;
    }

    private Stack<Vector3Int> GeneratePath(Node current)
    {
        if (current.Position==goalPos)
        {
            Stack<Vector3Int> finalPath = new Stack<Vector3Int>();

            while (current.Position != startPos) 
            {
                finalPath.Push(current.Position);
                current = current.Parent;
            }
            return finalPath;
        }
        return null;
    }

    public void Reset()
    {
        AstarDebugger.MyInstance.Reset(allNodes);

         
        foreach (Vector3Int position in changedTiles)
        {
            tilemap.SetTile(position, tiles[3]);
        }
        foreach(Vector3Int position in path)
        {
            tilemap.SetTile(position, tiles[3]);
        }

        tilemap.SetTile(startPos, tiles[3]);
        tilemap.SetTile(goalPos, tiles[3]);

        waterTiles.Clear();
        allNodes.Clear();
        start = false;
        goal = false;
        path = null;
        current = null;
    }
}
