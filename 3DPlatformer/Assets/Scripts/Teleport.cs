using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameEvent onPlayerTeleport;

    private void OnTriggerEnter(Collider other)
    {
        onPlayerTeleport.Raise(this, other.gameObject);
    }
}
