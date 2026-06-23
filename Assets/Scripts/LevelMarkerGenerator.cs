using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class LevelMarkerGenerator : MonoBehaviour
{
    [Header("Settings")]
    public string chartPath = "Assets/Charts/GrayEfflorescence/GrayEfflorescence.txt";
    public float speed = 9.0f;
    public float textOffset = 1.3f; // Road radius 1.0 + 0.3 gap
    public float sphereRadius = 0.4f;

    [ContextMenu("Generate All Markers")]
    public void GenerateMarkers()
    {
        ClearMarkers();

        GameObject root = new GameObject("LevelMarkers");
        
        PlayerController player = GetComponent<PlayerController>();
        if (player == null) return;

        float totalDuration = player.musicSource.clip.length;
        List<float> timestamps = ParseChartTimestamps();
        if (timestamps.Count == 0) return;

        // 1. Spawn Checkpoint Spheres
        var field = typeof(PlayerController).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            var checkpoints = (List<PlayerController.Checkpoint>)field.GetValue(player);
            foreach (var cp in checkpoints)
            {
                SpawnCheckpointSphere(cp, root.transform);
            }
        }

        // 2. Spawn Percentage Texts (10% to 90%)
        for (int i = 1; i <= 9; i++)
        {
            float p = i * 10f;
            // Accounting for music delay: percentage is music-based. 
            // Position on road corresponds to movementTime = musicTime + delay.
            float musicTime = (p / 100f) * totalDuration;
            float movementTime = musicTime + StartMenuManager.musicDelay;
            
            var point = CalculatePathAtTime(movementTime, timestamps);
            SpawnPercentageText(p, point.pos, point.dir, root.transform);
        }

        Debug.Log("Level markers generated successfully.");
    }

    private void SpawnCheckpointSphere(PlayerController.Checkpoint cp, Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = $"Checkpoint_{cp.percentage}%";
        sphere.transform.parent = parent;
        
        Vector3 pos = cp.position;
        float surfaceY = GetSurfaceHeight(pos);
        pos.y = surfaceY + sphereRadius;

        sphere.transform.position = pos;
        sphere.transform.localScale = Vector3.one * (sphereRadius * 2f);

        // URP Material Fix
        Renderer ren = sphere.GetComponent<Renderer>();
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) urpLit = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (urpLit == null) urpLit = Shader.Find("Standard"); // Fallback

        ren.sharedMaterial = new Material(urpLit);
        ren.sharedMaterial.color = Color.yellow;
        if (ren.sharedMaterial.HasProperty("_BaseColor"))
        {
            ren.sharedMaterial.SetColor("_BaseColor", Color.yellow);
        }
        
        // Trigger & Script
        SphereCollider col = sphere.GetComponent<SphereCollider>();
        if (col != null) col.isTrigger = true;
        sphere.AddComponent<CheckpointIndicator>();
    }

    private void SpawnPercentageText(float percentage, Vector3 pos, Vector3 dir, Transform parent)
    {
        GameObject textObj = new GameObject($"{percentage}%_Marker");
        textObj.transform.parent = parent;
        
        Vector3 perpendicular = Vector3.Cross(dir, Vector3.up).normalized;
        // Place text at the edge of the road
        Vector3 finalPos = pos - perpendicular * 1.1f;
        
        // IMPORTANT: Use the height of the road center to avoid placing text on roofs or buildings next to the road
        float roadHeight = GetSurfaceHeight(pos);
        finalPos.y = roadHeight + 0.05f; 
        
        textObj.transform.position = finalPos;
        textObj.transform.rotation = Quaternion.Euler(90, Quaternion.LookRotation(dir).eulerAngles.y, 0);

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = $"{percentage}%";
        tmp.fontSize = 18;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private float GetSurfaceHeight(Vector3 pos)
    {
        // Search from high up, but filter for road objects
        RaycastHit[] hits = Physics.RaycastAll(pos + Vector3.up * 50f, Vector3.down, 100f);
        float roadY = -100f;
        bool roadFound = false;

        foreach (var hit in hits)
        {
            GameObject obj = hit.collider.gameObject;
            // Prioritize the road track
            if (obj.name.StartsWith("Segment") || (obj.transform.parent != null && obj.transform.parent.name == "Generated_Road_Track"))
            {
                if (hit.point.y > roadY)
                {
                    roadY = hit.point.y;
                    roadFound = true;
                }
            }
        }

        if (roadFound) return roadY;

        // Fallback to ground if no road found
        float groundY = -100f;
        bool groundFound = false;
        foreach (var hit in hits)
        {
            string n = hit.collider.gameObject.name.ToLower();
            // Avoid buildings and markers
            if (n.Contains("marker") || n.Contains("checkpoint") || n.Contains("house") || n.Contains("building") || n.Contains("roof"))
                continue;

            if (hit.point.y > groundY)
            {
                groundY = hit.point.y;
                groundFound = true;
            }
        }

        return groundFound ? groundY : 0f;
    }

    private (Vector3 pos, Vector3 dir) CalculatePathAtTime(float targetTime, List<float> timestamps)
    {
        Vector3 currentPos = Vector3.zero;
        Vector3 currentDir = Vector3.forward;
        float lastTime = 0f;

        for (int i = 0; i < timestamps.Count; i++)
        {
            float t = timestamps[i];
            float dt = t - lastTime;

            if (targetTime <= t)
            {
                float segmentProgress = targetTime - lastTime;
                Vector3 pos = currentPos + currentDir * (speed * segmentProgress);
                return (pos, currentDir);
            }

            currentPos += currentDir * (speed * dt);
            currentDir = (currentDir == Vector3.forward) ? Vector3.right : Vector3.forward;
            lastTime = t;
        }
        return (currentPos, currentDir);
    }

    public void ClearMarkers()
    {
        GameObject old = GameObject.Find("LevelMarkers");
        if (old != null) DestroyImmediate(old);
    }

    private List<float> ParseChartTimestamps()
    {
        List<float> list = new List<float>();
        if (!File.Exists(chartPath)) return list;
        string[] lines = File.ReadAllLines(chartPath);
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("timegroup")) continue;
            if (float.TryParse(trimmed, out float ms)) list.Add(ms / 1000f);
        }
        return list;
    }
}
