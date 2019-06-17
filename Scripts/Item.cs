using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour {

    public int associatedGlobalVariable = -1;
    public int requiredGlobalVariable = -1;
    public GameObject textLabel;
    PlayerController player;

    void Start () {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        // If the player hasn't unlocked the requirement OR if player has already picked this up
        if ( (requiredGlobalVariable >= 0 && !player.globalVariables[requiredGlobalVariable])
            || player.globalVariables[associatedGlobalVariable])
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player.globalVariables[associatedGlobalVariable] = true;
            GameObject canvas = Instantiate(textLabel, transform.position, Quaternion.identity);
            Text itemText = canvas.GetComponentInChildren<Text>();

            GameObject emptyObject = new GameObject();
            emptyObject.transform.position = transform.position;
            canvas.transform.SetParent(emptyObject.transform);

            // All items automatically heal the player
            player.heal();

            // Health containers
            if (associatedGlobalVariable >= 20 && associatedGlobalVariable <= 46)
            {
                player.addHealthContainer();
                itemText.text = "Health Containers +1";
                itemText.color = Color.red;
            } else
            {
                switch (associatedGlobalVariable)
                {
                    case 1:
                        itemText.text = "Obtained Mirror Warp (can enter mirrors)";
                        itemText.color = Color.yellow;
                        GameObject.Find("Mirror").GetComponent<Teleporter>().unlockMirror();
                        break;
                    case 3:
                        itemText.text = "Obtained Flame Whip (Z)";
                        itemText.color = Color.yellow;
                        break;
                    case 4:
                        itemText.text = "Obtained Red Fireball (X while red)";
                        itemText.color = Color.yellow;
                        break;
                    case 5:
                        itemText.text = "Obtained Green Fireball (X while green)";
                        itemText.color = Color.yellow;
                        break;
                    case 6:
                        itemText.text = "Obtained Blue Fireball (X while blue)";
                        itemText.color = Color.yellow;
                        break;
                    case 7:
                        itemText.text = "Obtained Cyan Fireball (X while cyan)";
                        itemText.color = Color.yellow;
                        break;
                    case 8:
                        itemText.text = "Obtained Magenta Fireball (X while magenta)";
                        itemText.color = Color.yellow;
                        break;
                    case 9:
                        itemText.text = "Obtained Yellow Fireball (X while yellow)";
                        itemText.color = Color.yellow;
                        break;
                    case 11:
                        itemText.text = "Obtained Double Jump (C in midair)";
                        itemText.color = Color.yellow;
                        break;
                    case 100:
                        itemText.text = "Obtained Wall Jump (C when touching wall)";
                        itemText.color = Color.yellow;
                        break;
                    case 103:
                        itemText.text = "Obtained Long Whip (Z)";
                        itemText.color = Color.yellow;
                        break;
                    case 117:
                    case 118:
                        player.GetComponent<PlayerController>().power++;
                        itemText.text = "Attack power increased";
                        itemText.color = Color.yellow;
                        break;
                    default:
                        break;
                }
            }
            
            Destroy(gameObject);
        }
    }
}
