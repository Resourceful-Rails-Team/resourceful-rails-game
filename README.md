# Resourceful Rails

### Thomas Abel, Daniel Gerendasy, and Christian Schmid
</br>

## Description

Resourceful Rails is a 3D video game board game, where players pay for and build railroad tracks across the Pacific Northwest, delivering cargo to different cities by train, and making a profit from that cargo. Money is the name of the game - when a player earns a certain amount of dollars from their cargo, they win!

There are two different turn types for each game that is played - **build turns** and **delivery turns**.

- **Build Turns** - Build turns take place for the first segment of the game. During these turns players insert railroad tracks on the map, travelling from city to city, attempting to maximize their tracks' delivery potentials. Each track costs money to build, and different track nodes entail different costs (for instance - a mountain node requires more money to build a track on than a flat node).

- **Delivery Turns** - once the tracks have been built, the players begin delivering cargo to different cities, using their own tracks, and borrowing the tracks of other players (for a cost). To begin, players draw 3 order cards at random, each designating delivery plans from one city to another.

    There are two phases during a delivery turn:

    1. Players first direct their trains to pickup cargo at the cities specified by their order cards. Then they deliver that cargo to the target cities. If desired, players can also discard their whole hand and draw a new one, at the cost of their turn.

    2. Players can spend money to add onto their current tracks, up to a certain maximum amount of money. Along with this, they can choose to upgrade their trains, for different benefits such as added speed and more space for orders.

**Goal of the Game** - to connect railroad tracks to a specified amount of major cities, and to have a set amount of cash in hand. If a player meets both these goals, they win.


## Inspiration

Inspiration for Resourceful Rails comes from:

- [Empire Builder Board Game](https://en.wikipedia.org/wiki/Empire_Builder_(board_game))
- [18XX Board Game Format](https://en.wikipedia.org/wiki/18XX)
- [18XX Online Games](https://18xx.games/)

## Technology

Resourceful Rails is built using the Unity Engine, with the C# programming language. 


## Development Goals

1. Create a level generator / editor, which allows placement of nodes, cities, mountains, and other areas of interest through Unity's interface. For the creation of MVP map, and future maps created by open source developers. This will be made using Unity's editor tools, which offers a poweful and customizable system of generating a unique user interface to meet the requirements.

2. Implement game logic, which will dictate the state of a unique game session, including player turns, build vs. delivery stages, pathfinding, and random card drawing.

3. Develop an interactive user interface, that directs players during the game, and handles player tasks, such as building tracks, choosing cards, and directing trains. 

4. Create art and assets, and an interface which binds the assets to the game state, for players to see. Art assets include trains, tracks, nodes, and a world map background. Art will be both 3D (for models and nodes) and 2D (for background).

Refer to the [DESIGN](./DESIGN.md) and [CONTROLS](./CONTROLS.md) documents for more information about the game's rules, UI, controls, components, and objectives.

## Acknowledgements

- Thanks to the [Unity development team](https://unity.com/) for creating the Unity Engine, for the creation of open-source and industry video games.


## Project roadmap

[Click here](https://trello.com/b/gVsj6pIm), to see the project's roadmap. The roadmap includes color-coded tasks which will implement a three-week prototype (green-colored), and a six-week MVP (yellow-colored).

<br>

| World Creator / Editor        | Art / Assets                   | Game Logic                  | Stretch Goals      |
| :---                          | :---                           | :---                        | :---               |
| **Proto** - Map Creation      | **Proto** - Basic 3D Assets    | **Proto** - Pathfinding     | Optional Rules     |
| **Proto** - Map Serialization | **Proto** - Basic UI           | **Proto** - Level Setup     | Multi. Short Paths |
| **MVP**   - Map Editing       | **MVP**   - Map Background     | **Proto** - Build Turns     | 2D Billboard Art   | 
|                               | **MVP**   - Complete 3D Assets | **Proto** - Trains          | Controller Support ||                               | **MVP**   - Complete UI        | **MVP**   - Order Cards     |                    |
|                               |                                | **MVP**   - Delivery Turns  |                    |
|                               |                                | **MVP**   - Train Upgrades  |                    |
|                               |                                | **MVP**   - Determining Win |                    |

## License

This work is made available under the "MIT License". Please
see the [LICENSE](./LICENSE) in this distribution for license
terms.
