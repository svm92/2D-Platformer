using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour {

    public int associatedGlobalVariable;
    public List<int> entrances;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            collision.GetComponent<PlayerController>().globalVariables[associatedGlobalVariable] = true;
    }

}
