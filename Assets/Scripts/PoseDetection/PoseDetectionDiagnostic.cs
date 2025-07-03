using UnityEngine;
using PoseDetection;

/// <summary>
/// Simple diagnostic script to help identify connection issues between 
/// pose detection and character control
/// </summary>
public class PoseDetectionDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [SerializeField] private bool enableVerboseLogging = true;
    [SerializeField] private bool testCharacterControllerDirectly = false;
    
    private CharacterInputController characterController;
    private PoseInputController poseInputController;
    private PoseWebSocketClientOptimized webSocketClient;
    
    void Start()
    {
        if (enableVerboseLogging)
            Debug.Log("🔍 Starting Pose Detection Diagnostic...");
        
        // Find all the components
        FindComponents();
        
        // Run diagnostics
        RunDiagnostics();
        
        // Set up test key controls
        if (testCharacterControllerDirectly)
        {
            Debug.Log("🎮 Test Controls Enabled:");
            Debug.Log("  - Press 'T' to test Jump");
            Debug.Log("  - Press 'G' to test Slide");
            Debug.Log("  - Press 'F' to test Left Lane");
            Debug.Log("  - Press 'H' to test Right Lane");
        }
    }
    
    void Update()
    {
        // Test character controller directly with key presses
        if (testCharacterControllerDirectly && characterController != null)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("🧪 Testing Jump directly...");
                characterController.Jump();
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                Debug.Log("🧪 Testing Slide directly...");
                characterController.Slide();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("🧪 Testing Left Lane directly...");
                characterController.ChangeLane(-1);
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log("🧪 Testing Right Lane directly...");
                characterController.ChangeLane(1);
            }
        }
    }
    
    void FindComponents()
    {
        // Find CharacterInputController
        characterController = FindObjectOfType<CharacterInputController>();
        if (characterController != null)
        {
            Debug.Log($"✅ Found CharacterInputController on: {characterController.gameObject.name}");
        }
        else
        {
            Debug.LogError("❌ CharacterInputController not found in scene!");
        }
        
        // Find PoseInputController
        poseInputController = FindObjectOfType<PoseInputController>();
        if (poseInputController != null)
        {
            Debug.Log($"✅ Found PoseInputController on: {poseInputController.gameObject.name}");
        }
        else
        {
            Debug.LogError("❌ PoseInputController not found in scene!");
        }
        
        // Find WebSocket client
        webSocketClient = FindObjectOfType<PoseWebSocketClientOptimized>();
        if (webSocketClient != null)
        {
            Debug.Log($"✅ Found PoseWebSocketClientOptimized on: {webSocketClient.gameObject.name}");
        }
        else
        {
            Debug.LogError("❌ PoseWebSocketClientOptimized not found in scene!");
        }
    }
    
    void RunDiagnostics()
    {
        Debug.Log("📊 === POSE DETECTION DIAGNOSTICS ===");
        
        // Check if components are properly connected
        if (poseInputController != null && characterController != null)
        {
            // Use reflection to check if the character controller is assigned
            var controllerField = typeof(PoseInputController).GetField("characterController", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (controllerField != null)
            {
                var assignedController = controllerField.GetValue(poseInputController) as CharacterInputController;
                if (assignedController != null)
                {
                    Debug.Log($"✅ PoseInputController is connected to CharacterInputController: {assignedController.gameObject.name}");
                }
                else
                {
                    Debug.LogError("❌ PoseInputController's CharacterController field is not assigned!");
                }
            }
        }
        
        // Check character controller state
        if (characterController != null)
        {
            Debug.Log($"🎮 Character Controller State:");
            Debug.Log($"   - GameObject Active: {characterController.gameObject.activeInHierarchy}");
            Debug.Log($"   - Component Enabled: {characterController.enabled}");
            Debug.Log($"   - Is Jumping: {characterController.isJumping}");
            Debug.Log($"   - Is Sliding: {characterController.isSliding}");
        }
        
        // Test character controller methods exist
        if (characterController != null)
        {
            bool hasJump = characterController.GetType().GetMethod("Jump") != null;
            bool hasSlide = characterController.GetType().GetMethod("Slide") != null;
            bool hasChangeLane = characterController.GetType().GetMethod("ChangeLane") != null;
            
            Debug.Log($"🎮 Character Controller Methods:");
            Debug.Log($"   - Jump(): {(hasJump ? "✅" : "❌")}");
            Debug.Log($"   - Slide(): {(hasSlide ? "✅" : "❌")}");
            Debug.Log($"   - ChangeLane(): {(hasChangeLane ? "✅" : "❌")}");
        }
        
        Debug.Log("📊 === END DIAGNOSTICS ===");
    }
}
