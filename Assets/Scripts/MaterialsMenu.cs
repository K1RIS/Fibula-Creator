using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MaterialsMenu : MonoBehaviour
{ 
    [SerializeField]
    private RectTransform highlight;
    [SerializeField]
    private Dropdown dropdown;
    [SerializeField]
    private Transform grid;
    [SerializeField]
    private Button toogleView;

    private RectTransform higlightedButton;
    
    public bool interactable = true;

    public static string category = "Floor";
    public static Material material;
    
    private void Start()
    {
        loadPositionAndScale();
        createButtons("Floor");
    }

    private void Update()
    {
        highlight.position = higlightedButton.position;
    }

    private void loadPositionAndScale()
    {
        Vector2 defaultPosition = rightDownScreenCorner();

        RectTransform rt = transform.GetChild(0).GetComponent<RectTransform>();
        rt.localPosition = new Vector2(PlayerPrefs.GetFloat("x", defaultPosition.x), PlayerPrefs.GetFloat("y", defaultPosition.y));
        rt.sizeDelta = new Vector2(PlayerPrefs.GetFloat("width", 300), PlayerPrefs.GetFloat("height", 250));

        bool outsideScreen = rt.localPosition.x > Screen.width / 2 || rt.localPosition.x < Screen.width / -2 || rt.localPosition.y > Screen.height / 2 || rt.localPosition.y < Screen.height / -2;

        if (outsideScreen)
            rt.localPosition = defaultPosition;
    }

    private Vector2 rightDownScreenCorner()
    {
        RectTransform rt = transform.GetChild(0).GetComponent<RectTransform>();
        return new Vector2(Screen.width / 2 - rt.rect.width / 2, Screen.height / -2 + rt.rect.height / 2);
    }
    
    private void createButtons(string category)
    {
        Sprite[] sprites = loadSprites(category);

        GameObject newObj = new GameObject("Sprite" + (0));
        newObj.transform.SetParent(grid);
        newObj.AddComponent<Button>().onClick.AddListener(nullMaterial);
        newObj.AddComponent<Image>().color = new Color(255, 0, 254);

        higlightedButton = newObj.GetComponent<RectTransform>();

        for (int i = 0; i < sprites.Length; i++)
        {
            newObj = new GameObject("Sprite" + (i + 1));
            newObj.transform.SetParent(grid);
            newObj.AddComponent<Button>().onClick.AddListener(changeMaterial);
            newObj.AddComponent<Image>().sprite = sprites[i];
        }
    }

    private Sprite[] loadSprites(string category)
    {
        object[] loadedIcons = Resources.LoadAll(category, typeof(Sprite));
        Sprite[] sprites = new Sprite[loadedIcons.Length];

        for (int i = 0; i < loadedIcons.Length; i++)        
            sprites[i] = (Sprite)loadedIcons[i];        

        return sprites;
    }

    private void nullMaterial()
    {
        GameObject currentClicked = EventSystem.current.currentSelectedGameObject;
        material = null;
        higlightedButton = currentClicked.GetComponent<RectTransform>();
    }

    private void changeMaterial()
    {
        GameObject currentClicked = EventSystem.current.currentSelectedGameObject;
        material = (Material)Resources.Load(category + "/Materials/" + currentClicked.GetComponent<Image>().sprite.name);        
        higlightedButton = currentClicked.GetComponent<RectTransform>();
    }
            
    public void showMenu()
    {
        Vector2 defaultPosition = rightDownScreenCorner();

        transform.GetChild(0).GetComponent<RectTransform>().localPosition = new Vector2(PlayerPrefs.GetFloat("x", defaultPosition.x), PlayerPrefs.GetFloat("y", defaultPosition.y));
        dropdown.interactable = true;
        interactable = true;

        toogleView.onClick.AddListener(hideMenu);
    }

    public void hideMenu()
    {
        RectTransform rt = transform.GetChild(0).GetComponent<RectTransform>();
        rt.localPosition = new Vector2(Screen.width / 2 - rt.rect.width / 2, -Screen.height / 2 - rt.rect.height / 2 + rt.GetChild(0).GetComponent<RectTransform>().rect.height + 4f);
        dropdown.interactable = false;
        interactable = false;

        toogleView.onClick.AddListener(showMenu);
    }

    public void changeCategory(int index)
    {
        category = dropdown.options[index].text;
        material = null;
     
        for (int i = grid.childCount; i > 0; i--)        
            Destroy(grid.GetChild(i - 1).gameObject);
        
        createButtons(dropdown.options[index].text);
    }
}
