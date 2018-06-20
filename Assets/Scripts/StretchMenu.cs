using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StretchMenu : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
#pragma warning disable 0649
    [SerializeField]
    private Texture2D horizontalCursor, verticalCursor, rightTiltedCursor, leftTiltedCursor;

    [SerializeField]
    private RectTransform dragPanelRT, menuRT, panelRT;

    private RectTransform rt;

    private float minWidth;
    private float minHeight;
    private float borderWidth = 4f;

    private MaterialsMenu menu;
    private bool mouseMoved;
    public static bool stretching;

    private Vector2 stretchDirection;

    private Vector3 mousePositionBeforeStretching;
    private Vector2 scaleBeforeStretching;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
        menu = transform.parent.GetComponent<MaterialsMenu>();

        calculateMinWidth();
        calculateMinHeight();
    }

    private void calculateMinWidth()
    {
        for (int i = 0; i < menuRT.childCount; i++)
        {
            minWidth += menuRT.GetChild(i).GetComponent<RectTransform>().rect.width;
        }

        minWidth += borderWidth * 2;
    }

    private void calculateMinHeight()
    {
        GridLayoutGroup gl = panelRT.GetChild(0).GetComponent<GridLayoutGroup>();
        minHeight = dragPanelRT.rect.height + menuRT.rect.height + gl.cellSize.y + gl.padding.top + gl.padding.bottom + 2 * borderWidth;
    }

    private void Update()
    {
        if (!menu.interactable)
            return;

        mouseMoved = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) == Vector2.zero ? true : false;
    }

    private void FixedUpdate()
    {
        if (stretching || mouseMoved || DragMenu.dragging)
            return;
   
        Vector2 mousePosition = getMouseRectPosition();
        bool onHorizontalBorder = detectBorderAxis(mousePosition.y, mousePosition.x, rt.rect.height, rt.rect.width);
        bool onVerticalBorder = detectBorderAxis(mousePosition.x, mousePosition.y, rt.rect.width, rt.rect.height);

        if (onHorizontalBorder || onVerticalBorder)
        {
            setStretchDirectionAndCursorTexture(calculateStretchDirection(mousePosition, onHorizontalBorder, onVerticalBorder));
        }
        else if (stretchDirection != Vector2.zero)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            stretchDirection = Vector2.zero;
        }
    }

    private Vector2 getMouseRectPosition()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        Vector2 mousePoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pointerData.position, pointerData.pressEventCamera, out mousePoint);

        return mousePoint;
    }

    private bool detectBorderAxis(float firstParam, float secondParam, float firstAxis, float secondAxis)
    {
        secondAxis /= 2;
        firstAxis /= 2;

        bool firstBorder = firstParam < firstAxis && firstParam > firstAxis - borderWidth;
        bool secondBorder = firstParam > -firstAxis && firstParam < -firstAxis + borderWidth;
        bool beetwenThirdAndFourthBorder = secondParam < secondAxis && secondParam > -secondAxis;

        return (secondBorder || firstBorder) && beetwenThirdAndFourthBorder;
    }

    private Vector2 calculateStretchDirection(Vector2 mousePos, bool onHorizontalBorder, bool onVerticalBorder)
    {
        Vector2 direction = Vector2.zero;

        if (onHorizontalBorder)
            direction.y = mousePos.y > 0 ? 1 : -1;

        if (onVerticalBorder)
            direction.x = mousePos.x > 0 ? 1 : -1;

        return direction;
    }

    private void setStretchDirectionAndCursorTexture(Vector2 direction)
    {
        if (stretchDirection == direction)
            return;

        stretchDirection = direction;

        if (stretchDirection == Vector2.right || stretchDirection == Vector2.left)
            setCursorTexture(horizontalCursor);
        else if (stretchDirection == Vector2.up || stretchDirection == Vector2.down)
            setCursorTexture(verticalCursor);
        else if (stretchDirection == new Vector2(1, 1) || stretchDirection == new Vector2(-1, -1))
            setCursorTexture(rightTiltedCursor);
        else if (stretchDirection == new Vector2(-1, 1) || stretchDirection == new Vector2(1, -1))
            setCursorTexture(leftTiltedCursor);
    }

    private void setCursorTexture(Texture2D texture)
    {
        Cursor.SetCursor(texture, new Vector2(16, 16), CursorMode.Auto);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (menu.interactable && eventData.button == PointerEventData.InputButton.Left && stretchDirection != Vector2.zero)
            startStretching();
    }

    private void startStretching()
    {
        stretching = true;

        scaleBeforeStretching = rt.rect.size;
        mousePositionBeforeStretching = Input.mousePosition;

        calculateAndSetPivot();
    }

    private void calculateAndSetPivot()
    {
        float x = stretchDirection.x;
        float y = stretchDirection.y;

        x = x == 1 ? 0 : Mathf.Abs(x);
        y = y == 1 ? 0 : Mathf.Abs(y);

        setPivot(new Vector2(x, y));
    }

    private void setPivot(Vector2 pivot)
    {
        Vector2 scale = rt.rect.size;
        Vector2 deltaPivot = rt.pivot - pivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * scale.x, deltaPivot.y * scale.y);
        rt.pivot = pivot;
        rt.localPosition -= deltaPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (stretching)
        {
            stretch(stretchDirection);
        }
    }

    private void stretch(Vector2 direction)
    {
        Vector2 scale = scaleBeforeStretching;

        if (direction.x != 0)
        {
            if (direction.x > 0)
                scale.x += (Input.mousePosition.x - mousePositionBeforeStretching.x);
            else
                scale.x -= (Input.mousePosition.x - mousePositionBeforeStretching.x);

            if (Input.mousePosition.x > Screen.width)
                scale.x -= Input.mousePosition.x - Screen.width;
            else if (Input.mousePosition.x < 0)
                scale.x += Input.mousePosition.x;

            if (scale.x < minWidth)
                scale = new Vector2(minWidth, scale.y);
        }

        if (direction.y != 0)
        {
            if (direction.y > 0)
                scale.y += (Input.mousePosition.y - mousePositionBeforeStretching.y);
            else
                scale.y -= (Input.mousePosition.y - mousePositionBeforeStretching.y);

            if (Input.mousePosition.y > Screen.height - 30 - borderWidth)
                scale.y -= Input.mousePosition.y - Screen.height + 30 + borderWidth;
            else if (Input.mousePosition.y < 0)
                scale.y += Input.mousePosition.y;

            if (scale.y < minHeight)
                scale = new Vector2(scale.x, minHeight);
        }

        rt.sizeDelta = scale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (stretching && eventData.button == PointerEventData.InputButton.Left)
        {
            endStretching();
            savePositionAndScale();
        }
    }

    private void endStretching()
    {
        setPivot(new Vector2(0.5f, 0.5f));
        stretching = false;
    }

    private void savePositionAndScale()
    {
        PlayerPrefs.SetFloat("width", rt.rect.width);
        PlayerPrefs.SetFloat("height", rt.rect.height);
        PlayerPrefs.SetFloat("x", rt.localPosition.x);
        PlayerPrefs.SetFloat("y", rt.localPosition.y);
    }
}
