using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Material defaultGroundMaterial;
    [SerializeField] Material groundHoverMaterial;
    [SerializeField] GameObject waypointPrefab;
    [SerializeField] int maxWaypointSpawn = 4;
    [SerializeField] GameObject playButton;
    [SerializeField] Transform lrOrigin;

    (Vector3, Quaternion) initialPos;
    int currentWaypointIndex = 0;
    int waypointStart = 0;
    bool playLevel;

    RaycastHit groundHit;
    GameObject playerStep;
    GameObject waypoint;
    Renderer playerStepRenderer;

    RaycastHit mouseRayHit;
    GameObject mouseGroundHover;
    Renderer mouseHoverRenderer;

    List<GameObject> waypointCollection;
    LineRenderer lr;

    private void Start()
    {
        waypointCollection = new List<GameObject>();
        lr = GetComponent<LineRenderer>();
        // lr.positionCount = 2;

        // initial position of the player, adjust pos to middle of the tile / cube
        Physics.Raycast(transform.position, Vector3.down, out groundHit, Mathf.Infinity, groundLayer);
        Vector3 targetPosition = groundHit.collider.gameObject.transform.position;
        targetPosition.y = transform.position.y;
        transform.position = targetPosition;

        // get initial npc position
        initialPos = (transform.position, transform.rotation);
    }

    private void Update()
    {
        if (waypointCollection.Count <= 1)
            lr.SetPosition(0, lrOrigin.position);

        // change ground / tile where the player is standing
        Physics.Raycast(transform.position, Vector3.down, out groundHit, Mathf.Infinity, groundLayer);

        // respawn player if ground is not detected / goes through wall
        if (groundHit.collider == null)
        {
            transform.SetPositionAndRotation(initialPos.Item1, initialPos.Item2);
            playLevel = false;
            waypointStart = 0;
        }

        try
        {
            if (playerStep != groundHit.collider.gameObject && playerStep != null)
                playerStepRenderer.material = defaultGroundMaterial;
        }
        catch (System.Exception) { Debug.Log("Player Respawned; Check yo path!"); }

        // player hover
        if (groundHit.collider != null)
        {
            playerStep = groundHit.collider.gameObject;
            playerStepRenderer = groundHit.collider.gameObject.GetComponent<Renderer>();
            playerStepRenderer.material = groundHoverMaterial;
        }

        // MOUSE HOVER
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out mouseRayHit, Mathf.Infinity);
        try
        {
            if (mouseRayHit.collider.gameObject.CompareTag("Ground Grid"))
            {
                if (mouseGroundHover != null && mouseGroundHover != mouseRayHit.collider.gameObject)
                    mouseHoverRenderer.material = defaultGroundMaterial;

                if (mouseRayHit.collider != null)
                {
                    mouseGroundHover = mouseRayHit.collider.gameObject;
                    mouseHoverRenderer = mouseRayHit.collider.gameObject.GetComponent<Renderer>();
                    mouseHoverRenderer.material = groundHoverMaterial;
                }
            }

            // Mouse Binds
            if (Input.GetMouseButtonDown(0))
                PlaceWaypoint(mouseRayHit.collider.gameObject, waypointPrefab);

            // debug keybinds
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (waypointCollection == null) return;

                foreach (var item in waypointCollection)
                    Debug.Log("Transform position of waypoint:" + item.transform);
            }
        }
        catch (System.Exception) { }

        // start moving the player if play button is pressed
        if (waypointStart < waypointCollection.Count && playLevel)
        {
            Vector3 waypointDestination = waypointCollection[waypointStart].transform.position;
            waypointDestination.y = 1.5f;
            Vector3 moveNPC = Vector3.Lerp(transform.position, waypointDestination, moveSpeed);
            transform.position = moveNPC;

            float distance = Vector3.Distance(transform.position, waypointDestination);
            if (distance <= 0.05 && waypointStart < waypointCollection.Count)
                waypointStart++;
        }

        // Activate the play button
        if (maxWaypointSpawn == 0)
            playButton.SetActive(true);

        // KEYBINDS
        if (Input.GetKeyDown(KeyCode.R) && waypointCollection.Count > 0)
            UndoPlacement();

        if (Input.GetKeyDown(KeyCode.P))
            ResetLevel();
    }

    void PlaceWaypoint(GameObject groundGridObj, GameObject waypointPrefab)
    {
        if (groundGridObj.CompareTag("Ground Grid") && !groundGridObj.CompareTag("Wall"))
        {
            if (maxWaypointSpawn != 0)
            {
                maxWaypointSpawn--;
                currentWaypointIndex++;
                lr.positionCount++;
                Debug.Log(currentWaypointIndex);

                Vector3 spawnPosition = mouseGroundHover.transform.position;
                spawnPosition.y = 2f;
                waypoint = Instantiate(waypointPrefab, spawnPosition, Quaternion.identity);
                waypointCollection.Add(waypoint);

                // draw line renderer
                Vector3 lrSpawnPos = waypoint.transform.position;
                lrSpawnPos.y = waypoint.transform.position.y * .5f;
                lr.SetPosition(currentWaypointIndex, lrSpawnPos);

                playButton.SetActive(false);
                Debug.Log("maxWaypointSpawn: " + maxWaypointSpawn);
            }
        }
    }

    void UndoPlacement()
    {
        GameObject currentWaypoint = waypointCollection[^1];
        waypointCollection?.Remove(currentWaypoint);
        Destroy(currentWaypoint);
        currentWaypointIndex--;
        lr.positionCount--;
        maxWaypointSpawn++;

        playButton.SetActive(false);
    }

    void ResetLevel() => SceneHandler.Instance.RestartCurrentScene();

    public void PlayLevelButton() => playLevel = true;
}
