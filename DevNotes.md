# Assets

- Scenes/4xWeighted
    - Canvas/Map
        - TileMap.cs - houses the grid of cells to create the board
    - PlayerManager
        - Player.cs - player profiles are loaded in from scriptable objects. Those players use TileMap.cs to resolve attacks on each tile.
- Scenes/TestTiles - [BROKEN] - tests that tiles render correctly without connecting to any player profiles.
- CustomAssets/Ai* - Instance of a AI logic to be slotted into the Ai field of a PlayerStats.cs ScriptableObject.
    - AiAnyTile - AI weighs a random tile every 10 ticks
    - AiRandom - Chooses a random AI from a set of AIs
    - AiSurround - AI weighs tiles surrounding its enemy. Does not care where its enemy has placed weights.
- CustomAsset/Players
    - Computer1 - Red computer player
    - Computer1 - Yellow computer player
    - Player1 - Green local human player

# GameLoop

- TileMap.cs `Awake()`/`Start()`
    1. creates tiles
    2. places players using a `SpawnStrategy`
    3. resolves combat of players that spawned on the same tile
    4. resize tiles to fit the screen
- TileMap.cs has __no__ `Update()`
- Player.cs `Awake()`/`Start()`
    1. Reset/Initialize every player
    2. Determine which player is providing input
    3. Determine which players are AIs
    4. Kick off core game loop, `doTicks()`
- Player.cs 'doTicks()' (Replacement of `Update()` called on a FixedUpdate of `GameSettings.SecondsPerTick`)
    1. troops are moved among tiles dependant on weights & PathingStrategy set by players
    2. tiles are updated with new troop counts. Players are removed if they do not own any tiles.
    3. AI players determine which tiles to weigh

# Readme.md

[open Readme.md](Readme.md)
