using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public event Action OnLeftMouseClick, OnRightMouseClick, OnFly;
    public bool RunningPressed { get; private set; }
    public Vector3 MovementInput { get; private set; }
    public Vector2 MousePosition { get; private set; }
    public bool IsJumping { get; private set; }

    public int BlockTypeNumberSelected { get; set; }

    void Update()
    {
        GetLeftMouseClick();
        GetRightMouseClick();
        GetMousePosition();
        GetMovementInput();
        GetJumpInput();
        GetRunInput();
        GetFlyInput();
    }

    private void GetFlyInput()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            OnFly?.Invoke();
        }
    }

    private void GetRunInput()
    {
        RunningPressed = Input.GetKey(KeyCode.LeftShift);
    }

    private void GetJumpInput()
    {
        IsJumping = Input.GetButton("Jump");
    }

    private void GetMovementInput()
    {
        MovementInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    }

    private void GetMousePosition()
    {
        MousePosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    private void GetLeftMouseClick()
    {
        if (Input.GetMouseButtonDown(0)) OnLeftMouseClick?.Invoke();

    }
    private void GetRightMouseClick()
    {
        if (Input.GetMouseButtonDown(1)) OnRightMouseClick?.Invoke();

    }
}
