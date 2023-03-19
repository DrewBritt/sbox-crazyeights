# Crazy Eights Mapping Guide

## Details

Required entities:
 - For each player
   + `ce_chair` - A chair for players to sit in. The maximum number of players is determined by the number of available chair entities. 
   Set the `WorldModel` property to your desired chair model. 
 - `ce_deckspawn` x2 - Spawnpoints for the draw pile and discard pile (displays last card). Set the respective `SpawnTarget` property for each.

### Play/Chair order

The play rotation order is determined by the order of the `ce_chair` entities in the Hammer inspector.
I recommend grouping and reorganizing them in the inspector in clockwise/counterclockwise order around the table.

![inspector](https://user-images.githubusercontent.com/43252311/226146323-9358e43b-4d83-47d7-adce-4bb4739241d7.png)

![rotationorder](https://user-images.githubusercontent.com/43252311/226146520-1d9b0340-95a5-4f92-a98a-6c0f2a9fb629.png)
