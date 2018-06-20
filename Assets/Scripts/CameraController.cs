using UnityEngine;

public class CameraController : MonoBehaviour
{
    private bool mouseOverMenu;
    public static bool canMove = true;

    private float horizontal;
    private float vertical;
    private float depth;
    private float rot;

    private void Update()
    {
        if (!canMove)
            return;

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        depth = !mouseOverMenu ? (Input.GetAxis("Mouse ScrollWheel") * 10f) : 0;

        rot = Input.GetAxis("Rotate");
    }

    private void FixedUpdate()
    {
        Vector3 futurePos = transform.position + transform.TransformDirection(new Vector3(horizontal, vertical, depth));

        if (futurePos.x > GameManager.min.x - 10 && futurePos.x < GameManager.max.x + 10 && futurePos.z > GameManager.min.y - 10 && futurePos.z < GameManager.min.y + 10)
            transform.Translate(new Vector3(horizontal, vertical, depth));

        transform.Rotate(new Vector3(0f, 0f, rot));
    }

    public void setMouseOverMenu(bool a)
    {
        mouseOverMenu = a;
    }
}
