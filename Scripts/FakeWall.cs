using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeWall : MonoBehaviour {

    public int associatedGlobalVariable;

    PlayerController player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (player.globalVariables[associatedGlobalVariable])
            Destroy(gameObject);
    }

}
