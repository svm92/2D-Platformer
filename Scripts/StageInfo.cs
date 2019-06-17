using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageInfo : MonoBehaviour {

    public static string mapName()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Scene00")
            return "Gray Realm";

        if (sceneName.StartsWith("SceneBoss"))
        {
            // 'SceneBoss' + 'Color00'
            int secondPartLength = sceneName.Length - 9;
            int colorLength = secondPartLength - 2;
            return sceneName.Substring(9, colorLength) + " Realm";
        } 

        if (sceneName.StartsWith("SceneMainBoss"))
            return sceneName.Substring(13) + " Realm";

        if (sceneName == "SceneFinalBoss")
            return "Rainbow Realm";

        return sceneName.Substring(5) + " Realm";
    }

    // x is first room in that scene, y is last one
    static Vector2 firstAndLastRoomInScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Scene00":
                return new Vector2(500, 580);
            case "SceneRed":
                return new Vector2(200, 239);
            case "SceneGreen":
                return new Vector2(250, 297);
            case "SceneBlue":
                return new Vector2(150, 194);
            case "SceneCyan":
                return new Vector2(300, 362);
            case "SceneMagenta":
                return new Vector2(450, 484);
            case "SceneYellow":
                return new Vector2(400, 436);
            case "SceneRainbow":
                return new Vector2(600, 624);
        }

        return Vector2.zero;
    }

	public static float statsMapLocal(PlayerController player)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Vector2 mapVars = firstAndLastRoomInScene(sceneName);

        if (mapVars == Vector2.zero)
            return -1;

        return mapCompletion(new List<Vector2>() { mapVars }, player);
    }

    public static float statsMapGlobal(PlayerController player)
    {
        List<Vector2> mapVars = new List<Vector2>();
        string[] mappedScenes = new string[] { "Scene00", "SceneRed", "SceneGreen", "SceneBlue", "SceneCyan",
            "SceneMagenta", "SceneYellow", "SceneRainbow"};

        foreach (string scene in mappedScenes)
            mapVars.Add(firstAndLastRoomInScene(scene));

        return mapCompletion(mapVars, player);
    }
    
    static float mapCompletion(List<Vector2> mapVars, PlayerController player)
    {
        int nOfVisitedRooms = 0;
        int totalNOfRooms = 0;
        foreach (Vector2 scene in mapVars)
        {
            if (scene == Vector2.zero)
                continue;

            {
                // scene.x -> First room in that scene; scene.y -> Last room in that scene
                for (int i = (int)scene.x; i <= scene.y; i++)
                {
                    totalNOfRooms++;
                    if (player.globalVariables[i])
                        nOfVisitedRooms++;
                }
            }
        }
        
        float mapCompletion = nOfVisitedRooms / (float)totalNOfRooms;
        mapCompletion *= 100;
        mapCompletion = (float)System.Math.Round(mapCompletion, 1); // Get one decimal

        return mapCompletion;
    }

    static List<int> itemsInStage(string sceneName)
    {
        switch (sceneName)
        {
            case "Scene00":
                return new List<int> { 1, 25, 32, 39, 1000}; // 1000 is used to collectively identify the cubes
            case "SceneRed":
                return new List<int> { 3, 4, 21, 24, 36};
            case "SceneGreen":
                return new List<int> { 22, 23, 5, 100, 37, 118 };
            case "SceneBlue":
                return new List<int> { 20, 6, 43, 44, 45, 46 };
            case "SceneCyan":
                return new List<int> { 26, 27, 28, 29, 30, 31, 7, 103 };
            case "SceneMagenta":
                return new List<int> { 33, 34, 8, 35, 11, 38 };
            case "SceneYellow":
                return new List<int> { 9, 117, 40, 41, 42 };
        }

        return new List<int>();
    }

    public static float statsItemLocal(PlayerController player)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        List<int> listOfItems = itemsInStage(sceneName);

        if (listOfItems.Count == 0)
            return -1;

        return itemCompletion(listOfItems, player);
    }

    public static float statsItemGlobal(PlayerController player)
    {
        List<int> listOfItems = new List<int>();
        string[] scenesWithItems = new string[] { "Scene00", "SceneRed", "SceneGreen", "SceneBlue", "SceneCyan",
            "SceneMagenta", "SceneYellow"};
        foreach (string scene in scenesWithItems)
            listOfItems.AddRange(itemsInStage(scene));

        if (listOfItems.Count == 0)
            return -1;

        return itemCompletion(listOfItems, player);
    }

    static float itemCompletion(List<int> listOfItems, PlayerController player)
    {
        int nOfObtainedItems = 0;
        foreach (int itemVar in listOfItems)
        {
            if (itemVar == 1000) // Color cubes
            {
                foreach (bool colorCubeObtained in player.unlockedColors)
                    if (colorCubeObtained)
                        nOfObtainedItems++;
            } else if (player.globalVariables[itemVar]) // Every other item
                nOfObtainedItems++;
        }

        int totalNOfItems = listOfItems.Count;
        if (listOfItems.Contains(1000)) // If it contains the color cubes, count it as 6 instead of 1
            totalNOfItems += 5;

        float itemCompletion = nOfObtainedItems / (float)totalNOfItems;
        itemCompletion *= 100;
        itemCompletion = (float)System.Math.Round(itemCompletion, 1); // Get one decimal

        return itemCompletion;
    }
}
