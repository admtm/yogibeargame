# Yogi Bear Game – Picnic Basket Hunt

A simple 2D strategy game made in **C# using Windows Forms**, following a **three-layer architecture**.  
Help **Yogi Bear** collect all the picnic baskets in the forest while avoiding the patrolling rangers!

---

## Gameplay

- The goal: **Collect all picnic baskets without being seen by any ranger.**
- The game is played on an `n × n` grid-based board.
- The player controls **Yogi Bear**, who collects **picnic baskets** while avoiding **patrolling rangers**.
- Rangers move in a straight line (horizontally or vertically), and **reverse direction** when they hit an obstacle or the board edge.
- The game board consists of:
  - **Yogi Bear** (player) –  brown square
  - **Picnic baskets** –  magenta square
  - **Trees (obstacles)** –  dark green square
  - **Rangers** –  yellow square
- Each ranger has **3×3 vision** (including diagonals). If Yogi Bear enters this vision: **Game Over**.

---

## Controls

| Key               | Action                     |
|------------------|----------------------------|
| Arrow keys / WASD | Move                       |
| `Enter` or `Space` | Pause / Resume the game    |

---

## Difficulty Levels

| Level  | Board Size | Rangers | Baskets | Trees |
|--------|------------|---------|---------|-------|
| Easy   | 9×9        | 1       | 10      | 9     |
| Medium | 12×12      | 2       | 20      | 18    |
| Hard   | 15×15      | 4       | 30      | 27    |

---

## Architecture Overview

The game follows a **3-layer architecture**:

### 1. Model
- Contains the core game logic, state, and rules.
- **`GameModel`** handles:
  - Game state
  - Player and ranger movement
  - Win/lose conditions
  - Timer and score management
- Uses events like:
  - `GameAdvanced` → on state update (e.g. basket collected)
  - `GameOver` → when the game ends

### 2. Persistence
- Manages loading/saving game data via text files (`.txt`).
- Handles game board initialization and file I/O.
- Key components:
  - `YogiBoard` (implements `IYogiBoard`)
  - `IYogiGameDataAccess`
  - `YogiGameFileDataAccess`
  - `YogiBoardDataException`
- Supports:
  - Loading predefined levels
  - Saving and restoring game state
### 3. View (Windows Forms)
- Implements UI using a single `MainWindow` screen.
- Main features:
  - Dynamic grid (via `ItemsControl` and `UniformGrid`)
  - Side panel: shows elapsed time & basket counter
  - Pause menu & dialog boxes for save/load/game over

---
## Save & Load

- The game supports saving/loading via menu buttons:
  - `New Game`
  - `Save Game`
  - `Load Game`
  - `Exit`
- Dialogs are used for selecting filenames.
- The game pauses during save/load operations.

---

## Timer & Score Tracking

- Timer starts when the game begins and **pauses automatically** during a pause.
- The current **basket count** and **elapsed time** are displayed alongside the game grid.

---

### Requirements
- Windows 10/11
- [Visual Studio 2022 (17.8 or later)](https://visualstudio.microsoft.com/) with **.NET 8** and **Windows Desktop Development** workload installed
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
> ⚠ This project targets `net8.0-windows`, meaning it can only run on Windows and requires Windows-specific UI libraries (e.g. Windows Forms).

### Running the Game (Step-by-step)
1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-username/yogi-bear-game.git
   cd yogi-bear-game
2. Open solution in Visual Studio:

- File  
- Open  
- Project/Solution  
- Select `YogiBearGame.sln`

3. (Optional) Set startup project:

- Right-click main project  
- Select **Set as Startup Project**

4. Build & run:

- Click **Start** or press **F5**

---
## Documentation

The UML diagrams can be found in the [diagrams](diagrams) folder:

- [Model Diagram](diagrams/wpf_uml_1.pdf)
- [ViewModel Diagram](diagrams/wpf_uml_2.pdf)


> This project was developed for educational purposes as part of a university course
