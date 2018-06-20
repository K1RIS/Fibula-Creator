using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private GameObject gameManager, savePromptPref;

    [SerializeField]
    private Text level;

    public static bool unsavedChanges = false;

    private string path;
    
    public void newProject()
    {
        if (unsavedChanges)
            Instantiate(savePromptPref).GetComponent<SavePrompt>().action = NewProject;
        else
            NewProject();
    }

    private void NewProject()
    {
        GameManager.currentLevel = 0;
        level.text = "Level: " + GameManager.currentLevel;
        path = null;
        Destroy(GameManager.start.gameObject);
        Instantiate(gameManager);
        Camera.main.transform.position = new Vector3(0, 10, 0);
    }

    public void load()
    {
        if (unsavedChanges)
            Instantiate(savePromptPref).GetComponent<SavePrompt>().action = Load;
        else
            Load();
    }

    private void Load()
    {
        var paths = SFB.StandaloneFileBrowser.OpenFilePanel("Open File", "", "jd", false);
        path = paths[0];

        StartCoroutine(create(new System.Uri(paths[0]).AbsoluteUri));

        unsavedChanges = false;
    }

    public void save()
    {
        if (string.IsNullOrEmpty(path))
            saveAs();
        else
        {
            if (!string.IsNullOrEmpty(path))
                File.WriteAllText(path, getData());

            unsavedChanges = false;
        }
    }

    public void saveAs()
    {
        path = SFB.StandaloneFileBrowser.SaveFilePanel("Save As", "", "", "jd");

        if (!string.IsNullOrEmpty(path))
            File.WriteAllText(path, getData());

        unsavedChanges = false;
    }

    public void exit()
    {
        if (unsavedChanges)
            Instantiate(savePromptPref).GetComponent<SavePrompt>().action = Exit;
        else
            Exit();
    }

    private void Exit()
    {
        Application.Quit();
    }

    private string getData()
    {
        StringBuilder data = new StringBuilder();

        Vector2 diff = new Vector2((int)(((GameManager.max.x - GameManager.min.x) / 2) + GameManager.min.x), (int)(((GameManager.max.y - GameManager.min.y) / 2) + GameManager.min.y));

        for (int i = 0; i < GameManager.start.childCount; i++)
        {
            data.Append(GameManager.start.GetChild(i).name + "[ ");

            for (int j = 0; j < GameManager.start.GetChild(i).childCount; j++)
            {
                List<string> material = new List<string>();
                List<List<Vector2>> positions = new List<List<Vector2>>();

                data.Append(GameManager.start.GetChild(i).GetChild(j).name + "{ ");

                for (int y = 0; y < GameManager.start.GetChild(i).GetChild(j).childCount; y++)
                {
                    string materialName = GameManager.start.GetChild(i).GetChild(j).GetChild(y).GetComponent<MeshRenderer>().material.name;
                    materialName = materialName.Remove(materialName.Length - 11);
                    int index = material.IndexOf(materialName);

                    if (index == -1)
                    {
                        material.Add(materialName);
                        positions.Add(new List<Vector2>());

                        index = material.Count - 1;
                    }

                    Vector3 currentPos = GameManager.start.GetChild(i).GetChild(j).GetChild(y).position;
                    positions[index].Add(new Vector2(currentPos.x - diff.x, currentPos.z - diff.y));
                }

                for (int y = 0; y < material.Count; y++)
                {
                    data.Append(material[y] + " ");

                    foreach (Vector2 vec in positions[y])
                    {
                        data.Append(vec.ToString().Replace(" ", "") + " ");
                    }                   
                }               
            }            
        }
        data.Length--;
        return data.ToString();
    }

    private IEnumerator create(string url)
    {   
        Destroy(GameManager.start.gameObject);
        string[] data = new WWW(url).text.Split(' ');
        Transform t0 = Instantiate(gameManager).transform;  
        Transform t1 = new GameObject(data[0].Remove(data[0].Length - 1)).transform;
        t1.SetParent(t0);
        Transform t2 = new GameObject(data[1].Remove(data[1].Length - 1)).transform;
        t2.SetParent(t1);
        string materialName = data[2];

        int highestLevel = int.Parse(t2.name);

        for (int i = 3; i < data.Length; i++)
        {
            if (data[i][data[i].Length - 1] == '[')
            {
                t1 = new GameObject(data[i].Remove(data[i].Length - 1)).transform;
                t1.SetParent(t0);
            }
            else if (data[i][data[i].Length - 1] == '{')
            {
                t2 = new GameObject(data[i].Remove(data[i].Length - 1)).transform;
                t2.SetParent(t1);

                if (int.Parse(t2.name) > highestLevel)
                    highestLevel = int.Parse(t2.name);
            }
            else if (data[i][data[i].Length - 1] != ')')
            {
                materialName = data[i];
            }
            else
            {
                Vector2 vec = stringToVector(data[i]);
                Vector3 pos = new Vector3(vec.x, GameManager.getYforCategory(t1.name, int.Parse(t2.name)), vec.y);
                Instantiate((GameObject)Resources.Load(t1.name), pos, new Quaternion(), t2).GetComponent<MeshRenderer>().material = (Material)Resources.Load(t1.name + "/Materials/" + materialName);
            }
        }

        GameManager.currentLevel = highestLevel;
        level.text = "Level: " + GameManager.currentLevel;
        Camera.main.transform.position = new Vector3(0, GameManager.getYforCategory("Floor", highestLevel) + 10, 0);

        yield return null;
    }

    private Vector2 stringToVector(string text)
    {
        if (text.StartsWith("(") && text.EndsWith(")"))
        {
            text = text.Substring(1, text.Length - 2);
        }

        string[] sArray = text.Split(',');
        
        Vector2 result = new Vector2(
            float.Parse(sArray[0]),
            float.Parse(sArray[1])
        );
        
        return result;
    }

    public void moveFloor(string direction)
    {
        int x = int.Parse(direction.Split(',')[0]);
        int y = int.Parse(direction.Split(',')[1]);

        for (int i = 0; i < GameManager.start.childCount; i++)
        {
            Transform t2 = GameManager.start.GetChild(i).Find(GameManager.currentLevel.ToString());

            if (t2 != null)
                t2.position += new Vector3(x, 0, y);
        }
    }

    public void higherFloor()
    {
        if (GameManager.currentLevel < 9)
        {
            setActiveLevelAbove(true);

            GameManager.currentLevel++;
            level.text = "Level: " + GameManager.currentLevel;
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y + 2, Camera.main.transform.position.z);            
        }
    }

    public void lowerFloor()
    {
        if (GameManager.currentLevel > 0)
        {           
            GameManager.currentLevel--;
            level.text = "Level: " + GameManager.currentLevel;
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y - 2, Camera.main.transform.position.z);

            setActiveLevelAbove(false);
        }
    }

    private void setActiveLevelAbove(bool a)
    {
        for (int i = 0; i < GameManager.start.childCount; i++)
        {
            Transform t2 = GameManager.start.GetChild(i).Find((GameManager.currentLevel + 1).ToString());

            if (t2 != null)
                t2.gameObject.SetActive(a);
        }
    }
}
