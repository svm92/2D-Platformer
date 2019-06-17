using UnityEngine;
using UnityEngine.SceneManagement;

public class Mapper : MonoBehaviour
{

    public static bool noMapData = true;
    static Vector2 scale = Vector2.one;
    static Vector2 positionOfCenter = Vector2.zero;

    static Material lineMaterial;

    static void determineSpecificMapDetails()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Scene00":
                scale = new Vector2(0.0685f, 0.0495f);
                positionOfCenter = new Vector2(-2, 25);
                break;
            case "SceneRed":
                scale = new Vector2(0.15f, 0.043f);
                positionOfCenter = new Vector2(46, 73);
                break;
            case "SceneGreen":
                scale = new Vector2(0.09f, 0.09f);
                positionOfCenter = new Vector2(6, 9);
                break;
            case "SceneBlue":
                scale = new Vector2(0.095f, 0.06f);
                positionOfCenter = new Vector2(-75, -54);
                break;
            case "SceneCyan":
                scale = new Vector2(0.07f, 0.042f);
                positionOfCenter = new Vector2(80, 118);
                break;
            case "SceneMagenta":
                scale = new Vector2(0.05f, 0.04f);
                positionOfCenter = new Vector2(18, 85);
                break;
            case "SceneYellow":
                scale = new Vector2(0.0245f, 0.2f);
                positionOfCenter = new Vector2(105, 3.5f);
                break;
            case "SceneRainbow":
                scale = new Vector2(0.05f, 0.034f);
                positionOfCenter = new Vector2(0, 66);
                break;
            default: // No map data
                noMapData = true;
                return;
        }
        noMapData = false;
    }

    static void CreateLineMaterial(Texture mapCameraTexture)
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing simple colored things
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
            // Apply texture
            lineMaterial.mainTexture = mapCameraTexture;
        }
    }

    public static void printMap(PlayerController player, Texture mapCameraTexture)
    {
        determineSpecificMapDetails();
        if (noMapData)
            return;

        CreateLineMaterial(mapCameraTexture);
        //GL.PushMatrix();
        lineMaterial.SetPass(0);
        //GL.LoadPixelMatrix();
        GL.Begin(GL.LINES);
        GL.Color(Color.black);

        // Determine origin of coordinates for map
        // Get correctedScale factor
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        float ortSize = mainCamera.GetComponent<Camera>().orthographicSize;
        Vector2 correctedScale = new Vector2(scale.x * (ortSize / 6), scale.y * (ortSize / 6));

        // Start from camera position as (0, 0)
        Vector2 startPos = mainCamera.transform.position;

        // Shift according to center of map
        startPos -= new Vector2(positionOfCenter.x * correctedScale.x, positionOfCenter.y * correctedScale.y);

        // Print map areas
        foreach (GameObject mapArea in GameObject.FindGameObjectsWithTag("MapArea"))
        {
            if (player.globalVariables[mapArea.GetComponent<MapArea>().associatedGlobalVariable])
                printArea(mapArea, startPos, correctedScale);
        }

        printPlayer(player.gameObject, startPos, correctedScale);
        printSavepoints(player, startPos, correctedScale);
        printTeleporters(player, startPos, correctedScale);
        printMirror(player, startPos, correctedScale);

        GL.End();
        //GL.PopMatrix();
    }

    static void printArea(GameObject mapArea, Vector2 startPos, Vector2 correctedScale)
    {
        PolygonCollider2D col = mapArea.GetComponent<PolygonCollider2D>();
        Vector2[] positions = col.points;

        // Prepare vertices
        // Shift to (0, 0) of particular mapArea transform
        startPos += new Vector2(mapArea.transform.position.x * correctedScale.x, mapArea.transform.position.y * correctedScale.y);

        for (int i = 0; i < positions.Length; i++)
        {
            // Shift by the particular point of the mapArea
            positions[i] = new Vector2(positions[i].x * correctedScale.x, positions[i].y * correctedScale.y);
            positions[i] += startPos;
        }

        // Draw lines
        for (int i = 0; i < positions.Length; i++)
        {
            if (mapArea.GetComponent<MapArea>().entrances.Contains(i)) // If this point is an entrance/exit, ignore it
                continue;

            Vector3 firstPoint = positions[i];
            Vector3 secondPoint;
            if (i != positions.Length - 1) // For most points, the second point is the next one
                secondPoint = positions[i + 1];
            else // For the last point, the second point is the first one (in order to close the area)
                secondPoint = positions[0];

            GL.Vertex(firstPoint);
            GL.Vertex(secondPoint);
        }
    }

    static void printPlayer(GameObject player, Vector2 startPos, Vector2 correctedScale)
    {
        Vector2 playerPosition = player.transform.position;
        Vector2 playerMapPosition = startPos + 
            new Vector2(playerPosition.x * correctedScale.x, playerPosition.y * correctedScale.y);
        printRhombus(playerMapPosition, 0.15f, Color.white);
        printCross(playerMapPosition, 0.15f, Color.yellow);
    }

    static void printSavepoints(PlayerController player, Vector2 startPos, Vector2 correctedScale)
    {
        Transform savepointTr = GameObject.Find("Savepoints").transform;
        for (int i=0; i < savepointTr.childCount; i++)
        {
            GameObject savepoint = savepointTr.GetChild(i).gameObject;
            int mapAreaVar = savepoint.GetComponent<Savepoint>().associatedMapArea;
            if (player.globalVariables[mapAreaVar])
            {
                Vector2 savepointPos = savepoint.transform.position;
                Vector2 savepointMapPos = startPos + new Vector2(savepointPos.x * correctedScale.x,
                    savepointPos.y * correctedScale.y);
                printRhombus(savepointMapPos, 0.12f, new Color(0.75f, 0.2f, 0.25f));
            }
        }
    }

    static void printTeleporters(PlayerController player, Vector2 startPos, Vector2 correctedScale)
    {
        foreach (GameObject teleporter in GameObject.FindGameObjectsWithTag("Teleporter"))
        {
            // Check Collider2D is enabled to discard DontDestroyOnLoad teleporters from other scenes
            if (teleporter.name.StartsWith("WorldTeleporter") && teleporter.GetComponent<Collider2D>().enabled)
            {
                int mapAreaVar = teleporter.GetComponent<Teleporter>().associatedMapArea;
                if (mapAreaVar == -1 || player.globalVariables[mapAreaVar])
                {
                    Vector2 teleporterPos = teleporter.transform.position;
                    Vector2 teleporterMapPos = startPos + 
                        new Vector2(teleporterPos.x * correctedScale.x, teleporterPos.y * correctedScale.y) + 
                        Vector2.up * 0.075f;
                    printSquare(teleporterMapPos, 0.25f, teleporterColor(teleporter.GetComponent<Teleporter>()));
                }
            }
        }
    }

    static void printMirror(PlayerController player, Vector2 startPos, Vector2 correctedScale)
    {
        GameObject mirror = GameObject.Find("Mirror");
        int mapAreaVar = mirror.GetComponent<Teleporter>().associatedMapArea;
        if ((mapAreaVar == -1 || player.globalVariables[mapAreaVar]) && player.globalVariables[1])
        {
            Vector2 mirrorPos = mirror.transform.position;
            Vector2 mirrorMapPos = startPos + 
                new Vector2(mirrorPos.x * correctedScale.x, mirrorPos.y * correctedScale.y) + 
                Vector2.up * 0.075f;
            printSquare(mirrorMapPos, 0.3f, teleporterColor(mirror.GetComponent<Teleporter>()));
        }
    }

    static Color teleporterColor(Teleporter teleporter)
    {
        if (teleporter.fadeoutColor.r > 0.45 && teleporter.fadeoutColor.r < 0.55) // If gray
            return new Color(0.25f, 0.25f, 0.35f);

        return (teleporter.fadeoutColor);
    }

    static void printRhombus(Vector2 center, float radius, Color rhombusColor)
    {
        GL.End();
        GL.Begin(GL.QUADS);
        GL.Color(rhombusColor);
        GL.Vertex(center + Vector2.left * radius);
        GL.Vertex(center + Vector2.up * radius);
        GL.Vertex(center + Vector2.right * radius);
        GL.Vertex(center + Vector2.down * radius);
    }

    static void printSquare(Vector2 center, float squareSide, Color squareColor)
    {
        GL.End();
        GL.Begin(GL.QUADS);
        GL.Color(squareColor);
        GL.Vertex(center + (Vector2.up + Vector2.right) * squareSide * 0.5f);
        GL.Vertex(center + (Vector2.up + Vector2.left) * squareSide * 0.5f);
        GL.Vertex(center + (Vector2.down + Vector2.left) * squareSide * 0.5f);
        GL.Vertex(center + (Vector2.down + Vector2.right) * squareSide * 0.5f);
    }

    static void printCross(Vector2 center, float radius, Color crossColor)
    {
        GL.End();
        GL.Begin(GL.LINES);
        GL.Color(crossColor);
        // Horizontal line
        GL.Vertex(center + Vector2.left * radius);
        GL.Vertex(center + Vector2.right * radius);
        // Vertical line
        GL.Vertex(center + Vector2.up * radius);
        GL.Vertex(center + Vector2.down * radius);
    }

}