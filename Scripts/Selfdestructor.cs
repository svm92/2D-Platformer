using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selfdestructor : MonoBehaviour {

	void selfdestruct()
    {
        Destroy(transform.parent.gameObject);
    }

}
