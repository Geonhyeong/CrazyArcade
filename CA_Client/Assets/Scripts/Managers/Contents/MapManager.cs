using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager
{
    public Grid CurrentGrid { get; private set; }

    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int MinY { get; set; }
    public int MaxY { get; set; }

    private bool[,] _collision;

    public bool CanGo(Vector3Int cellPos)
    {
        if (cellPos.x < MinX || cellPos.x > MaxX)
            return false;
        if (cellPos.y < MinY || cellPos.y > MaxY)
            return false;

        int x = cellPos.x - MinX;
        int y = MaxY - cellPos.y;
        return !_collision[y, x];
    }

    public void LoadMap(int mapId)
    {
        DestroyMap();

        string mapName = "Map_" + mapId.ToString("000");
        GameObject go = Managers.Resource.Instantiate($"Map/{mapName}");
        go.name = "Map";

        CurrentGrid = go.GetComponent<Grid>();

        // Collision 관련 파일
        TextAsset txt = Managers.Resource.Load<TextAsset>($"Map/{mapName}");
        StringReader reader = new StringReader(txt.text);

        MinX = int.Parse(reader.ReadLine());
        MaxX = int.Parse(reader.ReadLine());
        MinY = int.Parse(reader.ReadLine());
        MaxY = int.Parse(reader.ReadLine());

        int xCount = MaxX - MinX + 1;
        int yCount = MaxY - MinY + 1;
        _collision = new bool[yCount, xCount];

        for (int y = 0; y < yCount; y++)
        {
            string line = reader.ReadLine();
            for (int x = 0; x < xCount; x++)
            {
                _collision[y, x] = (line[x] == '1' ? true : false);
            }
        }

        LoadObjects();

        GameObject tmObject = Util.FindChild(go, "Tilemap_Object", true);
        if (tmObject != null)
            tmObject.SetActive(false);

        GameObject tmBlock = Util.FindChild(go, "Tilemap_Block", true);
        if (tmBlock != null)
            tmBlock.SetActive(false);
    }

    public void DestroyMap()
    {
        GameObject map = GameObject.Find("Map");
        if (map != null)
        {
            GameObject.Destroy(map);
            CurrentGrid = null;
        }
    }

    public void LoadObjects()
    {
        GameObject map = GameObject.Find("Map");
        Tilemap tm_object = Util.FindChild<Tilemap>(map, "Tilemap_Object", true);

        for (int y = MaxY; y >= MinY; y--)
        {
            for (int x = MinX; x <= MaxX; x++)
            {
                TileBase tObject = tm_object.GetTile(new Vector3Int(x, y, 0));
                if (tObject != null)
                {
                    GameObject go = Managers.Resource.Instantiate($"Creature/{tObject.name}");
                    go.transform.position = CurrentGrid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.26f, 0.335f, (y - MaxY) * 0.1f);
                }
            }
        }
    }

    public float GetZ(Vector3Int cellPos)
    {
        return (cellPos.y - MaxY) * 0.1f;
    }
}