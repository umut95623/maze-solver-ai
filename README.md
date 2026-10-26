# MazeSolverAI

This project is a coursework assignment from my Artificial Intelligence course during my Erasmus exchange program. It is a Unity application that visualizes and compares the BFS and DFS search algorithms on a maze.

## Features

- Loads a maze from a text file (`Assets/Resources/maze.txt`) and builds it in 3D, viewed from above
- Animates the search: visited cells turn yellow and are labeled "Visited", and the final path is drawn in blue
- Comparison board showing time and path length for BFS and DFS
- Random maze generator: a recursive-backtracker maze with a few extra openings, so there are multiple routes and the two algorithms can find different paths

## How to run

1. Open the project in Unity 6 (6000.3.1f1).
2. Open `Assets/Scenes/SampleScene.unity` and press Play.

## Controls

- Space: start the search
- R: create a random maze

To switch between BFS and DFS, change "Selected Algorithm" on the GameManager object in the Inspector. The animation speed is set by "Step Delay" on the same object (default: 0.1 s per step).