using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    void Update()
    {
        if (!isLocalPlayer) { return; }

        float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
        float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;

        transform.Rotate(0, moveX, 0);
        transform.Translate(0, 0, moveZ);
    }
}
