# Goal

A strategy game. A simplified Travian-ish clone where attacks are orchestrated quickly via weighted tiles

# How to play

Tap on tiles to change whether or not troops rally at that tile. Each tile can have only 2 different weight values.

- no weight, this tile is only a troop factory
- weight, troops rally here

### Pathing

Your army will move as follows:
1. Uncaptured tiles with weight have priority over captures tiles with weight. This means your army will go right past a weighted tile you own to get to a tile you don't own. No troops will be pulled directly off of weighted tiles you own.
2. Proximity, your army will head towards the closest weighted uncaptured tile. If no uncaptured tiles are weighted, towards the closest captured and weighted tile.

In the screenshot below the green human player has weighted the top left and right tiles. The armies closest to each corner will move towards those weights. Capturing any tiles in their way. Each tile has a radial graph of its percentage of troops compared to other tiles owned by that player. Knowing this, greens strongest tile is the one with 22 troops because that graphically shows a solid light green. Yellows strongest tiles has 31 troops.
![Game screenshot](https://github.com/Feddas/4xWeighted/releases/download/0.0.2/4xWeighted.JPG)

# Origin

"Redirecting troops is a hassle (in Neptune's Pride 2). There needs to be a way to automate it." - Peter Maresh

# Development

Refer to [DevNotes.md](DevNotes.md).

# Repo

https://github.com/Feddas/4xWeighted
