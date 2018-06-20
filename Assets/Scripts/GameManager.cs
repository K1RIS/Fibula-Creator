using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    private GameObject floorPref, wallPref, roofPref;
    public static Transform start;

    public static Vector2 max;
    public static Vector2 min;

    public static int currentLevel = 0;

    private void Awake()
    {
        floorPref = (GameObject)Resources.Load("Floor");
        wallPref = (GameObject)Resources.Load("Wall");
        roofPref = (GameObject)Resources.Load("Roof");
        start = transform;
    }

    private void Update()
    {
        if (StretchMenu.stretching || DragMenu.dragging)
            return;

        if (uiClicked() && Input.GetMouseButton(0))
        {
            RaycastHit hit;

            if (MaterialsMenu.category == "Floor")
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) && hit.collider.tag == MaterialsMenu.category && hit.transform.position.y == getYforCategory(MaterialsMenu.category, currentLevel))
                    changeMaterial(hit.collider.gameObject);
                else
                    institatePrefabOnPointer(floorPref);
            }
            else if (MaterialsMenu.category == "Wall")
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    bool wallOnCurrentLevel = hit.collider.tag == MaterialsMenu.category && hit.transform.position.y == getYforCategory(MaterialsMenu.category, currentLevel);
                    bool floorOnCurrentLevel = hit.collider.tag == "Floor" && hit.transform.position.y == getYforCategory("Floor", currentLevel);

                    if (wallOnCurrentLevel)
                        changeMaterial(hit.collider.gameObject);
                    else if (floorOnCurrentLevel)
                        instantiatePrefab(wallPref, new Vector3(hit.transform.position.x, getYforCategory("Wall", currentLevel), hit.transform.position.z));
                }
            }
            else if (MaterialsMenu.category == "Roof")
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) && hit.collider.tag == MaterialsMenu.category && hit.transform.position.y == getYforCategory(MaterialsMenu.category, currentLevel))
                    changeMaterial(hit.collider.gameObject);
                else
                    institatePrefabOnPointer(roofPref);
            }
        }
        else if (Input.GetMouseButton(1))
        {
            destroyClickedObjectfromCurrentCategory();
        }
    }

    private bool uiClicked()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return (results.Count == 0) ? true : false;
    }

    public static float getYforCategory(string category, int level)
    {
        if (category == "Floor" || category == "Roof")
            return (level) * 2 - 0.5f;
        else if (category == "Wall")
            return (level) + 0.5f;     
        else
            return 997;
    }

    private void changeMaterial(GameObject gameObject)
    {
        gameObject.GetComponent<MeshRenderer>().material = MaterialsMenu.material;
    }

    private void institatePrefabOnPointer(GameObject prefab)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.transform.position.y - getYforCategory(MaterialsMenu.category, currentLevel);
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);

        mousePos.x = Mathf.Round(mousePos.x);
        mousePos.z = Mathf.Round(mousePos.z);

        instantiatePrefab(prefab, new Vector3(mousePos.x, getYforCategory(MaterialsMenu.category, currentLevel), mousePos.z));

        Menu.unsavedChanges = true;
    }

    private void instantiatePrefab(GameObject prefab, Vector3 pos)
    {
        Transform t1 = transform.Find(MaterialsMenu.category);

        if (t1 == null)
        {
            t1 = new GameObject(MaterialsMenu.category).transform;
            t1.SetParent(transform);
        }

        Transform t2 = t1.Find(currentLevel.ToString());

        if (t2 == null)
        {
            t2 = new GameObject(currentLevel.ToString()).transform;
            t2.SetParent(t1);
        }

        GameObject gameObject = Instantiate(prefab, pos, new Quaternion(), t2);
        changeMaterial(gameObject);

        if (pos.x > max.x)
            max.x = pos.x;
        else if (pos.x < min.x)
            min.x = pos.x;

        if (pos.y > max.y)
            max.y = pos.y;
        else if (pos.y < min.y)
            min.y = pos.y;
    }

    private void destroyClickedObjectfromCurrentCategory()
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) && hit.collider.tag == MaterialsMenu.category && hit.transform.position.y == getYforCategory(MaterialsMenu.category, currentLevel))
            Destroy(hit.collider.gameObject);
    }
}
