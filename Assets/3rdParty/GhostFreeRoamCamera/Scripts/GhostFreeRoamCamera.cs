using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GhostFreeRoamCamera : MonoBehaviour
{
    public float initialSpeed = 5f;
    public float increaseSpeed = 1f;

    public bool allowMovement = true;
    public bool allowRotation = true;

    public KeyCode forwardButton = KeyCode.W;
    public KeyCode backwardButton = KeyCode.S;
    public KeyCode rightButton = KeyCode.D;
    public KeyCode leftButton = KeyCode.A;

    public float cursorSensitivity = 0.02f;
    public bool cursorToggleAllowed = true;
    public KeyCode cursorToggleButton = KeyCode.Escape;

    private float currentSpeed = 0f;
    private bool moving = false;
    private bool togglePressed = false;

    private void OnEnable()
    {
     
        //Screen.lockCursor = true;
        //    Cursor.visible = false;
        
    }

    private void Update()
    {
        if (allowMovement)
        {
            bool lastMoving = moving;
            Vector3 deltaPosition = Vector3.zero;

            if (moving)
                currentSpeed += increaseSpeed * Time.deltaTime;

            moving = false;

            CheckMove(forwardButton, ref deltaPosition, transform.forward);
            CheckMove(backwardButton, ref deltaPosition, -transform.forward);
            CheckMove(rightButton, ref deltaPosition, transform.right);
            CheckMove(leftButton, ref deltaPosition, -transform.right);

            if (moving)
            {
                if (moving != lastMoving)
                    currentSpeed = initialSpeed;

                transform.position += deltaPosition * currentSpeed * Time.deltaTime;
            }
            else currentSpeed = 0f;            
        }

        if (allowRotation && Input.GetMouseButton(1))
        {
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.x += -Input.GetAxis("Mouse Y") * 359f * cursorSensitivity;
            eulerAngles.y += Input.GetAxis("Mouse X") * 359f * cursorSensitivity;
            transform.eulerAngles = eulerAngles;
        }

        if (cursorToggleAllowed)
        {
            if (Input.GetKey(cursorToggleButton))
            {
                if (!togglePressed)
                {
                    togglePressed = true;
                    //Screen.lockCursor = !Screen.lockCursor;
                    //Cursor.visible = !Cursor.visible;
                }
            }
            else togglePressed = false;
        }
        else
        {
            togglePressed = false;
            //Cursor.visible = false;
        }

        CameraReset();
    }


    private void CameraReset()
    {
        if (Input.GetKey(KeyCode.Alpha0))
        {
            transform.position = new Vector3(0, 0, -25);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        if (Input.GetKey(KeyCode.Alpha1))
        {
            transform.position = new Vector3(-2, 0, -23f);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            transform.position = new Vector3(1.5f, 0, -23f);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
    private void CheckMove(KeyCode keyCode, ref Vector3 deltaPosition, Vector3 directionVector)
    {
        if (Input.GetKey(keyCode))
        {
            moving = true;
            deltaPosition += directionVector;
        }
    }
}
