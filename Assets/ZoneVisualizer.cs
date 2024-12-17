using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu(fileName = "New Zone", menuName = "Zone")]
public class ZoneData : ScriptableObject
{
    public int ZoneId;
    public float X;
    public float Y;
}

public class ZoneVisualizer : MonoBehaviour
{
    public string zonesUrl = "http://localhost:8000/zones";
    public List<ZoneData> Zones { get; private set; }
    private Dictionary<int, GameObject> zoneSprites = new Dictionary<int, GameObject>();
    public string spriteFolderPath = "Assets/sprites/zones/";

    private void Start()
    {
        // Start fetching data
        InvokeRepeating(nameof(UpdateData), 0f, 1f); // Update data every 1 second
    }

    private async void UpdateData()
    {
        await FetchData();
        if (Zones != null)
        {
            DrawZones(Zones);
        }
    }

    private async Task FetchData()
    {
        try
        {
            UnityWebRequest request = UnityWebRequest.Get(zonesUrl);
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = request.downloadHandler.text;

                // Deserialize JSON to a list of dictionaries
                var zoneDictList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response);
                Zones = new List<ZoneData>();

                foreach (var zoneDict in zoneDictList)
                {
                    ZoneData zone = ScriptableObject.CreateInstance<ZoneData>();

                    if (zoneDict.TryGetValue("ZoneId", out var zoneId))
                    {
                        zone.ZoneId = Convert.ToInt32(zoneId);
                    }
                    if (zoneDict.TryGetValue("X", out var x))
                    {
                        zone.X = Convert.ToSingle(x);
                    }
                    if (zoneDict.TryGetValue("Y", out var y))
                    {
                        zone.Y = Convert.ToSingle(y);
                    }

                    Zones.Add(zone);
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

    [Serializable]
    public class ZoneListWrapper
    {
        public List<ZoneData> Zones;
    }

    public string ZoneIdToName(int zoneId)
    {
        string output = "";
        if(zoneId > 1000) {
            output += "M_Z";
        } else {
            output += "C_Z";
        }
        //Last two digits as string
        output += (zoneId % 100).ToString();
        return output;
    }

    private void DrawZones(List<ZoneData> zones)
    {
        foreach (var zone in zones)
        {
            Vector3 position = new Vector3(zone.X, zone.Y, 0);
            if (zoneSprites.ContainsKey(zone.ZoneId))
            {
                // Move existing sprite to new position
                zoneSprites[zone.ZoneId].transform.position = position;
            }
            else
            {
                // Instantiate new sprite for the zone
                string spritePath = "sprites/zones/" + ZoneIdToName(zone.ZoneId);
                Sprite sprite = Resources.Load<Sprite>(spritePath);
                if (sprite != null)
                {
                    GameObject zoneObject = new GameObject("ZoneSprite_" + zone.ZoneId);
                    SpriteRenderer renderer = zoneObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    zoneObject.transform.position = position;
                    zoneSprites.Add(zone.ZoneId, zoneObject);
                }
                else
                {
                    Debug.LogWarning($"Sprite for Zone ID {zone.ZoneId} not found at path: {spritePath}");
                }
            }
        }
    }
}
