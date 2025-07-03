/*
 * Simple WebSocket Connection Test
 * This script tests the WebSocket connection to the Python server
 * without requiring the full game setup
 */

using System.Collections;
using UnityEngine;
using NativeWebSocket;

public class WebSocketConnectionTest : MonoBehaviour
{
    private WebSocket websocket;
    private bool isConnected = false;
    
    [Header("Test Settings")]
    public string serverUrl = "ws://localhost:8765";
    public bool connectOnStart = true;
    
    async void Start()
    {
        if (connectOnStart)
        {
            await ConnectToServer();
        }
    }
    
    async System.Threading.Tasks.Task ConnectToServer()
    {
        Debug.Log("🔗 Attempting to connect to pose detection server...");
        
        try
        {
            websocket = new WebSocket(serverUrl);
            
            websocket.OnOpen += () => {
                Debug.Log("✅ Connected to pose detection server!");
                isConnected = true;
            };
            
            websocket.OnMessage += (bytes) => {
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log($"📩 Received: {message}");
            };
            
            websocket.OnError += (error) => {
                Debug.LogError($"❌ WebSocket Error: {error}");
            };
            
            websocket.OnClose += (code) => {
                Debug.Log($"🚪 Connection closed: {code}");
                isConnected = false;
            };
            
            await websocket.Connect();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Connection failed: {e.Message}");
        }
    }
    
    void Update()
    {
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
    }
    
    private void OnDestroy()
    {
        if (websocket != null)
        {
            websocket.Close();
        }
    }
    
    private void OnGUI()
    {
        // Show connection status
        string statusText = isConnected ? "🟢 Connected" : "🔴 Not Connected";
        GUI.Label(new Rect(10, 10, 300, 30), $"Server Status: {statusText}");
        
        // Show connection button
        if (!isConnected && GUI.Button(new Rect(10, 50, 150, 30), "Connect"))
        {
            _ = ConnectToServer();
        }
        
        // Show disconnect button
        if (isConnected && GUI.Button(new Rect(10, 90, 150, 30), "Disconnect"))
        {
            websocket?.Close();
        }
        
        // Show instructions
        GUI.Label(new Rect(10, 130, 500, 20), "Instructions:");
        GUI.Label(new Rect(10, 150, 500, 20), "1. Make sure Python server is running (webcam_server.py)");
        GUI.Label(new Rect(10, 170, 500, 20), "2. Click Connect to test WebSocket connection");
        GUI.Label(new Rect(10, 190, 500, 20), "3. Watch console for messages from pose detection");
        GUI.Label(new Rect(10, 210, 500, 20), "4. If connected, make gestures to see messages");
    }
}
