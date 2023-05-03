# Trolls-Dungeons

DEMO LINK: https://youtu.be/Z5O0b94cp-U

Controls:
WASD for movement

Objective: Collect all the Gems in the Troll Lair before losing all health points.


Generator:

The generator was created that uses the B678 Rule (inspired by https://generativelandscapes.wordpress.com/2019/04/11/cave-cellular-automaton-algorithm-12-3). 
It begins by randomly placing objects throughout the map,
it then proceeds to smoothing iterations where for each wall tile a rule will be applied. this rule
only applies for map objects such as ground and wall, the torch and gems are randomly spawned.
A wall is destroyed and a ground cell is born if the wall has less than 3 wall neighbours or more than 5.
A new wall cell is born if the neighbour count is 6,7 or 8 


Agents:

Thief - The thief uses steering behaviours to simulate a random wander movement. They are not aggressive and will not pursue the player, 
they wander randomly but if the player bumps into them then they will pickpocket the player's gems (2 gems). 
The thief uses UnityMovementAI package and uses
manual steering and reactive navigation techniques.

Troll - The trolls will seek out the player and attack him if he gets too close. Only way to avoid him is 
to pickup the torch which will make him flee. Uses a behavioural tree
developed with NPBehave.


Troll chief - Only one of these will spawn.This troll is very aggressive but also slow, torches will not work against him and he will
pursue unless you outrun him, in that case he will go back to wander. Uses a behavioural tree
developed with NPBehave.

Objects:

Gems - Gems are randomly spawned and must be picked up by the player
Torch - Torches provide assistance by making all trolls flee.
