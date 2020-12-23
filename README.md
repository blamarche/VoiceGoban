# VoiceGoban

VoiceGoban is a tool to allow players who play go (baduk, weiqi) to play hands free on a Windows PC using only their microphone. It optionally supports reading opponent's move coordinates back to the player so an entire game can be played without using a computer screen.

*NOTE: The program is set to run as an administrator because services like Tygem and Foxy run as admin, so for artificial mouse events to work on those programs VoiceGoban must be admin as well.*

Requirements: .NET 4.7.1

License: GPLv3

## Usage 

### Configuration

Ensure your microphone is plugged in and working as the default Windows recording device before beginning.

1. Start VoiceGoban.
2. Start your favorite go client and open a board or start a game.
3. Position your mouse on the top left corner of the board grid.
4. Say `Configure top left`, make sure VoiceGoban acknowledges the command.
5. Position your mouse on the bottom right corner of the board grid.
6. Say `Configure bottom right`, make sure VoiceGocan acknowledges the command.

**If you move or resize your board or game window, you will need to set the top left and bottom right again**

### Playing
1. Say any horizontal coordinate followed by a vertical number coordinate. Example: `C 16` or `B 3`. You can also use the phonetic alphabet to help the system understand your commands. Example: `Charlie 16` or `Bravo 3`.
2. By default, you must then say `Click` to place the stone on the board. See the options below and in the help file for more options.

Optional: (Experimental) Say `START GAME AS BLACK` or `START GAME AS WHITE`, or `ENABLE SPECTATOR MODE` when you begin a game and it will attempt to read your opponents move coordinates by scanning the screen. If computer performance is poor while reading moves, try shrinking your board window.

### Frequently Used Options
* `Enable Auto Play` - Automatically click the mouse after every movement on the grid. This is particularly useful if you are playing a blitz game, but can cause misplays if the speech engine doesn't detect a coordinate properly.
* `Ascending` - Some go programs show the coordinates in ascending order (1-19) from top to bottom. Say this command to configure it.
* `Descending` - Most go programs show the coordinates in descending order (19-1) from top to bottom. Say this command to configure it.

[Full Command List](help.txt)

*Copyright 2020 Brendan LaMarche*
