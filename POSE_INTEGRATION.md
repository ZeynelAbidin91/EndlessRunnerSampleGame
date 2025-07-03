# Pose Detection Integration for Unity Endless Runner Sample Game

## 🎯 Overview

This integration adds webcam-controlled pose detection to the Unity Endless Runner Sample Game. Players can control their character using natural body movements:

- **🦘 Jump Gesture** → Character jumps over obstacles
- **⬇️ Lean/Crouch** → Character slides under obstacles  
- **⬅️ Move Left** → Character changes to left lane
- **➡️ Move Right** → Character changes to right lane

## 🚀 Quick Setup

### 1. Prerequisites
- Unity 2022.3+ with the Endless Runner Sample Game imported
- Python 3.8+ with webcam access
- Windows 10+, macOS 10.15+, or Ubuntu 18.04+

### 2. Install Python Dependencies
```bash
cd ../PoseDetection
pip install -r requirements.txt
```

### 3. Unity Setup
1. Open the `EndlessRunnerSampleGame` project in Unity Hub
2. Let Unity import the new pose detection scripts
3. The required packages (Newtonsoft JSON, NativeWebSocket) will auto-install

### 4. Scene Setup
1. Open any endless runner scene (e.g., `MainScene`)
2. Create an empty GameObject named "PoseDetectionManager"
3. Add the `PoseDetectionSetup` component to it
4. Click "Setup Pose Detection" in the inspector OR let it auto-setup on play

Alternative: Add this script to any existing GameObject and it will auto-setup pose detection when the game starts.

### 5. Start Playing!
1. **Start the pose detection server**:
   ```bash
   cd ../PoseDetection
   python websocket_server.py
   ```

2. **Press Play in Unity**

3. **Calibrate**: Stand in T-pose when prompted

4. **Play with gestures**:
   - Stand in front of your webcam
   - Make jump, lean, and movement gestures
   - Watch your character respond in real-time!

## 🎮 Gesture Controls

| Body Movement | Game Action | Description |
|---------------|-------------|-------------|
| **Jump** | Character Jumps | Crouch down, then stand up quickly |
| **Lean Left/Right** | Character Slides | Lean your torso to either side |
| **Step Left** | Move to Left Lane | Step to the left with your feet |
| **Step Right** | Move to Right Lane | Step to the right with your feet |

## ⚙️ Configuration

### Gesture Sensitivity
Edit `../PoseDetection/config.json`:
```json
{
  "gesture_thresholds": {
    "jump_crouch_threshold": 0.15,
    "jump_speed_threshold": 0.3,
    "lean_angle_threshold": 15.0,
    "lane_change_threshold": 0.2,
    "smoothing_factor": 0.7,
    "gesture_cooldown": 0.5
  }
}
```

### Unity Settings
In the `PoseInputController` component:
- **Gesture Threshold**: Minimum confidence for gesture recognition (0.0-1.0)
- **Gesture Cooldown**: Time between gestures to prevent rapid-fire (seconds)
- **Enable Input Smoothing**: Smooths input for more stable control
- **Show Gesture UI**: Display debug overlay showing connection status

## 🔧 Troubleshooting

### "Pose Detection: 🔴 Disconnected"
- Ensure the Python WebSocket server is running: `python websocket_server.py`
- Check that no firewall is blocking port 8765
- Try restarting both Unity and the Python server

### "CharacterInputController not found"
- Ensure you're using the EndlessRunnerSampleGame project
- Check that the scene contains a GameObject with `CharacterInputController` component
- Manually assign it in the `PoseInputController` component if needed

### Gestures not recognized
- Ensure good lighting and clear view of your full body
- Try recalibrating by restarting the pose detection server
- Lower the gesture threshold in PoseInputController settings
- Check webcam permissions

### High latency
- Close other applications using the webcam
- Ensure good lighting (poor lighting = slower processing)
- Lower webcam resolution if needed
- Check computer performance - pose detection is CPU intensive

## 🏗️ Technical Architecture

```
┌─────────────────────┐    WebSocket     ┌──────────────────────┐
│  Unity EndlessRunner │ ←─────────────→ │  Python PoseDetection │
│                     │      JSON        │                      │
│  PoseInputController │                 │   GestureMapper      │
│  PoseWebSocketClient │                 │   WebSocketServer    │
│  CharacterInputController              │   MediaPipe          │
└─────────────────────┘                 └──────────────────────┘
          ↓                                        ↑
    Game Actions                             Webcam Feed
  (Jump/Slide/Move)                        (Real-time Analysis)
```

## 📁 Integration Files

### Added to EndlessRunnerSampleGame:
- `Assets/Scripts/PoseDetection/PoseWebSocketClient.cs` - WebSocket client
- `Assets/Scripts/PoseDetection/PoseInputController.cs` - Gesture to input mapping
- `Assets/Scripts/PoseDetection/PoseDetectionSetup.cs` - Auto-setup helper

### Uses existing PoseDetection:
- `../PoseDetection/websocket_server.py` - WebSocket server
- `../PoseDetection/gesture_mapper.py` - Gesture recognition logic  
- `../PoseDetection/pose_detector.py` - MediaPipe pose detection

## 🎯 Performance Targets

- **Pose Detection**: <50ms per frame
- **Gesture Recognition**: <30ms processing
- **WebSocket Communication**: <20ms latency
- **Total Input Latency**: <100ms end-to-end

## 🚀 Next Steps

Once basic integration is working:

1. **Fine-tune gesture sensitivity** for your setup
2. **Add visual feedback** for gesture recognition  
3. **Implement gesture combos** for special moves
4. **Add calibration UI** in Unity
5. **Support multiple players** with multiple webcams

---

**Ready to control your endless runner with your body? Let's move! 🏃‍♂️💨**
