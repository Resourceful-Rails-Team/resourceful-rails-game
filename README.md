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

## How To Play
See the [Release Page](https://github.com/Resourceful-Rails-Team/resourceful-rails-game/releases/tag/v0.1-alpha) to download a version that works with your system. No installation is required. Currently only Windows is supported.

## Build Instructions

1) Use `git` to download the project: `git clone https://github.com/Resourceful-Rails-Team/resourceful-rails-game`

2) Go to Unity's website using the following link: 

https://unity3d.com/get-unity/download

3) Download and install Unity Hub. This makes it easier to use different versions of Unity. Once it's done installing open Unity Hub.

4) In Unity Hub there will be 4 tabs on the left hand side Select the Installs tab. From there we will be installing the version of Unity that the game uses; **version 2020.3.12f1**. Follow this link and click the link right under the title that says "Unity Hub".

https://unity3d.com/unity/whats-new/2020.3.12

5) A popup window will appear in Unity Hub that will ask you to choose modules to install. In order to successfully compile the game you need to select the platform you're building to (most likely Windows, Mac, or Linux). Select Install and wait until it's done.

6) In order to open the project you first need to download the repository from GitHub. The entire repository needs to be in its own folder. Once that's done go back to Unity Hub to the Projects tab and select the "Add" button in the upper right. Select the folder you used for the repository.

7) Once the project has been added click on it to open it in the Unity Editor. Either press the key combination CTRL+SHIFT+B or select "File" in the upper left corner and then "Build Settings".

8) Keep the main selection on "PC, Mac & Linux Standalone" and select your platform from the dropdown list on the right. Select "Build and Run" at the bottom and then a popup will ask where you want to save the executable. Once you select the location the game will compile and automatically run. If it doesn't run (or you selected "Build" only) then you can always open the game using the executable itself.


## Running Tests

Resourceful Rails' testing system is directly integrated into the editor. Upon opening Unity, the tests will automatically run, and the results will be displayed in the console at the bottom of the window. The console is the thin stip of gray at the very bottom - clicking it will open the window to display multiple lines.

## Inspiration

Inspiration for Resourceful Rails comes from:

