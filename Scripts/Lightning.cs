using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour {

	public void activate()
    {
        gameObject.SetActive(false);
    }

    public void deactivate()
    {
        gameObject.SetActive(true);
    }

}
