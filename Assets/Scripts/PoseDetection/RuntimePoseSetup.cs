/*
 * Runtime Pose Detection Setup
 * This script can be run in the Unity Editor or at runtime to set up pose detection
 */

using UnityEngine;
using PoseDetection;

public class RuntimePoseSetup : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool enableDebugMode = true;
    
    private void Start()
    {
        if (setupOnStart)
        {
            SetupPoseDetectionSystem();
        }
    }
    
    [ContextMenu("Setup Pose Detection Now")]
    public void SetupPoseDetectionSystem()
    {
        Debug.Log("🚀 Setting up Pose Detection System...");
        
        // Create the pose detection manager
        GameObject manager = GameObject.Find("PoseDetectionManager");
        if (manager == null)
        {
            manager = new GameObject("PoseDetectionManager");
            Debug.Log("✅ Created PoseDetectionManager");
        }
        
        // Add the optimized WebSocket client
        if (manager.GetComponent<PoseWebSocketClientOptimized>() == null)
        {
            var client = manager.AddComponent<PoseWebSocketClientOptimized>();
            Debug.Log("✅ Added PoseWebSocketClientOptimized");
            
            // Enable debug logs if requested
            if (enableDebugMode)
            {
                // Use the public method to enable debug logs
                client.SetPerformanceSettings(true, true, 0.01f);
                Debug.Log("✅ Enabled debug logging for WebSocket client");
            }
        }
        
        // Add the input controller
        if (manager.GetComponent<PoseInputController>() == null)
        {
            var controller = manager.AddComponent<PoseInputController>();
            Debug.Log("✅ Added PoseInputController");
            
            // Find and connect the character controller
            var characterController = FindObjectOfType<CharacterInputController>();
            if (characterController != null)
            {
                controller.CharacterController = characterController;
                Debug.Log($"✅ Connected to CharacterInputController on '{characterController.name}'");
            }
            else
            {
                Debug.LogWarning("⚠️ CharacterInputController not found in scene!");
            }
            
            // Enable debug mode if requested
            if (enableDebugMode)
            {
                // Use the public method to enable debug settings
                controller.SetDebugSettings(true, true);
                Debug.Log("✅ Enabled debug logging for input controller");
            }
        }
        
        // Keep the manager alive across scene changes
        DontDestroyOnLoad(manager);
        
        Debug.Log("🎉 Pose Detection System is ready!");
        Debug.Log("📺 Make sure your Python server is running on ws://localhost:8765");
        Debug.Log("🎮 Try making gestures in front of your webcam!");
    }
    
    private void OnGUI()
    {
        // Show setup button
        if (GUI.Button(new Rect(10, 10, 200, 40), "Setup Pose Detection"))
        {
            SetupPoseDetectionSystem();
        }
        
        // Show status
        var manager = GameObject.Find("PoseDetectionManager");
        if (manager != null)
        {
            var hasClient = manager.GetComponent<PoseWebSocketClientOptimized>() != null;
            var hasController = manager.GetComponent<PoseInputController>() != null;
            
            GUI.Label(new Rect(10, 60, 300, 20), $"WebSocket Client: {(hasClient ? "✅" : "❌")}");
            GUI.Label(new Rect(10, 80, 300, 20), $"Input Controller: {(hasController ? "✅" : "❌")}");
            
            if (hasController)
            {
                var controller = manager.GetComponent<PoseInputController>();
                var hasCharacter = controller.CharacterController != null;
                GUI.Label(new Rect(10, 100, 300, 20), $"Character Controller: {(hasCharacter ? "✅" : "❌")}");
            }
        }
        
        // Show instructions
        GUI.Label(new Rect(10, 130, 500, 20), "Instructions:");
        GUI.Label(new Rect(10, 150, 500, 20), "1. Make sure Python server is running (webcam_server.py)");
        GUI.Label(new Rect(10, 170, 500, 20), "2. Click 'Setup Pose Detection' button");
        GUI.Label(new Rect(10, 190, 500, 20), "3. Watch the console for connection status");
        GUI.Label(new Rect(10, 210, 500, 20), "4. Make gestures in front of your webcam!");
    }
}
