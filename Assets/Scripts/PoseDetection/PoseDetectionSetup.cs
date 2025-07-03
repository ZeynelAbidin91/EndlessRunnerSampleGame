/*
 * Pose Detection Setup Helper for Unity Endless Runner Sample Game
 * Helps set up pose detection integration in existing scenes
 */

using UnityEngine;
using UnityEditor;
using PoseDetection;

namespace PoseDetection
{
    /// <summary>
    /// Helper script to automatically set up pose detection in Unity Endless Runner scenes
    /// </summary>
    public class PoseDetectionSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool enableDebugUI = true;
        
        [Header("Manual Setup")]
        [Button("Setup Pose Detection")]
        public bool setupButton;

        private void Start()
        {
            if (setupOnStart)
            {
                SetupPoseDetection();
            }
        }

        [ContextMenu("Setup Pose Detection")]
        public void SetupPoseDetection()
        {
            Debug.Log("🎮 Setting up Pose Detection for Endless Runner...");

            // Find or create the pose detection manager
            GameObject poseManager = GameObject.Find("PoseDetectionManager");
            if (poseManager == null)
            {
                poseManager = new GameObject("PoseDetectionManager");
                Debug.Log("✅ Created PoseDetectionManager GameObject");
            }

            // Add WebSocket client if it doesn't exist (use optimized version)
            PoseWebSocketClientOptimized wsClient = poseManager.GetComponent<PoseWebSocketClientOptimized>();
            if (wsClient == null)
            {
                wsClient = poseManager.AddComponent<PoseWebSocketClientOptimized>();
                Debug.Log("✅ Added PoseWebSocketClientOptimized component");
            }

            // Add Input controller if it doesn't exist
            PoseInputController inputController = poseManager.GetComponent<PoseInputController>();
            if (inputController == null)
            {
                inputController = poseManager.AddComponent<PoseInputController>();
                Debug.Log("✅ Added PoseInputController component");
            }

            // Find and connect the CharacterInputController
            CharacterInputController characterController = FindObjectOfType<CharacterInputController>();
            if (characterController != null)
            {
                // Set the character controller using the public property
                inputController.CharacterController = characterController;
                Debug.Log($"✅ Connected CharacterInputController '{characterController.name}' to PoseInputController");
            }
            else
            {
                Debug.LogWarning("⚠️ CharacterInputController not found in scene. Please ensure the Unity Endless Runner Sample Game is properly loaded.");
                Debug.LogWarning("💡 Tip: Make sure you're running this in a scene with the character prefab instantiated.");
            }

            // Configure settings
            if (enableDebugUI)
            {
                // Enable debug settings using public properties if available, otherwise use reflection
                try 
                {
                    var showUIField = typeof(PoseInputController).GetField("showGestureUI", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (showUIField != null)
                    {
                        showUIField.SetValue(inputController, true);
                    }
                    
                    var debugField = typeof(PoseInputController).GetField("enableDebugLogs", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (debugField != null)
                    {
                        debugField.SetValue(inputController, true);
                    }
                    
                    Debug.Log("✅ Enabled debug UI and logging");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"⚠️ Could not set debug options: {e.Message}");
                }
            }

            Debug.Log("🎉 Pose Detection setup complete!");
            Debug.Log("📝 Next steps:");
            Debug.Log("   1. Start Python pose detection server: cd PoseDetection && python webcam_server.py");
            Debug.Log("   2. Press Play in Unity");
            Debug.Log("   3. Make gestures in front of your webcam!");
            Debug.Log("");
            Debug.Log("🎯 Gesture Controls:");
            Debug.Log("   🦘 Head Up → Character jumps");
            Debug.Log("   ⬇️ Head Down → Character slides");
            Debug.Log("   ⬅️ Left hand up → Character moves to left lane");
            Debug.Log("   ➡️ Right hand up → Character moves to right lane");
            Debug.Log("");
            Debug.Log("🔧 System Gestures:");
            Debug.Log("   🔄 T-pose (hold 1 sec) → Recalibrate pose detection");
            Debug.Log("   ❌ Cross hands above head (hold 1 sec) → Quit application");
        }

        private void OnGUI()
        {
            if (!setupOnStart)
            {
                if (GUI.Button(new Rect(10, Screen.height - 100, 200, 30), "Setup Pose Detection"))
                {
                    SetupPoseDetection();
                }
            }

            // Show connection status
            var wsClient = FindObjectOfType<PoseWebSocketClientOptimized>();
            if (wsClient != null)
            {
                string status = wsClient.IsConnected ? "🟢 Connected" : "🔴 Disconnected";
                GUI.Label(new Rect(10, Screen.height - 60, 300, 30), $"Pose Detection: {status}");
            }
        }
    }

    // Custom attribute for button display in inspector
    public class ButtonAttribute : PropertyAttribute
    {
        public string MethodName { get; }
        
        public ButtonAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ButtonAttribute buttonAttribute = (ButtonAttribute)attribute;
            
            if (GUI.Button(position, buttonAttribute.MethodName))
            {
                var target = property.serializedObject.targetObject;
                var method = target.GetType().GetMethod("SetupPoseDetection");
                method?.Invoke(target, null);
            }
        }
    }
#endif
}
