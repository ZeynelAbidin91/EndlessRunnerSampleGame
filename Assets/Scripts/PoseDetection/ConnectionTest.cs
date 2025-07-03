using UnityEngine;
using PoseDetection;

/// <summary>
/// Simple connection test script that logs every step of the connection process
/// </summary>
public class ConnectionTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool enableVerboseLogging = true;
    [SerializeField] private bool testGestureReception = true;
    
    private PoseWebSocketClientOptimized webSocketClient;
    private int receivedGestureCount = 0;
    
    void Start()
    {
        Debug.Log("🧪 CONNECTION TEST STARTING...");
        
        // Find or create WebSocket client
        webSocketClient = FindObjectOfType<PoseWebSocketClientOptimized>();
        if (webSocketClient == null)
        {
            Debug.Log("🔧 Creating WebSocket client...");
            GameObject clientObj = new GameObject("TestWebSocketClient");
            webSocketClient = clientObj.AddComponent<PoseWebSocketClientOptimized>();
            webSocketClient.SetPerformanceSettings(true, true, 0.01f);
        }
        
        // Subscribe to events
        if (testGestureReception)
        {
            PoseWebSocketClientOptimized.OnGestureReceived += OnGestureReceived;
            PoseWebSocketClientOptimized.OnConnectionStatusChanged += OnConnectionStatusChanged;
            Debug.Log("✅ Subscribed to gesture events");
        }
        
        Debug.Log("🎮 Connection test setup complete");
        Debug.Log("📡 Make sure Python server is running on ws://localhost:8765");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (testGestureReception)
        {
            PoseWebSocketClientOptimized.OnGestureReceived -= OnGestureReceived;
            PoseWebSocketClientOptimized.OnConnectionStatusChanged -= OnConnectionStatusChanged;
        }
    }
    
    private void OnGestureReceived(GestureData gestureData)
    {
        receivedGestureCount++;
        
        if (enableVerboseLogging)
        {
            Debug.Log($"🎭 GESTURE RECEIVED #{receivedGestureCount}:");
            Debug.Log($"   - Gesture: '{gestureData.gesture}'");
            Debug.Log($"   - Confidence: {gestureData.confidence:F2}");
            Debug.Log($"   - Timestamp: {gestureData.timestamp:F2}");
        }
        else
        {
            Debug.Log($"🎭 Gesture: {gestureData.gesture} (#{receivedGestureCount})");
        }
    }
    
    private void OnConnectionStatusChanged(bool isConnected)
    {
        if (isConnected)
        {
            Debug.Log("✅ CONNECTION TEST: WebSocket connected successfully!");
            Debug.Log("🎮 Make gestures in front of your camera to test...");
        }
        else
        {
            Debug.Log("❌ CONNECTION TEST: WebSocket disconnected");
        }
    }
    
    void OnGUI()
    {
        // Status display
        GUI.Label(new Rect(10, 10, 300, 20), "🧪 CONNECTION TEST STATUS");
        GUI.Label(new Rect(10, 30, 300, 20), $"WebSocket Client: {(webSocketClient != null ? "✅" : "❌")}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Gestures Received: {receivedGestureCount}");
        
        // Test instructions
        GUI.Label(new Rect(10, 80, 500, 20), "1. Start Python server (webcam_server.py)");
        GUI.Label(new Rect(10, 100, 500, 20), "2. Watch Unity console for connection messages");
        GUI.Label(new Rect(10, 120, 500, 20), "3. Make gestures in front of camera");
        GUI.Label(new Rect(10, 140, 500, 20), "4. Check console for gesture reception logs");
        
        // Manual test button
        if (GUI.Button(new Rect(10, 170, 150, 30), "Test Connection"))
        {
            Debug.Log("🔧 Manual connection test initiated...");
            if (webSocketClient != null)
            {
                Debug.Log("🎮 WebSocket client found - connection should happen automatically");
            }
            else
            {
                Debug.LogError("❌ WebSocket client not found!");
            }
        }
    }
}
