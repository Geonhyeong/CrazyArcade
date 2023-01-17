using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

#if UNITY_EDITOR

using UnityEditor;
#endif

public class MapEditor
{
#if UNITY_EDITOR

    // % (Ctrl), # (Shift), & (Alt)
    [MenuItem("Tools/GenerateMap")]
    private static void GenerateMap()
    {
        GenerateByPath("Assets/Resources/Map");
        GenerateByPath("../Common/MapData");
    }

    private static void GenerateByPath(string pathPrefix)
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");

        foreach (GameObject go in gameObjects)
        {
            Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Object", true);
            Tilemap tm_block = Util.FindChild<Tilemap>(go, "Tilemap_Block", true);

            using (var writer = File.CreateText($"{pathPrefix}/{go.name}.txt"))
            {
                int xMin = tm.cellBounds.xMin;
                int xMax = tm.cellBounds.xMax - 1;
                int yMin = tm.cellBounds.yMin;
                int yMax = tm.cellBounds.yMax - 1;

                writer.WriteLine(xMin);
                writer.WriteLine(xMax);
                writer.WriteLine(yMin);
                writer.WriteLine(yMax);

                // Collision 출력
                for (int y = yMax; y >= yMin; y--)
                {
                    for (int x = xMin; x <= xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                            writer.Write("1");
                        else
                            writer.Write("0");
                    }
                    writer.WriteLine();
                }
                
                // Block 정보 출력
                for (int y = yMax; y >= yMin; y--)
                {
                    for (int x = xMin; x <= xMax; x++)
                    {
                        TileBase tile = tm_block.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                        {
                            switch(tile.name)
                            {
                                case "block_1":
                                    writer.Write("1");
                                    break;
                                case "block_2":
                                    writer.Write("2");
                                    break;
                                case "block_3":
                                    writer.Write("3");
                                    break;
                            }
                        }
                        else
                        {
                            writer.Write("0");
                        }
                    }
                    writer.WriteLine();
                }
            }
        }
    }

#endif
}