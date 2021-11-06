using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeLook : MouseLook
{
    [Header("Movement Settings")]
    public float maxSpeed = 5f;
    public float moveAcceleration = 10f;
    public float dragAcceleration = 7f;
    public float turnAccelerationScale = 2f;

    private Vector3 velocity;

    protected override void Update()
    {
        base.Update();
        
        velocity += HandleAcceleration();
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;
    }

    private Vector3 GetMoveDirection()
    {
        Vector3 moveDir = Input.GetAxisRaw("Horizontal") * transform.right + Input.GetAxisRaw("Vertical") * transform.forward;

        moveDir += Input.GetKey(KeyCode.Space) ? Vector3.up : Vector3.zero;
        moveDir += Input.GetKey(KeyCode.LeftControl) ? -Vector3.up : Vector3.zero;

        return moveDir.normalized;
    }

    private Vector3 HandleAcceleration()
    {
        Vector3 moveDir = GetMoveDirection();

        //var dotScale = Vector3.Dot(velocity.normalized, moveDir).RemapRange(-1, 1, moveTurnScale, 1);
        var dotScale = Vector3.Dot(velocity.normalized, moveDir) < 0 ? turnAccelerationScale : 1;

        Vector3 movementAccel = moveDir * moveAcceleration * dotScale;
        Vector3 dragAccel = -velocity * dragAcceleration * (/*1 - moveDir.magnitude*/ moveDir.sqrMagnitude > 0 ? 0 : 1);

        return (movementAccel + dragAccel) * Time.deltaTime;
    }

    /// <param name="color">Defaults to <see cref="Color.white"/></param>
    private void DrawPlayerLines(Vector3 lineEndPoint, Color? color = null)
    {
        Gizmos.color = color ?? Color.white;

        Gizmos.DrawWireSphere(transform.position + lineEndPoint, 1f);
        Gizmos.DrawLine(transform.position, transform.position + lineEndPoint);
    }

    private void OnDrawGizmosSelected()
    {
        DrawPlayerLines(GetMoveDirection());
        DrawPlayerLines(velocity, Color.blue);
        DrawPlayerLines(HandleAcceleration() / Time.deltaTime, Color.green);
        DrawPlayerLines(-velocity * dragAcceleration * (/*1 - moveDir.magnitude*/ GetMoveDirection().sqrMagnitude > 0 ? 0 : 1), Color.red);
    }
}
