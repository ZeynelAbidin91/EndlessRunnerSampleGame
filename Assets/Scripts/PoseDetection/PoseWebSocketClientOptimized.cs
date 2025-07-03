/*
 * Optimized Pose WebSocket Client for Unity Endless Runner Sample Game
 * High-performance version with minimal latency for gesture communication
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;

namespace PoseDetection
{
    /// <summary>
    /// Optimized WebSocket client for high-performance gesture communication
    /// </summary>
    public class PoseWebSocketClientOptimized : MonoBehaviour
    {
        [Header("Connection Settings")]
        [SerializeField] private string serverUrl = "ws://localhost:8765";
        [SerializeField] private float reconnectDelay = 3f;
        [SerializeField] private bool autoReconnect = true;
        
        [Header("Performance Settings")]
        [SerializeField] private bool enableFastMode = true;
        [SerializeField] private float maxProcessingInterval = 0.01f; // 10ms max delay
        [SerializeField] private bool enableDebugLogs = false;
        
        // Performance optimization fields
        private float lastMessageTime = 0f;
        private int processedMessageCount = 0;
        private Queue<string> messageQueue = new Queue<string>();
        private const int MAX_QUEUE_SIZE = 10;
        
        // Connection state
        private WebSocket websocket;
        private bool isConnecting = false;
        private bool shouldReconnect = true;
        
        // Events
        public static event Action<GestureData> OnGestureReceived;
        public static event Action<bool> OnConnectionStatusChanged;
        
        private void Start()
        {
            ConnectToServer();
        }
        
        private void Update()
        {
            if (enableFastMode)
            {
                // Process WebSocket messages immediately without throttling
                websocket?.DispatchMessageQueue();
                
                // Process queued messages
                ProcessQueuedMessages();
            }
            else
            {
                // Standard throttled processing
                if (Time.time - lastMessageTime >= maxProcessingInterval)
                {
                    websocket?.DispatchMessageQueue();
                    ProcessQueuedMessages();
                    lastMessageTime = Time.time;
                }
            }
        }
        
        private async void ConnectToServer()
        {
            if (isConnecting || websocket != null)
                return;
                
            isConnecting = true;
            
            try
            {
                websocket = new WebSocket(serverUrl);
                
                websocket.OnOpen += OnWebSocketOpen;
                websocket.OnMessage += OnWebSocketMessage;
                websocket.OnError += OnWebSocketError;
                websocket.OnClose += OnWebSocketClose;
                
                await websocket.Connect();
            }
            catch (Exception e)
            {
                if (enableDebugLogs)
                    Debug.LogError($"PoseWebSocketClientOptimized: Connection failed: {e.Message}");
                
                isConnecting = false;
                
                if (autoReconnect && shouldReconnect)
                {
                    StartCoroutine(ReconnectCoroutine());
                }
            }
        }
        
        private void OnWebSocketOpen()
        {
            isConnecting = false;
            
            if (enableDebugLogs)
            {
                Debug.Log("✅ PoseWebSocketClientOptimized: Connected with high-performance mode!");
                Debug.Log("🎮 Gesture detection is active - low latency enabled!");
            }
            
            OnConnectionStatusChanged?.Invoke(true);
        }
        
        private async void OnWebSocketMessage(byte[] data)
        {
            try
            {
                string message = System.Text.Encoding.UTF8.GetString(data);
                
                if (enableFastMode)
                {
                    // Immediate processing for fast mode
                    ProcessMessageImmediate(message);
                }
                else
                {
                    // Queue message for throttled processing
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(message);
                        if (messageQueue.Count > MAX_QUEUE_SIZE)
                        {
                            messageQueue.Dequeue(); // Drop old messages
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (enableDebugLogs)
                    Debug.LogError($"PoseWebSocketClientOptimized: Message processing error: {e.Message}");
            }
        }
        
        private void ProcessMessageImmediate(string message)
        {
            try
            {
                // Fast path for gesture messages (most common)
                if (message.Contains("\"type\":\"gesture\""))
                {
                    ProcessGestureMessageFast(message);
                    return;
                }
                
                // Fast path for common message types
                if (message.Contains("\"type\":\"connected\""))
                {
                    if (enableDebugLogs)
                        Debug.Log("PoseWebSocketClientOptimized: Connected to optimized server");
                    OnConnectionStatusChanged?.Invoke(true);
                    return;
                }
                
                if (message.Contains("\"type\":\"pong\""))
                {
                    // Ignore pong messages for performance
                    return;
                }
                
                // Fallback for other messages
                ProcessStandardMessage(message);
            }
            catch (Exception e)
            {
                if (enableDebugLogs)
                    Debug.LogError($"Immediate processing error: {e.Message}");
            }
        }
        
        private void ProcessGestureMessageFast(string message)
        {
            try
            {
                // Fixed timestamp parsing
                string fixedMessage = FixTimestampFormat(message);
                var gestureData = JsonConvert.DeserializeObject<GestureData>(fixedMessage);
                
                // Immediate gesture processing - no buffering for maximum responsiveness
                OnGestureReceived?.Invoke(gestureData);
                
                processedMessageCount++;
                
                if (enableDebugLogs && processedMessageCount % 20 == 0)
                {
                    Debug.Log($"🎮 Processed {processedMessageCount} gestures | Current: {gestureData.gesture}");
                }
            }
            catch (Exception e)
            {
                if (enableDebugLogs)
                    Debug.LogError($"Fast gesture processing error: {e.Message}\nMessage: {message}");
            }
        }
        
        private void ProcessStandardMessage(string message)
        {
            try
            {
                // Handle other message types here if needed
                if (enableDebugLogs)
                    Debug.Log($"PoseWebSocketClientOptimized: Received: {message}");
            }
            catch (Exception e)
            {
                if (enableDebugLogs)
                    Debug.LogError($"Standard message processing error: {e.Message}");
            }
        }
        
        private void ProcessQueuedMessages()
        {
            lock (messageQueue)
            {
                int processed = 0;
                while (messageQueue.Count > 0 && processed < 5) // Process max 5 per frame
                {
                    string message = messageQueue.Dequeue();
                    ProcessMessageImmediate(message);
                    processed++;
                }
            }
        }
        
        private string FixTimestampFormat(string jsonMessage)
        {
            try
            {
                // Use regex to find and convert ISO timestamp to Unix timestamp
                var timestampRegex = new System.Text.RegularExpressions.Regex(
                    @"""timestamp""\s*:\s*""([^""]+)""");
                
                var match = timestampRegex.Match(jsonMessage);
                if (match.Success)
                {
                    string isoTimestamp = match.Groups[1].Value;
                    if (System.DateTime.TryParse(isoTimestamp, out System.DateTime dt))
                    {
                        // Convert to Unix timestamp
                        var epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
                        float unixTimestamp = (float)(dt.ToUniversalTime() - epoch).TotalSeconds;
                        return timestampRegex.Replace(jsonMessage, $@"""timestamp"":{unixTimestamp}");
                    }
                }
                
                return jsonMessage;
            }
            catch (System.Exception ex)
            {
                if (enableDebugLogs)
                    Debug.LogWarning($"PoseWebSocketClientOptimized: Timestamp conversion failed: {ex.Message}");
                return jsonMessage; // Return original if preprocessing fails
            }
        }
        
        private void OnWebSocketError(string error)
        {
            if (enableDebugLogs)
                Debug.LogError($"PoseWebSocketClientOptimized: WebSocket error: {error}");
            
            isConnecting = false;
            OnConnectionStatusChanged?.Invoke(false);
            
            if (autoReconnect && shouldReconnect)
            {
                StartCoroutine(ReconnectCoroutine());
            }
        }
        
        private void OnWebSocketClose(WebSocketCloseCode closeCode)
        {
            if (enableDebugLogs)
                Debug.Log($"PoseWebSocketClientOptimized: Connection closed: {closeCode}");
            
            isConnecting = false;
            OnConnectionStatusChanged?.Invoke(false);
            
            if (autoReconnect && shouldReconnect && closeCode != WebSocketCloseCode.Normal)
            {
                StartCoroutine(ReconnectCoroutine());
            }
        }
        
        private IEnumerator ReconnectCoroutine()
        {
            if (enableDebugLogs)
                Debug.Log($"PoseWebSocketClientOptimized: Reconnecting in {reconnectDelay} seconds...");
                
            yield return new WaitForSeconds(reconnectDelay);
            
            if (shouldReconnect)
            {
                ConnectToServer();
            }
        }
        
        public async void CloseConnection()
        {
            if (websocket != null)
            {
                shouldReconnect = false;
                await websocket.Close();
                websocket = null;
            }
        }
        
        private void OnDestroy()
        {
            CloseConnection();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                CloseConnection();
            }
            else if (autoReconnect)
            {
                ConnectToServer();
            }
        }
        
        /// <summary>
        /// Configure performance settings at runtime
        /// </summary>
        public void SetPerformanceSettings(bool fastMode, bool debugLogs, float maxInterval = 0.01f)
        {
            enableFastMode = fastMode;
            enableDebugLogs = debugLogs;
            maxProcessingInterval = maxInterval;
            
            if (enableDebugLogs)
            {
                Debug.Log($"PoseWebSocketClientOptimized: Performance settings updated - Fast: {fastMode}, Debug: {debugLogs}, Interval: {maxInterval}s");
            }
        }
        
        /// <summary>
        /// Get current performance metrics
        /// </summary>
        public (int messageCount, bool fastMode, float interval) GetPerformanceMetrics()
        {
            return (processedMessageCount, enableFastMode, maxProcessingInterval);
        }
        
        /// <summary>
        /// Reset performance counters
        /// </summary>
        public void ResetPerformanceCounters()
        {
            processedMessageCount = 0;
            lastMessageTime = 0f;
            
            if (enableDebugLogs)
                Debug.Log("PoseWebSocketClientOptimized: Performance counters reset");
        }

        // Public properties for debugging
        public bool IsConnected => websocket?.State == WebSocketState.Open;
        public string ServerUrl => serverUrl;
        public int ProcessedMessageCount => processedMessageCount;
        public bool IsFastModeEnabled => enableFastMode;

        /// <summary>
        /// Update server URL and reconnect if needed
        /// </summary>
        public void SetServerUrl(string newUrl)
        {
            if (serverUrl != newUrl)
            {
                serverUrl = newUrl;
                
                if (IsConnected)
                {
                    CloseConnection();
                    ConnectToServer();
                }
                
                if (enableDebugLogs)
                    Debug.Log($"PoseWebSocketClientOptimized: Server URL updated to {newUrl}");
            }
        }
        
        /// <summary>
        /// Enable or disable auto-reconnect
        /// </summary>
        public void SetAutoReconnect(bool enabled)
        {
            autoReconnect = enabled;
            
            if (enableDebugLogs)
                Debug.Log($"PoseWebSocketClientOptimized: Auto-reconnect {(enabled ? "enabled" : "disabled")}");
        }
    }
}
