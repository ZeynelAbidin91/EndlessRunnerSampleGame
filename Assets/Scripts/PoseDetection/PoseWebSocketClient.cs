/*
 * Pose WebSocket Client for Unity Endless Runner Sample Game
 * Connects to Python pose detection service and receives gesture data
 */

using System;
using System.Collections;
using System.Text;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;

namespace PoseDetection
{
    /// <summary>
    /// Data structure for gesture information received from pose detection
    /// </summary>
    [Serializable]
    public class GestureData
    {
        public string gesture;
        public float confidence;
        public float timestamp;
    }

    /// <summary>
    /// WebSocket client that connects to the Python pose detection service
    /// </summary>
    public class PoseWebSocketClient : MonoBehaviour
    {
        [Header("Connection Settings")]
        [SerializeField] private string serverUrl = "ws://localhost:8765";
        [SerializeField] private float reconnectDelay = 3f;
        [SerializeField] private bool autoReconnect = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Events
        public static event Action<GestureData> OnGestureReceived;
        public static event Action<bool> OnConnectionStatusChanged;
        
        // WebSocket connection
        private WebSocket websocket;
        private bool isConnecting = false;
        private bool shouldReconnect = true;
        
        // Connection state
        public bool IsConnected => websocket?.State == WebSocketState.Open;
        public string ServerUrl => serverUrl;

        private void Start()
        {
            // Start connection on game start
            Connect();
        }

        private void Update()
        {
            // Dispatch WebSocket messages on main thread
            if (websocket != null)
            {
                #if !UNITY_WEBGL || UNITY_EDITOR
                websocket.DispatchMessageQueue();
                #endif
            }
        }

        private void OnDestroy()
        {
            shouldReconnect = false;
            CloseConnection();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                CloseConnection();
            }
            else if (shouldReconnect)
            {
                Connect();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                CloseConnection();
            }
            else if (shouldReconnect)
            {
                Connect();
            }
        }

        /// <summary>
        /// Connect to the pose detection WebSocket server
        /// </summary>
        public async void Connect()
        {
            if (isConnecting || IsConnected)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("PoseWebSocketClient: Already connected or connecting");
                return;
            }

            try
            {
                isConnecting = true;
                
                if (enableDebugLogs)
                {
                    Debug.Log($"🔌 PoseWebSocketClient: Connecting to {serverUrl}");
                    Debug.Log($"🌐 Make sure WebSocket server is running on {serverUrl}");
                }

                websocket = new WebSocket(serverUrl);

                websocket.OnOpen += OnWebSocketOpen;
                websocket.OnMessage += OnWebSocketMessage;
                websocket.OnError += OnWebSocketError;
                websocket.OnClose += OnWebSocketClose;

                await websocket.Connect();
            }
            catch (Exception e)
            {
                Debug.LogError($"PoseWebSocketClient: Connection failed: {e.Message}");
                isConnecting = false;
                
                if (autoReconnect && shouldReconnect)
                {
                    StartCoroutine(ReconnectCoroutine());
                }
            }
        }

        /// <summary>
        /// Close the WebSocket connection
        /// </summary>
        public async void CloseConnection()
        {
            if (websocket != null)
            {
                shouldReconnect = false;
                await websocket.Close();
                websocket = null;
            }
        }

        private void OnWebSocketOpen()
        {
            isConnecting = false;
            
            if (enableDebugLogs)
            {
                Debug.Log("✅ PoseWebSocketClient: Connected successfully!");
                Debug.Log("🎮 Pose detection is now active - move your body to control the game!");
            }
            
            OnConnectionStatusChanged?.Invoke(true);
        }

        private void OnWebSocketMessage(byte[] data)
        {
            try
            {
                string message = Encoding.UTF8.GetString(data);
                
                if (enableDebugLogs)
                    Debug.Log($"PoseWebSocketClient: Received: {message}");

                // Pre-process the message to handle timestamp format issues
                string processedMessage = FixTimestampFormat(message);
                
                // Parse the gesture data
                var gestureData = JsonConvert.DeserializeObject<GestureData>(processedMessage);
                
                if (gestureData != null && !string.IsNullOrEmpty(gestureData.gesture))
                {
                    // Fire event for other components to handle
                    OnGestureReceived?.Invoke(gestureData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"PoseWebSocketClient: Error parsing message: {e.Message}");
                Debug.LogError($"PoseWebSocketClient: Raw message: {Encoding.UTF8.GetString(data)}");
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
                Debug.LogWarning($"PoseWebSocketClient: Timestamp conversion failed: {ex.Message}");
                return jsonMessage; // Return original if preprocessing fails
            }
        }

        private void OnWebSocketError(string error)
        {
            Debug.LogError($"PoseWebSocketClient: WebSocket error: {error}");
            
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
                Debug.Log($"PoseWebSocketClient: Connection closed: {closeCode}");
            
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
                Debug.Log($"PoseWebSocketClient: Reconnecting in {reconnectDelay} seconds...");
            
            yield return new WaitForSeconds(reconnectDelay);
            
            if (shouldReconnect)
            {
                Connect();
            }
        }

        /// <summary>
        /// Send a test message to the server
        /// </summary>
        public async void SendTestMessage()
        {
            if (!IsConnected)
            {
                Debug.LogWarning("PoseWebSocketClient: Not connected, cannot send message");
                return;
            }

            var testMessage = new
            {
                type = "test",
                timestamp = Time.time,
                message = "Test from Unity Endless Runner"
            };

            string json = JsonConvert.SerializeObject(testMessage);
            await websocket.SendText(json);
            
            if (enableDebugLogs)
                Debug.Log($"PoseWebSocketClient: Sent test message: {json}");
        }

        /// <summary>
        /// Update server URL (requires reconnection)
        /// </summary>
        public void SetServerUrl(string newUrl)
        {
            if (serverUrl != newUrl)
            {
                serverUrl = newUrl;
                
                if (IsConnected)
                {
                    CloseConnection();
                    Connect();
                }
            }
        }

        /// <summary>
        /// Enable or disable auto-reconnect
        /// </summary>
        public void SetAutoReconnect(bool enabled)
        {
            autoReconnect = enabled;
        }
    }
}
