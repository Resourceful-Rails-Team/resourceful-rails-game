# Resourceful Rails

## Description

Resourceful Rails is a 3D video game board game, where players pay for and build railroad tracks across the Pacific Northwest, delivering cargo to different cities by train, and making a profit from that cargo. Money is the name of the game - when a player earns a certain amount of dollars from their cargo, they win!

There are two different phases for each game that is played - the **build phase** and the **delivery phase**.
- **Build Phase** - during this phase players take turns inserting railroad tracks on the map, travelling from city to city, attempting to maximize their tracks' delivery potentials. Each track costs money to build, and different track nodes entail different costs (for instance - a mountain node requires extra money to build a track on).
- **Delivery Phase** - once the tracks are (for better or worse) complete, the players begin delivering cargo to different cities, using their own tracks, and borrowing the tracks of other players (for a cost). The players draw a pool of cards at random, each designating delivery plans from one city to another. Players then can choose one of these cards and attempt to deliver that particular load. If they do so they make money. 

**Goal of the Game** - to make a certain maximum amount of money. Whichever player reaches the goal first wins.


## Technology

Resourceful Rails is built using the Unity Engine, with the C# programming language. 


## Development Goals

1. Create a level generator / editor, which allows placement of nodes, cities, mountains, and other areas of interest through Unity's interface. For the creation of MVP map, and future maps created by open source developers. This will be made using Unity's editor tools, which offers a poweful and customizable system of generating a unique user interface to meet the requirements.

2. Implement game logic, which will dictate the state of a unique game session, including player turns, build vs. delivery stages, pathfinding, and random card drawing.

3. Develop an interactive user interface, that directs players during the game, and handles player tasks, such as building tracks, choosing cards, and directing trains. 

4. Create art and assets, and an interface which binds the assets to the game state, for players to see. Art assets include trains, tracks, nodes, and a world map background. Art will be both 3D (for models and nodes) and 2D (for background).


## Acknowledgements

- Thanks to the [Unity development team](https://unity.com/) for creating the Unity Engine, for the creation of open-source and industry video games.


## Project roadmap

[Click here](https://trello.com/b/gVsj6pIm) to see the project's roadmap. The roadmap includes color-coded tasks which will implement a three-week prototype (green-colored), and a six-week MVP (yellow-colored).

## License

This work is made available under the "MIT License". Please
see the file `LICENSE` in this distribution for license
terms.