- [Empire Builder Board Game](https://en.wikipedia.org/wiki/Empire_Builder_(board_game))
- [18XX Board Game Format](https://en.wikipedia.org/wiki/18XX)
- [18XX Online Games](https://18xx.games/)

## Technology

Resourceful Rails is built using the Unity Engine, with the C# programming language. 


## Development Goals

1. **COMPLETE** Create a level generator / editor, which allows placement of nodes, cities, mountains, and other areas of interest through Unity's interface. For the creation of MVP map, and future maps created by open source developers. This will be made using Unity's editor tools, which offers a poweful and customizable system of generating a unique user interface to meet the requirements.

2. **COMPLETE WITH SOME BUGS** Implement game logic, which will dictate the state of a unique game session, including player turns, build vs. delivery stages, pathfinding, and random card drawing. Please see the project's issues for 

3. **COMPLETE** Develop an interactive user interface, that directs players during the game, and handles player tasks, such as building tracks, choosing cards, and directing trains. 

4. **COMPLETE** Create art and assets, and an interface which binds the assets to the game state, for players to see. Art assets include trains, tracks, nodes, and a world map background. Art will be both 3D (for models and nodes) and 2D (for background).

Refer to the [DESIGN](./DESIGN.md) and [CONTROLS](./CONTROLS.md) documents for more information about the game's rules, UI, controls, components, and objectives.

## Acknowledgements

Thanks to the [Unity development team](https://unity.com/) for creating the Unity Engine, for the creation of open-source and industry video games.

**Pacific Northwest Major Exports Information**

- [World's Top Exports - Utah's Top 10 Exports](https://www.worldstopexports.com/utahs-top-10-exports/)
- [World's Top Exports - Colorado's Top 10 Exports](https://www.worldstopexports.com/colorados-top-10-exports/)
- [World's Top Exports - Idaho's Top 10 Exports](https://www.worldstopexports.com/idahos-top-10-exports/)
- [World's Top Exports - California's Top 10 Exports](https://www.worldstopexports.com/californias-top-10-exports/)
- [World's Top Exports - Nevada's Top 10 Exports](https://www.worldstopexports.com/nevadas-top-10-exports/)
- [World's Top Exports - Wyoming's Top 10 Exports](https://www.worldstopexports.com/wyomings-top-10-exports/)
- [World's Top Exports - Oregon's Top 10 Exports](https://www.worldstopexports.com/oregons-top-10-exports/)
- [World's Top Exports - Montana's Top 10 Exports](https://www.worldstopexports.com/montanas-top-10-exports/)
- [United States Census - State Exports from Wyoming](https://www.census.gov/foreign-trade/statistics/state/data/wy.html)
- [CDFA - California Agricultural Production Statistics](https://www.cdfa.ca.gov/Statistics/#:~:text=California%20agricultural%20exports%20totaled%20%2421.7,%2C%20Davis%2C%20Agricultural%20Issues%20Center.)
- *[California Agricultural Exports: 2016-2017](https://www.cdfa.ca.gov/statistics/PDFs/2017AgExports.pdf)*
- [Office of the United State's Trade Representative: Nevada](https://ustr.gov/node/7244)
- [Office of the United State's Trade Representative: Idaho](https://ustr.gov/map/state-benefits/id)
- [Office of the United State's Trade Representative: Oregon](https://ustr.gov/map/state-benefits/or)
- [Office of the United State's Trade Representative: Montana](https://ustr.gov/map/state-benefits/mt)
*[Seattle Business Magazine: Nearly 20 Percent of Washington's Economy Tied to International Exports](https://www.seattlebusinessmag.com/economy/nearly-20-percent-washington%E2%80%99s-economy-tied-international-exports)*
- [Blue Book Services: "Oregon Grown: Big Exports Business Threatened](https://www.producebluebook.com/2019/03/07/oregon-grown-big-exports-business-threatened/)

**Algorithms**

- Priority Queue Algorithm: [GeeksForGeeks](https://www.geeksforgeeks.org/priority-queue-using-binary-heap/)

- Djikstra's Pathfinding Algorithm: [Wikipedia](https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm).

- Depth-first Search Algorithm: [GeeksForGeeks](https://www.geeksforgeeks.org/difference-between-bfs-and-dfs/)

**3D Models**

- Train Model
    - https://sooline.org/Publications/Drawings/locomotive_Funits.shtml
    - https://www.flickr.com/photos/chuckzeiler/31691793781

- Rail Cross-section
    - http://www.railway-fasteners.com/rail-track.html

- Rail Cross-tie/Sleeper
    - https://ascelibrary.org/cms/asset/e59831a4-1f51-45f3-84fa-0291a2de5f18/2.gif
    - https://ascelibrary.org/doi/abs/10.1061/%28ASCE%29TE.1943-5436.0000256

## Project roadmap

[Click here](https://trello.com/b/gVsj6pIm), to see the project's roadmap. 

| World Creator / Editor                    | Art / Assets                    | Game Logic                          | Stretch Goals      |
| :---                                      | :---                            | :---                                | :---               |
| COMPLETE - Map Creation                   | COMPLETE - Basic 3D Assets      | COMPLETE WITH BUGS - Pathfinding    | Optional Rules     |
| COMPLETE WITH HOTFIX - Map Serialization  | COMPLETE - Basic UI             | COMPLETE - Level Setup              | Multi. Short Paths |
| COMPLETE - Map Editing                    | COMPLETE - Map Background       | COMPLETE - Build Turns              | 2D Billboard Art   | 
|                                           | COMPLETE - Complete 3D Assets   | COMPLETE - Trains                   | Controller Support |
|                                           | COMPLETE - Complete UI          | COMPLETE - Order Cards              |                    |
|                                           |                                 | COMPLETE WITH BUGS - Delivery Turns |                    |
|                                           |                                 | COMPLETE - Train Upgrades           |                    |
|                                           |                                 | COMPLETE - Determining Win          |                    |

<br>

## Demo Video

Please [click here](https://drive.google.com/file/d/1wDlqFxuudIed_5_tMfCL8w4oI_okC6Ot/view?usp=sharing) to see our project video.

## Work Summary

Thomas Abel
- 
- Writing
- 3D modelling
- Goods icons art
- Main Game loop logic

Daniel Gerendasy
- Title screen UI and logic
- Create game screen UI and logic
- In game UI and UI logic
- Map editing tool
- Editor map rendering
- Map data structure and serialization

Christian Schmid
- Vector art / texture
- Map design
- Graphics system
- Pathfinding system
- Track systems
- Card Deck system
- Train movement system
- Test system / writing


## License

This work is made available under the "MIT License". Please
see the [LICENSE](./LICENSE) in this distribution for license
terms.