using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public enum RingColorEnum
{
    RING_BLUE = 1,
    RING_GREEN = 2,
    RING_ORANGE = 3,
    RING_YELLOW = 4
}
[JsonObject]
public class RingColorClass
{
    public int RingColor { get; set; } // Assuming it has a property like ColorCode
}
public enum BaseColor
{
    BASE_RED = 1,
    BASE_BLACK = 2,
    BASE_SILVER = 3,
    BASE_CLEAR = 4
}

public enum CapColor
{
    CAP_BLACK = 1,
    CAP_GREY = 2
}

[CreateAssetMenu(fileName = "New Robot", menuName = "Robot")]
public class RobotData : ScriptableObject
{
    [JsonProperty("RobotName")]
    public string RobotName;

    [JsonProperty("TeamName")]
    public string TeamName;

    [JsonProperty("JerseyNumber")]
    public int JerseyNumber;

    [JsonProperty("TeamColor")]
    public int TeamColor;

    [JsonProperty("Position")]
    public PositionData Position;

    [JsonProperty("CurrentZone")]
    public ZoneRobotData CurrentZone;

    [JsonProperty("HomeZone")]
    public ZoneRobotData HomeZone;

    [JsonProperty("HeldProduct")]
    public ProductData HeldProduct;

    [JsonProperty("FutureProduct")]
    public ProductData FutureProduct;

    [JsonProperty("inputOutputLock")]
    public string InputOutputLock;

    [JsonProperty("FinishedTasks")]
    public List<TaskData> FinishedTasks;
}

[Serializable]
public class PositionData
{
    public float X;
    public float Y;
    public float Orientation;
}

[Serializable]
public class ZoneRobotData
{
    public int ZoneId;
    public float X;
    public float Y;
}

[Serializable]
public class ProductData
{
    [JsonProperty("ID")]
    public int ID;

    [JsonProperty("RingCount")]
    public int RingCount;

    [JsonProperty("Base")]
    public BaseData Base;

    [JsonProperty("Cap")]
    public CapData Cap;

    [JsonProperty("RingList")]
    public List<RingColorClass> RingList;
}

[Serializable]
public class BaseData
{
    [JsonProperty("BaseColor")]
    public BaseColor BaseColor;
}

[Serializable]
public class CapData
{
    [JsonProperty("CapColor")]
    public CapColor CapColor;
}

[Serializable]
public class TaskData
{
    [JsonProperty("TaskId")]
    public int TaskId;

    [JsonProperty("Successful")]
    public bool Successful;
}

public class RobotVisualizer : MonoBehaviour
{
    public string robotsUrl = "http://localhost:8000/robots";
    public List<RobotData> Robots { get; private set; }
    private Dictionary<string, GameObject> robotSprites = new Dictionary<string, GameObject>();
    public string spriteFolderPath = "Assets/sprites/robots/";

    private void Start()
    {
        // Start fetching data
        InvokeRepeating(nameof(UpdateData), 0f, 1f); // Update data every 1 second
    }

    private async void UpdateData()
    {
        await FetchData();
        if (Robots != null)
        {
            DrawRobots(Robots);
        }
    }

