/*
 * Unity Pose Detection Setup Verifier
 * Add this to any GameObject to verify pose detection is properly configured
 */

using UnityEngine;
using PoseDetection;

public class PoseDetectionVerifier : MonoBehaviour
{
    [Header("Verification")]
    [SerializeField] private bool runOnStart = true;
    
    void Start()
    {
        if (runOnStart)
        {
            VerifySetup();
        }
    }
    
    [ContextMenu("Verify Pose Detection Setup")]
    public void VerifySetup()
    {
        Debug.Log("🔍 === POSE DETECTION SETUP VERIFICATION ===");
        
        // Check for PoseWebSocketClient
        PoseWebSocketClient wsClient = FindObjectOfType<PoseWebSocketClient>();
        if (wsClient != null)
        {
            Debug.Log($"✅ PoseWebSocketClient found on: {wsClient.gameObject.name}");
            Debug.Log($"📡 Server URL: {wsClient.ServerUrl}");
            Debug.Log($"🔗 Is Connected: {wsClient.IsConnected}");
        }
        else
        {
            Debug.LogError("❌ PoseWebSocketClient NOT FOUND in scene!");
            Debug.LogError("   Add PoseDetectionSetup component to a GameObject");
        }
        
        // Check for PoseInputController
        PoseInputController inputController = FindObjectOfType<PoseInputController>();
        if (inputController != null)
        {
            Debug.Log($"✅ PoseInputController found on: {inputController.gameObject.name}");
            
            // Check if character controller is properly connected
            if (inputController.CharacterController != null)
            {
                Debug.Log($"✅ CharacterInputController connected: {inputController.CharacterController.gameObject.name}");
            }
            else
            {
                Debug.LogError("❌ CharacterInputController NOT CONNECTED to PoseInputController!");
                Debug.LogError("   Try running PoseDetectionSetup or manually assign in inspector");
            }
        }
        else
        {
            Debug.LogError("❌ PoseInputController NOT FOUND in scene!");
        }
        
        // Check for CharacterInputController in scene
        CharacterInputController[] allCharControllers = FindObjectsOfType<CharacterInputController>();
        if (allCharControllers.Length > 0)
        {
            Debug.Log($"✅ Found {allCharControllers.Length} CharacterInputController(s) in scene:");
            foreach (var controller in allCharControllers)
            {
                Debug.Log($"   📍 {controller.gameObject.name} (Active: {controller.gameObject.activeInHierarchy})");
            }
        }
        else
        {
            Debug.LogError("❌ NO CharacterInputController found in scene!");
            Debug.LogError("   Make sure you're using the Unity Endless Runner Sample Game scene");
            Debug.LogError("   The character prefab should have a CharacterInputController component");
        }
        
        // Check for PoseDetectionSetup
        PoseDetectionSetup setup = FindObjectOfType<PoseDetectionSetup>();
        if (setup != null)
        {
            Debug.Log($"✅ PoseDetectionSetup found on: {setup.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ PoseDetectionSetup not found (manual setup detected)");
        }
        
        Debug.Log("🔍 === VERIFICATION COMPLETE ===");
    }
}
