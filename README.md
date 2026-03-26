# CubeRunner

A 3D platformer built with Unity where the player controls a cube running across procedurally generated platforms. The game gets progressively harder as speed increases, and features a risk-based scoring system that rewards risky jumps, edge landings, and combos. A global leaderboard tracks the highest scores using a Node.js backend with PostgreSQL.

## How to Play

- **A / D** or **Left / Right Arrow** - Move left/right
- **Space** - Jump (double jump available mid-air)
- **Left Ctrl** - Crouch (builds up a speed boost while grounded)
- **Mouse Scroll** - Zoom camera in/out

Avoid obstacles (rotating blades, falling blocks, walls) and don't fall off the platforms. The further you go, the higher your score.

## Requirements

- **Unity 6000.2.7f2** (or compatible)
- **Node.js** (for the leaderboard backend)
- **PostgreSQL 15+** (database for scores)

## Getting Started

### Game (Unity)

1. Clone this repo
2. Open the project folder in Unity Hub
3. Open `Assets/Scenes/MainMenu.unity` (or whichever scene is set as the start scene)
4. Hit Play

### Leaderboard Backend

The game connects to a REST API at `localhost:3000` for score submission and leaderboard fetching. To run the backend:

1. Set up a PostgreSQL database (the project uses `postgres:15-alpine` via Docker)
2. Run the Node.js server:
   ```
   cd <backend-directory>
   npm install
   npm start
   ```
3. The API will be available at `http://localhost:3000/api`

> The game works without the backend, but leaderboard features won't load.

## Project Structure

```
Assets/
  Scripts/
    Manager/       - GameManager, scoring, leaderboard logic
    Player/        - Player controller, gravity, camera, input
    Platform/      - Platform types (base, speed boost, obstacles)
    Spawner/       - Procedural platform spawning
    Obstacles/     - Obstacle behaviors (rotating, falling, wall)
    UI/            - Score display, endgame screen, main menu
```

## Screenshots

**Backend**

<img width="1766" height="1201" alt="Backend" src="https://github.com/user-attachments/assets/3b731d24-39b6-4cee-876f-322083d4d56b" />

**Game Start**

<img width="3234" height="1439" alt="Game Start" src="https://github.com/user-attachments/assets/b8ebee3b-c789-4789-ae01-5053afe1c1cc" />
<img width="3199" height="1351" alt="Character Select" src="https://github.com/user-attachments/assets/d462fb22-6ef1-421e-a6ef-77e90cf5e11b" />

**Gameplay**

<img width="2325" height="1276" alt="Gameplay 1" src="https://github.com/user-attachments/assets/5eca8b33-8617-4a81-85d7-4d5bd0d3d641" />
<img width="2968" height="1407" alt="Gameplay 2" src="https://github.com/user-attachments/assets/303e0b8b-f286-4dbd-b21f-aa86307449aa" />
<img width="3253" height="1405" alt="Gameplay 3" src="https://github.com/user-attachments/assets/ba337fd7-edd8-4f0e-a96b-c3cf6d720da2" />

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) for details.
