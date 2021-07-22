# Game Controls Design

Here lies a breakdown of the game controls. Included are design ideas and suggestions. This doc will be updated as the game is implemented.

The game is multiplayer, thus there are two main ways to play:

## Hotseat
  
- Players play on the same system
- Also called same room, couch play, etc

## Network

- Over the internet
- Would probably have a host system with peers connected
- Probably not getting made this term

## Mouse & Keyboard or Gamepad

The game should be playable with both Mouse & Keyboard (M&K) and Gamepad. M&K involves players using the same devices while any number of gamepads may be used. Gamepads should probably be the preferred control device.

Luckily there is no private information so all good cards and money amounts will be visible at all times to all players. For this we have two options:

### Screen borders

- Create player window that holds all info and stack them around the border of the screen
- Can choose top, bottom, or sides
- Always visible

### Popup

- Have a popup window that can be toggled with a button
- I'm thinking right side of the screen

Suffice to say, the information of the current player whose turn it is will always have their information displayed. Each player window will display the following information:

### Player Info window

- Name
- Turn order/player number
- Major cities connected
- Money
  - As numeral
  - As collection of cash?
- Train type
- Goods held
- Order cards

Since order cards have a lot of text on them it may be too difficult to display everything on a small status window. Most likely the order cards of the current player will be displayed in a larger size on the bottom of the screen. On the small status window all we need is a goods icon to represent each order.

## Pathfinding

We will be incorporating pathfinding into the game to make it more streamlined to build track and move trains. Pathfinding should be able to find the path between any number of points in sequence (ie from A to B, B to C, etc). There will be two settings for the pathfinding: shortest distance and lowest cost.

### Shortest distance

- The path that uses the least amount of edges
- Fairly straightforward for building track
- For moving trains this may involve using other players' track (incurring a payment to that player)

### Lowest Cost

- For building it takes into account node type and looks for the cheapest path to build
- For moving it just avoids using other player's track (since self owned track is free to use)

Note that neither of these options takes into account both at once. For instance, if building a track to an opponent's track to use as a bridge to one's own track is cheaper in the long run.

Players should be able to place track and only pay the cost and actually build it when they end the turn. Distances should probably always be displayed, along with total cost.
