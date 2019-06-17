using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuItem : MonoBehaviour {

    public string itemName;
    [HideInInspector] public string description;
    [HideInInspector] public Color nameColor = Color.black;

    public int requiredGlobalVariable = -1;
    public int alternateRequiredGlobalVariable = -1;
    public int requiredColorVariable = -1;

    PlayerController player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        changeDescriptionAccordingToName();
    }

    private void OnEnable()
    {
        if (itemName.StartsWith("Attack Power"))
        {
            // Change name
            int nPowerUps = 0;
            if (player.globalVariables[117])
                nPowerUps++;
            if (player.globalVariables[118])
                nPowerUps++;
            itemName = "Attack Power ";
            for (int i = 0; i < nPowerUps; i++)
                itemName += "+";

            // Change description
            if (nPowerUps == 1)
                description = "Slightly increases the power of all your attacks. This includes your Flame Whip and your fireballs.";
            else if (nPowerUps == 2)
                description = "Further increases the power of all your attacks. This includes your Flame Whip and your fireballs.";

        }
    }

    void changeDescriptionAccordingToName()
    {
        if (itemName.StartsWith("Flame - "))
        {
            string colorName = itemName.Substring(8).ToLower();
            description = "Allows you to turn your flames " + colorName + ", emitting " + colorName + " light.";
            nameColor = getColorFromName(colorName);
        }
        else if (itemName.StartsWith("Fireball - "))
        {
            string colorName = itemName.Substring(11).ToLower();
            description = "(X, " + colorName + " only)\n\n";
            switch (colorName)
            {
                case "red":
                    description += "Spawns a giant, slow-moving fireball. Deals more damage than other fireballs. Can light up torches.";
                    break;
                case "green":
                    description += "Spawns a small, fast-moving fireball that pierces through enemies, able to hit many at once. Can light up torches.";
                    break;
                case "blue":
                    description += "Spawns three fireballs at once in a wide arc. Can light up torches.";
                    break;
                case "cyan":
                    description += "Spawns a slow fireball that explodes into four. Can light up torches.";
                    break;
                case "magenta":
                    description += "Spawns a fireball that creates a courtain of fireballs upon hitting the ceiling. Can light up torches.";
                    break;
                case "yellow":
                    description += "Spawns a fireball that creates more fireballs in its wake. Can light up torches.";
                    break;
            }
            nameColor = getColorFromName(colorName);
        } else
        {
            switch (itemName)
            {
                case "Flame Whip":
                    description = "(Z)\n\nRotate around, hurting enemies and lighting up torches around you.";
                    break;
                case "Long Whip":
                    description = "(Z)\n\nIncreases the reach of your Flame Whip. Doesn't affect its power.";
                    break;
                case "Wall Jump":
                    description = "(X near a wall)\n\nAllows you to jump off of a wall once before touching the floor.";
                    break;
                case "Double Jump":
                    description = "(X in midair)\n\nAllows you to jump again in midair once before touching the floor. Can be chained with the Wall Jump.";
                    break;
                case "Mirror Warp":
                    description = "Allows you to enter mirrors. You will find a mirror in every realm.";
                    break;
                case "Resume":
                    description = "Leave this menu and return to the game.";
                    break;
                case "Quit":
                    description = "Return to the opening screen. Any unsaved progress will be lost.";
                    break;
            }
        }
    }

    Color getColorFromName(string colorName)
    {
        switch(colorName)
        {
            case "red":
                return Color.red;
            case "green":
                return new Color(0, 0.5f, 0);
            case "blue":
                return Color.blue;
            case "cyan":
                return new Color(0, 0.7f, 0.7f);
            case "magenta":
                return new Color(0.75f, 0, 0.75f);
            case "yellow":
                return new Color(0.6f, 0.6f, 0);
            default:
                return Color.black;
        }
    }

}
