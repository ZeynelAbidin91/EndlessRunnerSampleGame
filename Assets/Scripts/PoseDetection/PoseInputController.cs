/*
 * Pose Input Controller for Unity Endless Runner Sample Game
 * Maps body gestures to game controls:
 * - Up (Jump): Jump gesture detection
 * - Down (Slide): Lean/crouch gesture detection  
 * - Left (Lane Change): Move left gesture detection
 * - Right (Lane Change): Move right gesture detection
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoseDetection
{
    /// <summary>
    /// Integrates pose detection with Unity Endless Runner Sample Game
    /// Translates body gestures into game input commands
    /// </summary>
    public class PoseInputController : MonoBehaviour
    {
        [Header("Game References")]
        [SerializeField] private CharacterInputController characterController;
        
        // Public property for external setup
        public CharacterInputController CharacterController 
        { 
            get => characterController; 
            set => characterController = value; 
        }
        
        [Header("Gesture Settings")]
        [SerializeField] private float gestureThreshold = 0.7f;
        [SerializeField] private float gestureCooldown = 0.5f;
        
        [Header("Input Smoothing")]
        [SerializeField] private bool enableInputSmoothing = true;
        [SerializeField] private float inputDelay = 0.1f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool showGestureUI = false;
        
        // Gesture timing management
        private float lastJumpTime = 0f;
        private float lastSlideTime = 0f;
        private float lastLaneChangeTime = 0f;
        
        // Input queue for smoothing
        private Queue<GestureInput> inputQueue = new Queue<GestureInput>();
        
        private struct GestureInput
        {
            public string gesture;
            public float timestamp;
            public float confidence;
        }
        
        // Events for other systems
        public static event Action<string, float> OnGestureDetected;
        public static event Action<string> OnGestureExecuted;

        private void Start()
        {
            InitializeComponents();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            ProcessInputQueue();
        }

        private void InitializeComponents()
        {
            // Auto-find CharacterInputController if not assigned
            if (characterController == null)
            {
                if (enableDebugLogs)
                    Debug.Log("PoseInputController: Searching for CharacterInputController...");
                
                // Try different search strategies
                characterController = FindObjectOfType<CharacterInputController>();
                
                // If still not found, try searching inactive objects too
                if (characterController == null)
                {
                    CharacterInputController[] allControllers = Resources.FindObjectsOfTypeAll<CharacterInputController>();
                    if (allControllers.Length > 0)
                    {
                        characterController = allControllers[0];
                        if (enableDebugLogs)
                            Debug.Log($"PoseInputController: Found CharacterInputController on inactive object: {characterController.name}");
                    }
                }
                
                // Last resort: try to find by name
                if (characterController == null)
                {
                    GameObject characterObj = GameObject.Find("Character");
                    if (characterObj != null)
                    {
                        characterController = characterObj.GetComponent<CharacterInputController>();
                        if (characterController != null && enableDebugLogs)
                            Debug.Log("PoseInputController: Found CharacterInputController by searching for 'Character' GameObject");
                    }
                }
            }

            if (characterController == null)
            {
                Debug.LogError("PoseInputController: CharacterInputController not found! Please ensure the Unity Endless Runner Sample Game is properly set up with a Character GameObject containing the CharacterInputController component.");
                Debug.LogError("PoseInputController: You can also manually assign the CharacterInputController in the inspector.");
                enabled = false;
                return;
            }

            if (enableDebugLogs)
                Debug.Log($"PoseInputController: Successfully found CharacterInputController on GameObject: {characterController.name}");
        }

        private void SubscribeToEvents()
        {
            // Subscribe to gesture events from both WebSocket clients (regular and optimized)
            PoseWebSocketClient.OnGestureReceived += HandleGestureReceived;
            PoseWebSocketClient.OnConnectionStatusChanged += HandleConnectionStatusChanged;
            
            // Also subscribe to optimized client events
            PoseWebSocketClientOptimized.OnGestureReceived += HandleOptimizedGestureReceived;
            PoseWebSocketClientOptimized.OnConnectionStatusChanged += HandleConnectionStatusChanged;
        }

        private void UnsubscribeFromEvents()
        {
            // Unsubscribe from events
            PoseWebSocketClient.OnGestureReceived -= HandleGestureReceived;
            PoseWebSocketClient.OnConnectionStatusChanged -= HandleConnectionStatusChanged;
            
            // Also unsubscribe from optimized client events
            PoseWebSocketClientOptimized.OnGestureReceived -= HandleOptimizedGestureReceived;
            PoseWebSocketClientOptimized.OnConnectionStatusChanged -= HandleConnectionStatusChanged;
        }

        private void HandleConnectionStatusChanged(bool isConnected)
        {
            if (enableDebugLogs)
                Debug.Log($"PoseInputController: Pose detection connection {(isConnected ? "established" : "lost")}");
        }

        private void HandleGestureReceived(GestureData gestureData)
        {
            if (gestureData == null || string.IsNullOrEmpty(gestureData.gesture))
                return;

            // Always log gesture reception for debugging
            if (enableDebugLogs)
                Debug.Log($"PoseInputController: Received gesture '{gestureData.gesture}' with confidence {gestureData.confidence:F2}");

            // Lower confidence threshold for testing - gesture detection is already good
            float effectiveThreshold = Math.Min(gestureThreshold, 0.5f);
            
            // Check confidence threshold
            if (gestureData.confidence < effectiveThreshold)
            {
                if (enableDebugLogs)
                    Debug.Log($"PoseInputController: Gesture '{gestureData.gesture}' below confidence threshold ({gestureData.confidence:F2} < {effectiveThreshold:F2})");
                return;
            }

            // Fire event for other systems
            OnGestureDetected?.Invoke(gestureData.gesture, gestureData.confidence);

            if (enableInputSmoothing)
            {
                // Add to input queue for smoothing
                inputQueue.Enqueue(new GestureInput
                {
                    gesture = gestureData.gesture,
                    timestamp = Time.time,
                    confidence = gestureData.confidence
                });
            }
            else
            {
                // Process immediately
                ProcessGesture(gestureData.gesture, gestureData.confidence);
            }
        }

        private void HandleOptimizedGestureReceived(GestureData gestureData)
        {
            if (gestureData == null || string.IsNullOrEmpty(gestureData.gesture))
                return;

            if (enableDebugLogs)
                Debug.Log($"PoseInputController: Received optimized gesture '{gestureData.gesture}' (conf: {gestureData.confidence:F2})");

            // Use the gesture data directly since it's already compatible
            HandleGestureReceived(gestureData);
        }

        private void ProcessInputQueue()
        {
            if (!enableInputSmoothing || inputQueue.Count == 0)
                return;

            // Process gestures that are old enough (past the input delay)
            while (inputQueue.Count > 0)
            {
                var input = inputQueue.Peek();
                if (Time.time - input.timestamp >= inputDelay)
                {
                    inputQueue.Dequeue();
                    ProcessGesture(input.gesture, input.confidence);
                }
                else
                {
                    break; // Remaining inputs are too recent
                }
            }
        }

        private void ProcessGesture(string gesture, float confidence)
        {
            if (enableDebugLogs)
                Debug.Log($"PoseInputController: Processing gesture '{gesture}' with confidence {confidence:F2}");
                
            if (characterController == null)
            {
                Debug.LogError("PoseInputController: CharacterController is null! Cannot process gesture.");
                return;
            }

            float currentTime = Time.time;
            
            // Map gestures to game controls based on your requirements:
            // Up button -> Jump, Down button -> Slide, Left/Right -> Lane changes
            
            switch (gesture.ToLower())
            {
                case "jump":
                case "up":
                    if (currentTime - lastJumpTime >= gestureCooldown)
                    {
                        if (enableDebugLogs)
                            Debug.Log("PoseInputController: Attempting to execute jump...");
                        ExecuteJump();
                        lastJumpTime = currentTime;
                    }
                    else if (enableDebugLogs)
                    {
                        Debug.Log($"PoseInputController: Jump on cooldown ({currentTime - lastJumpTime:F2}s < {gestureCooldown:F2}s)");
                    }
                    break;

                case "slide":
                case "crouch":
                case "lean":
                case "down":
                    if (currentTime - lastSlideTime >= gestureCooldown)
                    {
                        if (enableDebugLogs)
                            Debug.Log("PoseInputController: Attempting to execute slide...");
                        ExecuteSlide();
                        lastSlideTime = currentTime;
                    }
                    else if (enableDebugLogs)
                    {
                        Debug.Log($"PoseInputController: Slide on cooldown ({currentTime - lastSlideTime:F2}s < {gestureCooldown:F2}s)");
                    }
                    break;

                case "left":
                case "move_left":
                case "lane_left":
                case "shift_left":
                    if (currentTime - lastLaneChangeTime >= gestureCooldown)
                    {
                        if (enableDebugLogs)
                            Debug.Log("PoseInputController: Attempting to execute left lane change...");
                        ExecuteLaneChange(-1); // Move left
                        lastLaneChangeTime = currentTime;
                    }
                    else if (enableDebugLogs)
                    {
                        Debug.Log($"PoseInputController: Lane change on cooldown ({currentTime - lastLaneChangeTime:F2}s < {gestureCooldown:F2}s)");
                    }
                    break;

                case "right":
                case "move_right":
                case "lane_right":
                case "shift_right":
                    if (currentTime - lastLaneChangeTime >= gestureCooldown)
                    {
                        if (enableDebugLogs)
                            Debug.Log("PoseInputController: Attempting to execute right lane change...");
                        ExecuteLaneChange(1); // Move right
                        lastLaneChangeTime = currentTime;
                    }
                    else if (enableDebugLogs)
                    {
                        Debug.Log($"PoseInputController: Lane change on cooldown ({currentTime - lastLaneChangeTime:F2}s < {gestureCooldown:F2}s)");
                    }
                    break;

                default:
                    if (enableDebugLogs)
                        Debug.LogWarning($"PoseInputController: Unknown gesture: '{gesture}' - Available: jump, slide, left, right");
                    break;
            }
        }

        private void ExecuteJump()
        {
            if (characterController != null && characterController.gameObject.activeInHierarchy)
            {
                characterController.Jump();
                OnGestureExecuted?.Invoke("jump");
                
                if (enableDebugLogs)
                    Debug.Log("✅ PoseInputController: Successfully executed Jump");
            }
            else
            {
                if (enableDebugLogs)
                    Debug.LogError($"❌ PoseInputController: Cannot execute Jump - CharacterController: {(characterController != null ? "inactive" : "null")}");
            }
        }

        private void ExecuteSlide()
        {
            if (characterController != null && characterController.gameObject.activeInHierarchy)
            {
                characterController.Slide();
                OnGestureExecuted?.Invoke("slide");
                
                if (enableDebugLogs)
                    Debug.Log("✅ PoseInputController: Successfully executed Slide");
            }
            else
            {
                if (enableDebugLogs)
                    Debug.LogError($"❌ PoseInputController: Cannot execute Slide - CharacterController: {(characterController != null ? "inactive" : "null")}");
            }
        }

        private void ExecuteLaneChange(int direction)
        {
            if (characterController != null && characterController.gameObject.activeInHierarchy)
            {
                characterController.ChangeLane(direction);
                OnGestureExecuted?.Invoke($"lane_change_{(direction > 0 ? "right" : "left")}");
                
                if (enableDebugLogs)
                    Debug.Log($"✅ PoseInputController: Successfully executed Lane Change {(direction > 0 ? "Right" : "Left")}");
            }
            else
            {
                if (enableDebugLogs)
                    Debug.LogError($"❌ PoseInputController: Cannot execute Lane Change - CharacterController: {(characterController != null ? "inactive" : "null")}");
            }
        }

        // Debug visualization
        private void OnGUI()
        {
            if (!showGestureUI)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("🎮 Pose Input Controller", GUI.skin.box);
            GUILayout.Label($"Character Controller: {(characterController != null ? "✅ Connected" : "❌ Missing")}");
            
            var poseClient = FindObjectOfType<PoseWebSocketClient>();
            GUILayout.Label($"Pose Detection: {(poseClient != null && poseClient.IsConnected ? "✅ Connected" : "❌ Disconnected")}");
            
            GUILayout.Label($"Input Queue: {inputQueue.Count}");
            GUILayout.Label($"Confidence Threshold: {gestureThreshold:F2}");
            GUILayout.Label($"Gesture Cooldown: {gestureCooldown:F2}s");
            
            GUILayout.Space(10);
            GUILayout.Label("🎯 Controls:", GUI.skin.box);
            GUILayout.Label("🦘 Jump Gesture → Up/Jump");
            GUILayout.Label("⬇️ Lean/Crouch → Down/Slide");
            GUILayout.Label("⬅️ Move Left → Left Lane");
            GUILayout.Label("➡️ Move Right → Right Lane");
            
            GUILayout.EndArea();
        }

        #region Public API

        /// <summary>
        /// Manually trigger a gesture for testing
        /// </summary>
        public void TriggerTestGesture(string gesture, float confidence = 1.0f)
        {
            var testGesture = new GestureData
            {
                gesture = gesture,
                confidence = confidence,
                timestamp = Time.time
            };
            
            HandleGestureReceived(testGesture);
        }

        /// <summary>
        /// Enable or disable gesture input processing
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            this.enabled = enabled;
            
            if (!enabled)
            {
                inputQueue.Clear();
            }
            
            if (enableDebugLogs)
                Debug.Log($"PoseInputController: Input {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Update gesture confidence threshold
        /// </summary>
        public void SetGestureThreshold(float threshold)
        {
            gestureThreshold = Mathf.Clamp01(threshold);
            
            if (enableDebugLogs)
                Debug.Log($"PoseInputController: Threshold set to {gestureThreshold:F2}");
        }

        /// <summary>
        /// Update gesture cooldown time
        /// </summary>
        public void SetGestureCooldown(float cooldown)
        {
            gestureCooldown = Mathf.Max(0f, cooldown);
            
            if (enableDebugLogs)
                Debug.Log($"PoseInputController: Cooldown set to {gestureCooldown:F2}s");
        }

        /// <summary>
        /// Manually retry finding the CharacterInputController
        /// Useful if the character is instantiated after this component starts
        /// </summary>
        [ContextMenu("Retry Find Character Controller")]
        public void RetryFindCharacterController()
        {
            characterController = null;
            InitializeComponents();
        }

        /// <summary>
        /// Enable debug logging and UI settings
        /// </summary>
        public void SetDebugSettings(bool debugLogs, bool gestureUI = true)
        {
            enableDebugLogs = debugLogs;
            showGestureUI = gestureUI;
            
            if (enableDebugLogs)
            {
                Debug.Log($"PoseInputController: Debug settings updated - Logs: {debugLogs}, UI: {gestureUI}");
            }
        }

        #endregion
    }
}
