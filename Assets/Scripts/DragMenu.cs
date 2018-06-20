using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rt;
    
    private MaterialsMenu menu;
    private bool mouseMoved;
    private bool mouseOverDragPanel;
    private bool cursorChanged;
    private bool canDrag;
    public static bool dragging;

    [SerializeField]
    private Texture2D dragCursor;

    private Vector3 startPos;

    private void Start()
    {
        rt = transform.parent.GetComponent<RectTransform>();
        menu = transform.parent.parent.GetComponent<MaterialsMenu>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOverDragPanel = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!dragging && !StretchMenu.stretching)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            mouseOverDragPanel = false;
        }
    }

    private void Update()
    {
        if (!menu.interactable)
            return;

        mouseMoved = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) == Vector2.zero ? true : false;
    }
    
    private void FixedUpdate()
    {
        if (mouseMoved || !menu.interactable || dragging || !mouseOverDragPanel || StretchMenu.stretching)
            return;

        if (mouseNNotOverButton())
        {
            canDrag = true;
            Cursor.SetCursor(dragCursor, new Vector2(16, 16), CursorMode.Auto);
        }
        else
        {
            canDrag = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    private bool mouseNNotOverButton()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count > 0 ? (results[0].gameObject == gameObject ? true : false) : false;       
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (menu.interactable && canDrag && eventData.button == PointerEventData.InputButton.Left)
        {
            dragging = true;
            startPos = Input.mousePosition - transform.parent.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragging)
        {
            Vector2 pos = Input.mousePosition - startPos;

            if (pos.x > Screen.width)
                pos.x = Screen.width;
            else if (pos.x < 0)
                pos.x = 0;

            if (pos.y + rt.rect.height / 2 > Screen.height - 30)
                pos.y = Screen.height - rt.rect.height / 2 - 30;
            else if (pos.y < 0)
                pos.y = 0;

            transform.parent.position = pos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragging)
        {
            savePosition();

            dragging = false;
        }
    }

    private void savePosition()
    {       
        PlayerPrefs.SetFloat("x", rt.localPosition.x);
        PlayerPrefs.SetFloat("y", rt.localPosition.y);
    }
}
