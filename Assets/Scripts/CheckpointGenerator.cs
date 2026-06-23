using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class CheckpointGenerator : MonoBehaviour
{
    [Header("Settings")]
    public string chartPath = "Assets/Charts/GrayEfflorescence/GrayEfflorescence.txt";
    public float speed = 9.0f;

    [ContextMenu("Generate Checkpoints for Player")]
    public void UpdatePlayerCheckpoints()
    {
        PlayerController player = GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogError("PlayerController not found on this GameObject!");
            return;
        }

        if (player.musicSource == null || player.musicSource.clip == null)
        {
            Debug.LogError("Player's AudioSource or Clip is missing!");
            return;
        }

        float totalDuration = player.musicSource.clip.length;
        List<float> timestamps = ParseChartTimestamps();
        
        if (timestamps.Count == 0) return;

        // Target percentages
        float[] targetPercentages = { 25f, 50f, 75f };
        List<PlayerController.Checkpoint> newCheckpoints = new List<PlayerController.Checkpoint>();

        foreach (float p in targetPercentages)
        {
            float targetMoveTime = (p / 100f) * totalDuration;
            PlayerController.Checkpoint cp = CalculateCheckpointAtTime(targetMoveTime, timestamps);
            cp.percentage = (int)p;
            
            // Adjust Y height via Raycast
            cp.position.y = AdjustToRoadHeight(cp.position, player.trailHeight);
            
            newCheckpoints.Add(cp);
            Debug.Log($"Generated CP {p}%: Time={targetMoveTime:F2}s, Pos={cp.position}, Dir={cp.direction}");
        }

        // Update player data
        #if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(player, "Update Checkpoints");
        #endif
        
        // Use reflection or make the list public to update
        var field = typeof(PlayerController).GetField("checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(player, newCheckpoints);
        }
        else
        {
            Debug.LogError("Could not find 'checkpoints' field in PlayerController. Make sure it's private/protected and spelled correctly.");
        }
    }

    private PlayerController.Checkpoint CalculateCheckpointAtTime(float targetTime, List<float> timestamps)
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
                // Target is within this segment
                float segmentProgress = targetTime - lastTime;
                Vector3 pos = currentPos + currentDir * (speed * segmentProgress);
                return new PlayerController.Checkpoint 
                { 
                    musicTime = targetTime, // This is the 'MovementTime' equivalent
                    position = pos, 
                    direction = currentDir 
                };
            }

            // Move to next turn
            currentPos += currentDir * (speed * dt);
            currentDir = (currentDir == Vector3.forward) ? Vector3.right : Vector3.forward;
            lastTime = t;
        }

        // Fallback for end of track
        return new PlayerController.Checkpoint { musicTime = targetTime, position = currentPos, direction = currentDir };
    }

    private float AdjustToRoadHeight(Vector3 pos, float trailHeight)
    {
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up * 50f, Vector3.down, out hit, 100f))
        {
            return hit.point.y + (trailHeight * 0.5f);
        }
        return 0.25f; // Default
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
