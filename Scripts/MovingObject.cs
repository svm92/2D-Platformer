using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour {

    public List<Vector3> destinations;
    public float speed = 1f;
    public float stopTime = 0f;
    public bool isPlatform = false;
    // For initially inactive platforms. Will set this var on upon entering platform, and will remain on after saving
    public int activationVariable = -1;

    public List<int> dontStopAfterSteps = new List<int>();
    bool[] stopAfterStep;

    public GameObject associatedFakeWall;

    PlayerController player;
    SpriteRenderer spriteRenderer;
    Sprite originalSprite;
    public Sprite inactiveSprite;

    private void Start()
    {
        if (activationVariable >= 0)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalSprite = spriteRenderer.sprite;
        }

        for (int i = 0; i < destinations.Count; i++)
            destinations[i] += transform.position;
        destinations.Add(transform.position);
        if (activationVariable < 0 || player.globalVariables[activationVariable])
            StartCoroutine(followPath());
        else
            spriteRenderer.sprite = inactiveSprite;

        stopAfterStep = new bool[destinations.Count];
        for (int i=0; i < stopAfterStep.Length; i++)
        {
            stopAfterStep[i] = !dontStopAfterSteps.Contains(i);
        } 
    }

    IEnumerator followPath()
    {
        for (int i=0; i < destinations.Count; i++)
        {
            Vector3 destination = destinations[i];
            Vector3 lastPosition = transform.position;
            float distance = Vector3.Distance(lastPosition, destination);
            float travelTime = distance / speed;
            float timer = 0;
            while (transform.position != destination)
            {
                transform.position = Vector3.Lerp(lastPosition, destination, (1/travelTime) * timer);
                timer += Time.deltaTime;
                yield return null;
            }
            if (stopAfterStep[i])
                yield return new WaitForSeconds(stopTime);
        }
        StartCoroutine(followPath());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPlatform && collision.CompareTag("Player"))
        {
            if (activationVariable >= 0 && !player.globalVariables[activationVariable])
            {
                player.globalVariables[activationVariable] = true;
                spriteRenderer.sprite = originalSprite;
                StartCoroutine(followPath());
            }

            if (associatedFakeWall != null)
                Destroy(associatedFakeWall);

            collision.transform.SetParent(transform);
            StartCoroutine(pollPlayerLeaving());
        } 
    }

    IEnumerator pollPlayerLeaving() // This isn't inside a OnTriggerExit2D because of conflicts with player script
    {
        yield return new WaitForSeconds(0.1f);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (GetComponent<Collider2D>().Distance(player.GetComponent<Collider2D>()).distance > 0.05f) // Player left
        {
            player.transform.SetParent(null);
            DontDestroyOnLoad(player);
            yield break;
        }
        // If the player hasn't left yet, keep polling
        StartCoroutine(pollPlayerLeaving());
    }

}
