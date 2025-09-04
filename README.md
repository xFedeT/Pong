# Pong Multiplayer

A classic Pong game implementation with real-time multiplayer networking support, built in C# using Windows Forms and TCP/IP communication.

## 🎮 Features

- **Real-time Multiplayer**: Two players can connect and play over a network
- **TCP/IP Networking**: Reliable client-server architecture
- **Smooth Graphics**: Anti-aliased rendering with customizable colors
- **Game Physics**: Realistic ball physics with collision detection
- **Score Tracking**: Live score display for both players
- **Countdown System**: 3-second countdown before each game starts
- **Auto-reconnection**: Clients can reconnect after disconnection
- **Cross-platform Server**: Console-based server with management commands

## 🏗️ Architecture

The project follows a clean, modular architecture:

```
Pong/
├── Client/                 # Client-side components
│   ├── App/               # Application entry point
│   └── Graphics/          # Rendering and UI components
├── Server/                # Server-side components
│   └── App/              # Server application entry point
├── Game/                 # Core game logic (shared)
│   ├── Config/           # Game configuration
│   ├── Logic/            # Game physics and rules
│   ├── Network/          # Network communication
│   └── State/            # Game state management
└── Program.cs            # Main application entry point
```

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 or later
- Windows OS (for Windows Forms client)
- Visual Studio 2022 or VS Code with C# extension

### Building the Project

1. Clone the repository:
```bash
git clone [your-repository-url]
cd PongClient
```

2. Build the solution:
```bash
dotnet build
```

3. Or open `PongClient.sln` in Visual Studio and build.

### Running the Game

#### Starting the Server

Run the server from command line:
```bash
dotnet run --project Pong -- server
```

Or from Visual Studio, set command line arguments to `server`.

The server will start and display:
```
=====================================
    Pong Multiplayer Server v1.0    
=====================================

✓ Server started successfully!
✓ Listening on port 12346
✓ Ready to accept player connections
```

#### Starting the Client

Run the client:
```bash
dotnet run --project Pong
```

Or simply run without arguments from Visual Studio.

When prompted, enter the server IP address (default: `127.0.0.1` for local play).

## 🎯 How to Play

### Controls
- **Up Arrow**: Move paddle up
- **Down Arrow**: Move paddle down

### Gameplay
1. Two players connect to the server
2. A 3-second countdown begins
3. The ball starts moving automatically
4. Players control their paddles to hit the ball
5. Score points when the ball passes the opponent's paddle
6. Ball speed increases after each paddle hit
7. Game continues until a player disconnects

### Player Assignment
- **Player 1** (Blue paddle): Left side of the screen
- **Player 2** (Red paddle): Right side of the screen

## 🛠️ Server Commands

While the server is running, you can use these commands:

| Command | Description |
|---------|-------------|
| `help` or `h` | Show available commands |
| `status` or `s` | Display server status |
| `restart` or `r` | Restart the server |
| `clear` or `cls` | Clear console screen |
| `quit` or `exit` or `q` | Stop server and exit |

## ⚙️ Configuration

Game settings can be modified in `GameConfiguration.cs`:

### Field Dimensions
```csharp
public const int FieldWidth = 800;
public const int FieldHeight = 520;
```

### Paddle Settings
```csharp
public const int PaddleWidth = 10;
public const int PaddleHeight = 100;
public const int PaddleSpeed = 10;
```

### Ball Physics
```csharp
public const int BallSize = 20;
public const int BallInitialSpeed = 2;
public const int MaxBallSpeed = 12;
```

### Network Settings
```csharp
public const int ServerPort = 12346;
public const string DefaultServerIP = "127.0.0.1";
```

## 🔧 Technical Details

### Networking Protocol

The game uses a simple text-based protocol over TCP:

#### Client to Server Messages
- `MOVE:1` - Move paddle down
- `MOVE:-1` - Move paddle up

#### Server to Client Messages
- `ASSIGN:1` - Assign player ID (1 or 2)
- `COUNTDOWN:3` - Countdown display (3, 2, 1)
- `START` - Game begins
- `STATE:x,y,p1y,p2y,s1,s2` - Game state update
- `QUIT` - Other player disconnected

### Game State Synchronization

The server maintains the authoritative game state and broadcasts updates to all clients at 30 FPS. The state includes:
- Ball position (x, y)
- Ball velocity (x, y)
- Player 1 paddle Y position
- Player 2 paddle Y position
- Player scores

### Collision Detection

The game implements rectangle-based collision detection for:
- Ball-wall collisions (top/bottom walls)
- Ball-paddle collisions (left/right paddles)
- Goal detection (ball passes paddle boundaries)

## 🐛 Troubleshooting

### Common Issues

1. **Connection Failed**
   - Ensure the server is running
   - Check firewall settings
   - Verify the correct IP address and port

2. **Game Lag**
   - Check network connection stability
   - Ensure server has sufficient resources

3. **Client Crashes**
   - Verify .NET 8.0 is installed
   - Check Windows Forms dependencies

### Debug Output

The application outputs debug information to the console:
- Connection status
- Player assignments
- Network errors
- Game state changes

## 🏆 Future Enhancements

Potential improvements for future versions:

- [ ] Sound effects and background music
- [ ] Multiple game modes (tournament, practice)
- [ ] AI opponent for single-player mode
- [ ] Improved graphics with animations
- [ ] Game replay system
- [ ] Player statistics tracking
- [ ] Custom paddle colors and themes
- [ ] Mobile client support

## 📝 License

This project is open source and available under the [MIT License](LICENSE).


**Enjoy playing Pong Multiplayer!** 🏓
