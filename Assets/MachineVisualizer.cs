using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu(fileName = "New Machine", menuName = "Machine")]
public class MachineData : ScriptableObject
{
    public string Name;
    public bool HasTag;
    public int Zone;
    public float Rotation;
    public LightData RedLight;
    public LightData GreenLight;
    public LightData YellowLight;
    public string TaskDescription;
    public ProductData ProductOnBelt;
    public ProductData ProductAtIn;
    public ProductData ProductAtOut;
}

[Serializable]
public class LightData
{
    public int LightColor;
    public bool LightOn;
}

public class MachineVisualizer : MonoBehaviour
{
    public string machinesUrl = "http://localhost:8000/machines";
    public List<MachineData> Machines { get; private set; }
    private Dictionary<string, GameObject> machineSprites = new Dictionary<string, GameObject>();
    public string spriteFolderPath = "Assets/sprites/machines/";

    private void Start()
    {
        // Start fetching data
        InvokeRepeating(nameof(UpdateData), 0f, 1f); // Update data every 1 second
    }

    private async void UpdateData()
    {
        await FetchData();
        if (Machines != null)
        {
            DrawMachines(Machines);
        }
    }

    private async Task FetchData()
    {
        try
        {
            UnityWebRequest request = UnityWebRequest.Get(machinesUrl);
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = request.downloadHandler.text;
                Debug.Log(response);

                // Deserialize JSON to a list of dictionaries
                var machineDictList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response);
                Machines = new List<MachineData>();

                foreach (var machineDict in machineDictList)
                {
                    MachineData machine = ScriptableObject.CreateInstance<MachineData>();

                    if (machineDict.TryGetValue("Name", out var name))
                    {
                        machine.Name = name.ToString();
                    }
                    if (machineDict.TryGetValue("HasTag", out var hasTag))
                    {
                        machine.HasTag = Convert.ToBoolean(hasTag);
                    }
                    if (machineDict.TryGetValue("Zone", out var zone))
                    {
                        machine.Zone = Convert.ToInt32(zone);
                    }
                    if (machineDict.TryGetValue("Rotation", out var rotation))
                    {
                        machine.Rotation = Convert.ToSingle(rotation);
                    }
                    if (machineDict.TryGetValue("RedLight", out var redLight))
                    {
                        machine.RedLight = JsonConvert.DeserializeObject<LightData>(redLight.ToString());
                    }
                    if (machineDict.TryGetValue("GreenLight", out var greenLight))
                    {
                        machine.GreenLight = JsonConvert.DeserializeObject<LightData>(greenLight.ToString());
                    }
                    if (machineDict.TryGetValue("YellowLight", out var yellowLight))
                    {
                        machine.YellowLight = JsonConvert.DeserializeObject<LightData>(yellowLight.ToString());
                    }
                    if (machineDict.TryGetValue("TaskDescription", out var taskDescription))
                    {
                        machine.TaskDescription = taskDescription.ToString();
                    }
                    if (machineDict.TryGetValue("ProductOnBelt", out var productOnBelt) && productOnBelt != null)
                    {
                        machine.ProductOnBelt = JsonConvert.DeserializeObject<ProductData>(productOnBelt.ToString());
                    }
                    if (machineDict.TryGetValue("ProductAtIn", out var productAtIn) && productAtIn != null)
                    {
                        machine.ProductAtIn = JsonConvert.DeserializeObject<ProductData>(productAtIn.ToString());
                    }
                    if (machineDict.TryGetValue("ProductAtOut", out var productAtOut) && productAtOut != null)
                    {
                        machine.ProductAtOut = JsonConvert.DeserializeObject<ProductData>(productAtOut.ToString());
                    }


                    Machines.Add(machine);
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

    private Vector3 GetPosition(int zone)
    {
        int y = (zone % 10);
        int x = ((zone / 10) % 10);
        return zone >= 1000 ? new Vector3(-x + 0.5f, y - 0.5f, -1) : new Vector3(x - 0.5f, y - 0.5f, -1);
    }

    private void DrawMachines(List<MachineData> machines)
    {
        foreach (var machine in machines)
        {
            Vector3 position = GetPosition(machine.Zone);
            Quaternion rotation = Quaternion.Euler(0, 0, machine.Rotation - 90);

            if (machineSprites.ContainsKey(machine.Name))
            {
                // Move existing sprite to new position
                machineSprites[machine.Name].transform.position = position;
                machineSprites[machine.Name].transform.rotation = rotation;
            }
            else
            {
                // Instantiate new sprite for the machine
                string spritePath = "sprites/machines/" + machine.Name;
                Sprite sprite = Resources.Load<Sprite>(spritePath);
                if (sprite != null)
                {
                    GameObject machineObject = new GameObject("MachineSprite_" + machine.Name);
                    SpriteRenderer renderer = machineObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    machineObject.transform.position = position;
                    machineObject.transform.rotation = rotation;
                    machineObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    machineSprites.Add(machine.Name, machineObject);
                }
                else
                {
                    Debug.LogWarning($"Sprite for Machine {machine.Name} not found at path: {spritePath}");
                }
            }

            // Draw product sprites
            DrawProduct(machine, position);
        }
    }

    private void DrawProduct(MachineData machine, Vector3 machinePosition)
    {
        // Handle ProductAtIn
        HandleProductSprite(machine, "ProductAtIn", machine.ProductAtIn, new Vector3(0.0f, 0.3f, 0), "ProductAtIn_" + machine.Name);

        // Handle ProductAtOut
        HandleProductSprite(machine, "ProductAtOut", machine.ProductAtOut, new Vector3(0.0f, -0.3f, 0), "ProductAtOut_" + machine.Name);

        // Handle ProductOnBelt
        HandleProductSprite(machine, "ProductOnBelt", machine.ProductOnBelt, new Vector3(0.5f, 0.0f, 0), "ProductOnBelt_" + machine.Name);
    }

    private void HandleProductSprite(MachineData machine, string productKey, ProductData product, Vector3 localPosition, string childObjectName)
    {
        if (machineSprites.ContainsKey(machine.Name))
        {
            Transform parentTransform = machineSprites[machine.Name].transform;
            Transform existingChild = parentTransform.Find(childObjectName);

            if (product != null)
            {
                // Update or create the product sprite
                if (existingChild == null)
                {
                    string spritePath = GetHeldProductSpritePath(product);
                    Sprite productSprite = Resources.Load<Sprite>(spritePath);
                    if (productSprite != null)
                    {
                        GameObject productObject = new GameObject(childObjectName);
                        SpriteRenderer renderer = productObject.AddComponent<SpriteRenderer>();
                        renderer.sprite = productSprite;
                        productObject.transform.parent = parentTransform;
                        productObject.transform.localPosition = localPosition;
                        productObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                        renderer.sortingOrder = 1;
                    }
                }
                else
                {
                    // Update the existing product sprite
                    existingChild.localPosition = localPosition;
                }
            }
            else if (existingChild != null)
            {
                // Delete the child if the corresponding product is null
                Destroy(existingChild.gameObject);
            }
        }
    }

    private string GetHeldProductSpritePath(ProductData product)
    {
        string baseColor = product.Base.BaseColor switch
        {
            BaseColor.BASE_RED => "RED",
            BaseColor.BASE_BLACK => "BLACK",
            BaseColor.BASE_SILVER => "SILVER",
            BaseColor.BASE_CLEAR => "CLEAR",
            _ => "UNKNOWN"
        };

        List<string> ringColors = new List<string>();
        foreach (var ring in product.RingList)
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
        if(product.Cap != null)
        {
            capColor = product.Cap.CapColor switch
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
        if (capColor != null)
        {
            output += "-CAP_" + capColor;
        }
        return output;
    }

}
