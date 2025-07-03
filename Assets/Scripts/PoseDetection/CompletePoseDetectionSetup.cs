/*
 * Complete Pose Detection Setup and Troubleshooter
 * Automatically sets up all required components for pose detection in Unity
 */

using UnityEngine;
using PoseDetection;

namespace PoseDetection
{
    public class CompletePoseDetectionSetup : MonoBehaviour
    {
        [Header("Setup Options")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool useOptimizedClient = true;
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showDebugUI = true;
        
        [Header("Troubleshooting")]
        [SerializeField] private bool enableDiagnostics = true;
        
        void Start()
        {
            if (autoSetupOnStart)
            {
                SetupPoseDetectionComplete();
            }
        }
        
        [ContextMenu("Complete Pose Detection Setup")]
        public void SetupPoseDetectionComplete()
        {
            Debug.Log("🚀 === COMPLETE POSE DETECTION SETUP ===");
            
            // Step 1: Create or find pose detection manager
            GameObject poseManager = SetupPoseManager();
            
            // Step 2: Setup WebSocket client (regular or optimized)
            SetupWebSocketClient(poseManager);
            
            // Step 3: Setup input controller
            SetupInputController(poseManager);
            
            // Step 4: Connect to character controller
            ConnectCharacterController(poseManager);
            
            // Step 5: Setup additional components
            SetupAdditionalComponents();
            
            // Step 6: Run diagnostics
            if (enableDiagnostics)
            {
                RunDiagnostics();
            }
            
            // Step 7: Show final instructions
            ShowSetupInstructions();
            
            Debug.Log("✅ === POSE DETECTION SETUP COMPLETE ===");
        }
        
        private GameObject SetupPoseManager()
        {
            GameObject poseManager = GameObject.Find("PoseDetectionManager");
            if (poseManager == null)
            {
                poseManager = new GameObject("PoseDetectionManager");
                poseManager.transform.position = Vector3.zero;
                DontDestroyOnLoad(poseManager);
                Debug.Log("✅ Created PoseDetectionManager GameObject");
            }
            else
            {
                Debug.Log("ℹ️ Using existing PoseDetectionManager");
            }
            return poseManager;
        }
        
        private void SetupWebSocketClient(GameObject poseManager)
        {
            if (useOptimizedClient)
            {
                // Remove regular client if it exists
                PoseWebSocketClient regularClient = poseManager.GetComponent<PoseWebSocketClient>();
                if (regularClient != null)
                {
                    DestroyImmediate(regularClient);
                    Debug.Log("🔄 Removed regular WebSocket client for optimized version");
                }
                
                // Add optimized client
                PoseWebSocketClientOptimized optimizedClient = poseManager.GetComponent<PoseWebSocketClientOptimized>();
                if (optimizedClient == null)
                {
                    optimizedClient = poseManager.AddComponent<PoseWebSocketClientOptimized>();
                    Debug.Log("✅ Added PoseWebSocketClientOptimized component");
                }
                
                // Configure optimized client
                optimizedClient.SetPerformanceSettings(true, enableDebugLogging, 0.1f);
                Debug.Log("🚀 Configured optimized WebSocket client for low latency");
            }
            else
            {
                // Use regular client
                PoseWebSocketClient wsClient = poseManager.GetComponent<PoseWebSocketClient>();
                if (wsClient == null)
                {
                    wsClient = poseManager.AddComponent<PoseWebSocketClient>();
                    Debug.Log("✅ Added PoseWebSocketClient component");
                }
            }
        }
        
        private void SetupInputController(GameObject poseManager)
        {
            PoseInputController inputController = poseManager.GetComponent<PoseInputController>();
            if (inputController == null)
            {
                inputController = poseManager.AddComponent<PoseInputController>();
                Debug.Log("✅ Added PoseInputController component");
            }
            
            // Configure input controller using reflection if needed
            ConfigureInputController(inputController);
        }
        
        private void ConfigureInputController(PoseInputController inputController)
        {
            try
            {
                // Use reflection to set private fields
                var enableDebugField = typeof(PoseInputController).GetField("enableDebugLogs", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (enableDebugField != null)
                {
                    enableDebugField.SetValue(inputController, enableDebugLogging);
                }
                
                var showUIField = typeof(PoseInputController).GetField("showGestureUI", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (showUIField != null)
                {
                    showUIField.SetValue(inputController, showDebugUI);
                }
                
                // Reduce gesture cooldown for more responsive controls
                var cooldownField = typeof(PoseInputController).GetField("gestureCooldown", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (cooldownField != null)
                {
                    cooldownField.SetValue(inputController, 0.2f); // Faster than default 0.5s
                }
                
                // Lower confidence threshold for easier activation
                var thresholdField = typeof(PoseInputController).GetField("gestureThreshold", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (thresholdField != null)
                {
                    thresholdField.SetValue(inputController, 0.5f); // Lower than default 0.7f
                }
                
                Debug.Log("✅ Configured PoseInputController for optimal performance");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Could not configure input controller: {e.Message}");
            }
        }
        
        private void ConnectCharacterController(GameObject poseManager)
        {
            PoseInputController inputController = poseManager.GetComponent<PoseInputController>();
            if (inputController == null)
            {
                Debug.LogError("❌ PoseInputController not found!");
                return;
            }
            
            // Find character controller in multiple ways
            CharacterInputController characterController = FindCharacterController();
            
            if (characterController != null)
            {
                inputController.CharacterController = characterController;
                Debug.Log($"✅ Connected CharacterInputController '{characterController.name}' to PoseInputController");
                Debug.Log($"   Character is active: {characterController.gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.LogError("❌ CharacterInputController not found in scene!");
                Debug.LogError("   Make sure you're in the correct Unity Endless Runner scene");
                Debug.LogError("   The character GameObject should have a CharacterInputController component");
                
                // Try to find any potential character objects
                FindPotentialCharacterObjects();
            }
        }
        
        private CharacterInputController FindCharacterController()
        {
            // Method 1: Direct search
            CharacterInputController controller = FindObjectOfType<CharacterInputController>();
            if (controller != null)
            {
                Debug.Log($"📍 Found CharacterInputController via FindObjectOfType: {controller.name}");
                return controller;
            }
            
            // Method 2: Search including inactive objects
            CharacterInputController[] allControllers = Resources.FindObjectsOfTypeAll<CharacterInputController>();
            if (allControllers.Length > 0)
            {
                var activeController = System.Array.Find(allControllers, c => c.gameObject.scene.isLoaded);
                if (activeController != null)
                {
                    Debug.Log($"📍 Found CharacterInputController via Resources search: {activeController.name}");
                    return activeController;
                }
            }
            
            // Method 3: Search by common names
            string[] characterNames = { "Character", "Player", "Runner", "EndlessRunner" };
            foreach (string name in characterNames)
            {
                GameObject characterGO = GameObject.Find(name);
                if (characterGO != null)
                {
                    CharacterInputController charController = characterGO.GetComponent<CharacterInputController>();
                    if (charController != null)
                    {
                        Debug.Log($"📍 Found CharacterInputController by name search: {name}");
                        return charController;
                    }
                }
            }
            
            return null;
        }
        
        private void FindPotentialCharacterObjects()
        {
            Debug.Log("🔍 Searching for potential character objects...");
            
            // Look for objects with "Character" in name
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("character") || 
                    obj.name.ToLower().Contains("player") || 
                    obj.name.ToLower().Contains("runner"))
                {
                    Debug.Log($"   🎯 Potential character object: {obj.name} (Active: {obj.activeInHierarchy})");
                    
                    // Check what components it has
                    Component[] components = obj.GetComponents<Component>();
                    Debug.Log($"      Components: {string.Join(", ", System.Array.ConvertAll(components, c => c.GetType().Name))}");
                }
            }
        }
        
        private void SetupAdditionalComponents()
        {
            // Add verifier for troubleshooting
            PoseDetectionVerifier verifier = FindObjectOfType<PoseDetectionVerifier>();
            if (verifier == null)
            {
                GameObject verifierGO = new GameObject("PoseDetectionVerifier");
                verifier = verifierGO.AddComponent<PoseDetectionVerifier>();
                Debug.Log("✅ Added PoseDetectionVerifier for troubleshooting");
            }
        }
        
        private void RunDiagnostics()
        {
            Debug.Log("🔍 === RUNNING DIAGNOSTICS ===");
            
            // Check WebSocket client
            if (useOptimizedClient)
            {
                PoseWebSocketClientOptimized optimizedClient = FindObjectOfType<PoseWebSocketClientOptimized>();
                if (optimizedClient != null)
                {
                    Debug.Log($"✅ PoseWebSocketClientOptimized: {optimizedClient.gameObject.name}");
                    Debug.Log($"   Connected: {optimizedClient.IsConnected}");
                    Debug.Log($"   Server URL: {optimizedClient.ServerUrl}");
                }
                else
                {
                    Debug.LogError("❌ PoseWebSocketClientOptimized not found!");
                }
            }
            else
            {
                PoseWebSocketClient wsClient = FindObjectOfType<PoseWebSocketClient>();
                if (wsClient != null)
                {
                    Debug.Log($"✅ PoseWebSocketClient: {wsClient.gameObject.name}");
                    Debug.Log($"   Connected: {wsClient.IsConnected}");
                    Debug.Log($"   Server URL: {wsClient.ServerUrl}");
                }
                else
                {
                    Debug.LogError("❌ PoseWebSocketClient not found!");
                }
            }
            
            // Check input controller
            PoseInputController inputController = FindObjectOfType<PoseInputController>();
            if (inputController != null)
            {
                Debug.Log($"✅ PoseInputController: {inputController.gameObject.name}");
                Debug.Log($"   Character connected: {inputController.CharacterController != null}");
                if (inputController.CharacterController != null)
                {
                    Debug.Log($"   Character name: {inputController.CharacterController.name}");
                    Debug.Log($"   Character active: {inputController.CharacterController.gameObject.activeInHierarchy}");
                }
            }
            else
            {
                Debug.LogError("❌ PoseInputController not found!");
            }
            
            Debug.Log("🔍 === DIAGNOSTICS COMPLETE ===");
        }
        
        private void ShowSetupInstructions()
        {
            Debug.Log("📋 === SETUP INSTRUCTIONS ===");
            Debug.Log("1. Start Python server:");
            Debug.Log("   cd PoseDetection");
            Debug.Log("   python webcam_server.py");
            Debug.Log("");
            Debug.Log("2. Look for webcam window and 'Unity Clients: 1' when Unity connects");
            Debug.Log("");
            Debug.Log("3. Test gestures in front of webcam:");
            Debug.Log("   🦘 Head Up = Jump");
            Debug.Log("   ⬇️ Head Down = Slide");
            Debug.Log("   ⬅️ Left Hand Up = Move Left");
            Debug.Log("   ➡️ Right Hand Up = Move Right");
            Debug.Log("");
            Debug.Log("4. Check Unity Console for:");
            Debug.Log("   'PoseInputController: Received gesture...'");
            Debug.Log("   'PoseInputController: Successfully executed...'");
            Debug.Log("");
            Debug.Log("📋 === TROUBLESHOOTING ===");
            Debug.Log("• If no connection: Check if Python server is running on port 8765");
            Debug.Log("• If gestures detected but no game response: Check CharacterInputController connection");
            Debug.Log("• If no gesture detection: Calibrate by standing still for 3 seconds");
            Debug.Log("• Use 'P' key in Python webcam window for performance stats");
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 200));
            
            GUILayout.Label("🎮 Pose Detection Setup", GUI.skin.box);
            
            if (GUILayout.Button("Complete Setup"))
            {
                SetupPoseDetectionComplete();
            }
            
            if (GUILayout.Button("Run Diagnostics"))
            {
                RunDiagnostics();
            }
            
            if (GUILayout.Button("Test Character Connection"))
            {
                TestCharacterConnection();
            }
            
            // Status indicators
            var wsClient = useOptimizedClient ? 
                (MonoBehaviour)FindObjectOfType<PoseWebSocketClientOptimized>() : 
                (MonoBehaviour)FindObjectOfType<PoseWebSocketClient>();
            
            if (wsClient != null)
            {
                bool isConnected = useOptimizedClient ? 
                    ((PoseWebSocketClientOptimized)wsClient).IsConnected : 
                    ((PoseWebSocketClient)wsClient).IsConnected;
                
                string status = isConnected ? "🟢 Connected" : "🔴 Disconnected";
                GUILayout.Label($"WebSocket: {status}");
            }
            else
            {
                GUILayout.Label("WebSocket: ❌ Not Found");
            }
            
            var inputController = FindObjectOfType<PoseInputController>();
            if (inputController != null && inputController.CharacterController != null)
            {
                GUILayout.Label("Character: ✅ Connected");
            }
            else
            {
                GUILayout.Label("Character: ❌ Not Connected");
            }
            
            GUILayout.EndArea();
        }
        
        private void TestCharacterConnection()
        {
            var inputController = FindObjectOfType<PoseInputController>();
            if (inputController != null)
            {
                // Manually trigger a test gesture
                inputController.TriggerTestGesture("jump", 1.0f);
                Debug.Log("🧪 Triggered test jump gesture");
            }
            else
            {
                Debug.LogError("❌ Cannot test - PoseInputController not found");
            }
        }
    }
}
