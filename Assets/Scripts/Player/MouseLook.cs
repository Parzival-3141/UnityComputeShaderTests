using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : LookBase
{
    //public Transform playerBody;

    private float upDownRotation = 0f;
    private float leftRightRotation = 0f;

    private void Start()
    {
        IsCursorLocked = true;
    }

    protected override void Update()
    {
        base.Update();
        if (!cursorLocked) { return; }

        var mouse = GetMouseXY();

        upDownRotation -= mouse.y;
        upDownRotation = Mathf.Clamp(upDownRotation, -verticalLimitInDegrees, verticalLimitInDegrees);

        leftRightRotation += mouse.x;

        transform.localRotation = Quaternion.Euler(upDownRotation, leftRightRotation, 0f);
        //playerBody.Rotate(playerBody.up, mouseX);
    }
}
