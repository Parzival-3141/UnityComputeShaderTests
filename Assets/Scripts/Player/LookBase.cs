using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LookBase : MonoBehaviour
{
    public bool IsCursorLocked { get => cursorLocked; set => ToggleCursorLock(value); }
    
    public float sensitivity = 100f;
    [Range(0, 90)] public float verticalLimitInDegrees = 89f;

    protected bool cursorLocked = false;

    protected virtual void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleCursorLock(!cursorLocked);
#endif
    }


    public virtual Vector2 GetMouseXY()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity * Time.deltaTime;
        return new Vector2(mouseX, mouseY);
    }

    // @Refactor? kinda works
    public virtual void ToggleCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        cursorLocked = locked;
    }
}
