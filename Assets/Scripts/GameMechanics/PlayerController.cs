using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerInputSystem : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float limitXMin = -6f;
    public float limitXMax = 6f;

    void Update()
    {
        float horizontal = 0f;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal += 1f;
        }

        var gp = Gamepad.current;
        if (gp != null)
        {
            horizontal += gp.leftStick.x.ReadValue();
        }

        horizontal = Mathf.Clamp(horizontal, -1f, 1f);
        if (horizontal == 0f) return;

        Vector3 pos = transform.position;
        pos.x += horizontal * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, limitXMin, limitXMax);
        transform.position = pos;
    }
}