    private async Task FetchData()
    {
        try
        {
            UnityWebRequest request = UnityWebRequest.Get(robotsUrl);
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = request.downloadHandler.text;

                // Deserialize JSON to a list of RobotData
                var robotList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response);
                Robots = new List<RobotData>();

                foreach (var robotDict in robotList)
                {
                    RobotData robot = ScriptableObject.CreateInstance<RobotData>();

                    if (robotDict.TryGetValue("RobotName", out var robotName) && robotName != null)
                    {
                        robot.RobotName = robotName.ToString();
                    }
                    if (robotDict.TryGetValue("TeamName", out var teamName) && teamName != null)
                    {
                        robot.TeamName = teamName.ToString();
                    }
                    if (robotDict.TryGetValue("JerseyNumber", out var jerseyNumber) && jerseyNumber != null)
                    {
                        robot.JerseyNumber = Convert.ToInt32(jerseyNumber);
                    }
                    if (robotDict.TryGetValue("TeamColor", out var teamColor) && teamColor != null)
                    {
                        robot.TeamColor = Convert.ToInt32(teamColor);
                    }
                    if (robotDict.TryGetValue("Position", out var position) && position != null)
                    {
                        robot.Position = JsonConvert.DeserializeObject<PositionData>(position.ToString());
                    }
                    if (robotDict.TryGetValue("CurrentZone", out var currentZone) && currentZone != null)
                    {
                        robot.CurrentZone = JsonConvert.DeserializeObject<ZoneRobotData>(currentZone.ToString());
                    }
                    if (robotDict.TryGetValue("HomeZone", out var homeZone) && homeZone != null)
                    {
                        robot.HomeZone = JsonConvert.DeserializeObject<ZoneRobotData>(homeZone.ToString());
                    }
                    if (robotDict.TryGetValue("HeldProduct", out var heldProduct) && heldProduct != null)
                    {
                        robot.HeldProduct = JsonConvert.DeserializeObject<ProductData>(heldProduct.ToString());
                    }
                    if (robotDict.TryGetValue("FutureProduct", out var futureProduct) && futureProduct != null)
                    {
                        robot.FutureProduct = JsonConvert.DeserializeObject<ProductData>(futureProduct.ToString());
                    }
                    if (robotDict.TryGetValue("inputOutputLock", out var inputOutputLock) && inputOutputLock != null)
                    {
                        robot.InputOutputLock = inputOutputLock.ToString();
                    }
                    if (robotDict.TryGetValue("FinishedTasks", out var finishedTasks) && finishedTasks != null)
                    {
                        robot.FinishedTasks = JsonConvert.DeserializeObject<List<TaskData>>(finishedTasks.ToString());
                    }

                    Robots.Add(robot);
                }
            }
            else
            {
                Debug.LogError($"Error fetching data: {request.error}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching data: {e.Message}");
        }
    }

    private void DrawRobots(List<RobotData> robots)
    {
        foreach (var robot in robots)
        {
            Vector3 position = new Vector3(robot.Position.X, robot.Position.Y, -2);
            Quaternion rotation = Quaternion.Euler(0, 0, 360 - robot.Position.Orientation - 90);

            if (robotSprites.ContainsKey(robot.RobotName))
            {
                // Move existing sprite to new position
                var robotObject = robotSprites[robot.RobotName];
                robotObject.transform.position = position;
                robotObject.transform.rotation = rotation;

                // Update or remove held product sprite
                Transform heldProductTransform = robotObject.transform.Find("HeldProductSprite");
                if (robot.HeldProduct != null)
                {
                    string heldProductSpritePath = GetHeldProductSpritePath(robot.HeldProduct);
                    Sprite heldProductSprite = Resources.Load<Sprite>(heldProductSpritePath);
                    if (heldProductSprite != null)
                    {
                        if (heldProductTransform == null)
                        {
                            GameObject heldProductObject = new GameObject("HeldProductSprite");
                            heldProductObject.transform.parent = robotObject.transform;
                            heldProductObject.transform.localPosition = Vector3.zero;
                            heldProductObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                            SpriteRenderer heldProductRenderer = heldProductObject.AddComponent<SpriteRenderer>();
                            heldProductRenderer.sprite = heldProductSprite;
                            heldProductRenderer.sortingOrder = 1; // Ensure it's rendered above the robot
                        }
                        else
                        {
                            heldProductTransform.GetComponent<SpriteRenderer>().sprite = heldProductSprite;
                        }
                    }
                }
                else if (heldProductTransform != null)
                {
                    Destroy(heldProductTransform.gameObject);
                }
            }
            else
            {
                // Instantiate new sprite for the robot
                string color = robot.TeamColor == 0 ? "CYAN" : "MAGENTA";
                string spritePath = "sprites/robots/robot-" + color + robot.JerseyNumber;
                Sprite sprite = Resources.Load<Sprite>(spritePath);
                if (sprite != null)
                {
                    GameObject robotObject = new GameObject("RobotSprite_" + robot.RobotName);
                    SpriteRenderer renderer = robotObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    robotObject.transform.position = position;
                    robotObject.transform.rotation = rotation;
                    robotObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    robotSprites.Add(robot.RobotName, robotObject);

                    // Add held product sprite if applicable
                    if (robot.HeldProduct != null)
                    {
                        string heldProductSpritePath = GetHeldProductSpritePath(robot.HeldProduct);
                        Sprite heldProductSprite = Resources.Load<Sprite>(heldProductSpritePath);
                        if (heldProductSprite != null)
                        {
                            GameObject heldProductObject = new GameObject("HeldProductSprite");
                            heldProductObject.transform.parent = robotObject.transform;
                            heldProductObject.transform.localPosition =  new Vector3(-0.3f, 0, 0);
                            heldProductObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                            SpriteRenderer heldProductRenderer = heldProductObject.AddComponent<SpriteRenderer>();
                            heldProductRenderer.sprite = heldProductSprite;
                            heldProductRenderer.sortingOrder = 1; // Ensure it's rendered above the robot
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Sprite for Robot {robot.RobotName} not found at path: {spritePath}");
                }
            }
        }
    }

    private string GetHeldProductSpritePath(ProductData heldProduct)
    {
        string baseColor = heldProduct.Base.BaseColor switch
        {
            BaseColor.BASE_RED => "RED",
            BaseColor.BASE_BLACK => "BLACK",
            BaseColor.BASE_SILVER => "SILVER",
            BaseColor.BASE_CLEAR => "CLEAR",
            _ => "UNKNOWN"
        };

        List<string> ringColors = new List<string>();
        foreach (var ring in heldProduct.RingList)
        {
            string ringColor = (RingColorEnum)ring.RingColor switch
            {
                RingColorEnum.RING_BLUE => "BLUE",
                RingColorEnum.RING_GREEN => "GREEN",
                RingColorEnum.RING_ORANGE => "ORANGE",
                RingColorEnum.RING_YELLOW => "YELLOW",
                _ => "UNKNOWN"
            };
            ringColors.Add(ringColor);
        }

        string capColor = null;
        if(heldProduct.Cap != null)
        {
            capColor = heldProduct.Cap.CapColor switch
            {
                CapColor.CAP_BLACK => "BLACK",
                CapColor.CAP_GREY => "GREY",
                _ => null
            };
        }

        string output = "sprites/workpieces/BASE_" + baseColor;
        foreach (var ringColor in ringColors)
        {
            output += "-RING_" + ringColor;
        }
        if(capColor != null)
        {
            output += "-CAP_" + capColor;
        }
        return output;
    }
}
