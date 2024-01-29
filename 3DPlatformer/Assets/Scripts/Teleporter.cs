using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform teleportA;
    public Transform teleportB;

    public Transform blackoutA;
    public Transform blackoutB;

    private GameObject playerTransform;

    public void Teleport(Component sender, object data)
    {
        playerTransform = (GameObject) data;

        Debug.Log("Start teleporting");

        playerTransform.transform.position = teleportA.position;

        if (playerTransform.TryGetComponent<PlayerMovement>(out var player))
        {
            player.teleporting = true;
        }
        StartCoroutine(StartTeleporting());
    }


    IEnumerator StartTeleporting()
    {
        blackoutA.gameObject.SetActive(true);
        blackoutB.gameObject.SetActive(true);
        teleportB.gameObject.SetActive(true);
        playerTransform.transform.position = teleportB.position;
        yield return new WaitForSeconds(2);
        blackoutA.gameObject.SetActive(false);
        blackoutB.gameObject.SetActive(false);
        yield return new WaitForSeconds(1);

        if (playerTransform.TryGetComponent<PlayerMovement>(out var player))
        {
            player.canMove = true;
            player.teleporting = false;
        }
        teleportB.gameObject.SetActive(false);
    }
}
