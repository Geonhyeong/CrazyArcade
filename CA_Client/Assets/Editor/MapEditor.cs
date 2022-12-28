using System.Collections;
using System.Collections.Generic;
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
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");

        foreach(GameObject go in gameObjects)
        {
            Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Object", true);

            using (var writer = File.CreateText($"Assets/Resources/Map/{go.name}.txt"))
            {
                int xMin = tm.cellBounds.xMin;
                int xMax = tm.cellBounds.xMax - 1;
                int yMin = tm.cellBounds.yMin;
                int yMax = tm.cellBounds.yMax - 1;

                writer.WriteLine(xMin);
                writer.WriteLine(xMax);
                writer.WriteLine(yMin);
                writer.WriteLine(yMax);

                for (int y = yMax; y >= yMin; y--)
                {
                    for (int x = xMin; x <= xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                        {
                            if (tile.name.StartsWith("object"))
                                writer.Write("1");
                            else if (tile.name.StartsWith("block"))
                                writer.Write("2");
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

    [MenuItem("Tools/MakeObjectsTest")]
    private static void MakeObjects()
    {
        
    }

#endif
}
