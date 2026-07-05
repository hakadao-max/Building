using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class RuntimeInput
{
    private const float InputSystemMouseDeltaScale = 0.1f;

    public static bool GetKeyDown(KeyCode keyCode)
    {
        if (keyCode == KeyCode.None)
        {
            return false;
        }

#if ENABLE_INPUT_SYSTEM
        if (TryGetInputSystemKey(keyCode, out Key key))
        {
            return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(keyCode);
#else
        return false;
#endif
    }

    public static bool GetKey(KeyCode keyCode)
    {
        if (keyCode == KeyCode.None)
        {
            return false;
        }

#if ENABLE_INPUT_SYSTEM
        if (TryGetInputSystemKey(keyCode, out Key key))
        {
            return Keyboard.current != null && Keyboard.current[key].isPressed;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKey(keyCode);
#else
        return false;
#endif
    }

    public static bool GetMouseButtonDown(int button)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            switch (button)
            {
                case 0:
                    return Mouse.current.leftButton.wasPressedThisFrame;
                case 1:
                    return Mouse.current.rightButton.wasPressedThisFrame;
                case 2:
                    return Mouse.current.middleButton.wasPressedThisFrame;
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButtonDown(button);
#else
        return false;
#endif
    }

    public static Vector2 GetMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.delta.ReadValue() * InputSystemMouseDeltaScale;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#else
        return Vector2.zero;
#endif
    }

    public static Vector2 GetPointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePosition;
#else
        return Vector2.zero;
#endif
    }

    public static Vector2 GetMoveAxesRaw()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            Vector2 axes = Vector2.zero;
            axes.x += Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f;
            axes.x -= Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f;
            axes.y += Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f;
            axes.y -= Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? 1f : 0f;
            return axes;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#else
        return Vector2.zero;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private static bool TryGetInputSystemKey(KeyCode keyCode, out Key key)
    {
        switch (keyCode)
        {
            case KeyCode.Alpha0:
                key = Key.Digit0;
                return true;
            case KeyCode.Alpha1:
                key = Key.Digit1;
                return true;
            case KeyCode.Alpha2:
                key = Key.Digit2;
                return true;
            case KeyCode.Alpha3:
                key = Key.Digit3;
                return true;
            case KeyCode.Alpha4:
                key = Key.Digit4;
                return true;
            case KeyCode.Alpha5:
                key = Key.Digit5;
                return true;
            case KeyCode.Alpha6:
                key = Key.Digit6;
                return true;
            case KeyCode.Alpha7:
                key = Key.Digit7;
                return true;
            case KeyCode.Alpha8:
                key = Key.Digit8;
                return true;
            case KeyCode.Alpha9:
                key = Key.Digit9;
                return true;
            case KeyCode.Keypad0:
                key = Key.Numpad0;
                return true;
            case KeyCode.Keypad1:
                key = Key.Numpad1;
                return true;
            case KeyCode.Keypad2:
                key = Key.Numpad2;
                return true;
            case KeyCode.Keypad3:
                key = Key.Numpad3;
                return true;
            case KeyCode.Keypad4:
                key = Key.Numpad4;
                return true;
            case KeyCode.Keypad5:
                key = Key.Numpad5;
                return true;
            case KeyCode.Keypad6:
                key = Key.Numpad6;
                return true;
            case KeyCode.Keypad7:
                key = Key.Numpad7;
                return true;
            case KeyCode.Keypad8:
                key = Key.Numpad8;
                return true;
            case KeyCode.Keypad9:
                key = Key.Numpad9;
                return true;
            case KeyCode.A:
                key = Key.A;
                return true;
            case KeyCode.D:
                key = Key.D;
                return true;
            case KeyCode.E:
                key = Key.E;
                return true;
            case KeyCode.F:
                key = Key.F;
                return true;
            case KeyCode.R:
                key = Key.R;
                return true;
            case KeyCode.S:
                key = Key.S;
                return true;
            case KeyCode.W:
                key = Key.W;
                return true;
            case KeyCode.Escape:
                key = Key.Escape;
                return true;
            case KeyCode.LeftShift:
                key = Key.LeftShift;
                return true;
            case KeyCode.RightShift:
                key = Key.RightShift;
                return true;
            case KeyCode.Space:
                key = Key.Space;
                return true;
            case KeyCode.Tab:
                key = Key.Tab;
                return true;
            case KeyCode.LeftArrow:
                key = Key.LeftArrow;
                return true;
            case KeyCode.RightArrow:
                key = Key.RightArrow;
                return true;
            case KeyCode.UpArrow:
                key = Key.UpArrow;
                return true;
            case KeyCode.DownArrow:
                key = Key.DownArrow;
                return true;
            default:
                key = Key.None;
                return false;
        }
    }
#endif
}
