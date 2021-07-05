# Resourceful Rails Basic Game Flow

Here lies a breakdown of the game, its rules, phases, components, and objectives.

**Objective of the Game**: to build railroad track connecting 6 of the 7 major cities and have $250 in cash.

## Setup

- 2-6 players
- choose map
- each player chooses color
- determine turn order
  - random
  - custom
- each player starts with
  - $50
  - 3 order cards

## Build turns

- start with player 1
- can only build
- move to next player
- when at end of turn of last player
  - reverse turn order
  - last player goes again
- when at end of turn of first player
  - reverse turn order
  - first player goes again

## First Normal turn (for each player)

- player places train on any city
- city must have track from that player connected
- then play continues as a normal turns

## Normal turns

- Phase 1
  - move train
  - pickup good
  - drop off good
  - discard hand and draw 3 new cards
    - this option replaces any other action
    - instantly ends the turn

- Phase 2
  - build track
    - $20 limit
  - upgrade train
    - may choose to upgrade instead of building
    - costs $20

## Order Cards

- Each player must have 3 at all times
- 3 orders per card
- Order consists of 3 elements
  - City to deliver to
  - Good to deliver
  - Money recieved on delivery
- when a good is delivered
  - Discard that card (only one good may be delivered per card)
  - Draw a new card
  - Eecieve payment immediately
- Card has varied payouts
- Payment based on distance and difficulty getting good

## Building track

- Track built between adjacent nodes
- Nodes are in hexagonal grid formation
- Thus each node has only 6 possible nodes to build to
- Node can one of several types
  - clear         $1
  - mountain      $2
  - small city    $3
  - medium city   $3
  - major city    $5
  - water         N/A
- Water nodes cannot be built on at all
- Price is determined by node being built to, not from

## River

- Building over a river incurs an extra cost on top of the normal build cost
- +$2

## Cities

- Each city has between 0-3 goods that trains may pick up
- Small city
  - only 2 players may build into
  - 1 node
- Medium city
  - only 3 players may build into
  - 1 node
- Major city
  - any number of players may build into
  - 7 nodes total (hexagon)
  - no player may build between major city nodes
  - major city nodes are considered track that all players may use freely

- Courtesy rule
  - players cannot build such that they block other players to prevent the maximum number of connections
  - ie, there must be enough inlets for 2 players to build into a small city
  - they may block more convenient access however

## Goods

- 30 types in total
- Each takes up one good slot on train

## Trains

- 4 types of train
  - Modest train
    - 9 movement
    - 2 goods
  - Fast train
    - 12 movement
    - 2 goods
  - Heavy train
    - 9 movement
    - 3 goods
  - Advanced train
    - 12 movement
    - 3 goods
- All players start with modest train
- Modest train can upgrade to fast or heavy train
- Fast or heavy trains can upgrade to advanced train

## Optional Rules

- Fast trains
  - 9 -> 12
  - 12 -> 16
  - Great for more players
- Extra build turn
  - Start with $60
  - 3 build turns instead of 2
- Extra starting cards
  - Start with 5 cards instead of 3
  - When first normal turn starts, discard 2 cards
