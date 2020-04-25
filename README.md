# VoiceGoban

VoiceGoban is a tool to allow players who play go (baduk, weiqi) to play hands free on a Windows PC using only their microphone.

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
2. By default, you must then say `Click` to place the stone on the board. See the options below.

### Frequently Used Options
* `Enable Auto Play` - Automatically click the mouse after every movement on the grid. This is particularly useful if you are playing a blitz game, but can cause misplays if the speech engine doesn't detect a coordinate properly.
* `Ascending` - Some go programs show the coordinates in ascending order (1-19) from top to bottom. Say this command to configure it.
* `Descending` - Most go programs show the coordinates in descending order (19-1) from top to bottom. Say this command to configure it.

### Full Command List

```
SPEECH OFF - Disable voice processing
SPEECH ON - (default) Enable voice processing

CONFIGURE TOP LEFT - Sets the top left corner of the board grid to the mouse position
CONFIGURE BOTTOM RIGHT - Sets the bottom right corner of the board grid to the mouse position

CLICK - click mouse at current position
<H-Coord> <V-Coord> - Move cursor to grid position specified. Ex: 'C 17' or 'Bravo 4'

ASCENDING - Set vertical coordinates to be ascending from 1-19
DESCENDING - Set vertical coordinates to be descending from 19-1

BOARD SIZE 19 - Set board size to 19x19
BOARD SIZE 13 - Set board size to 13x13
BOARD SIZE 9 - Set board size to 9x9

DISABLE AUTO - (default) Disables auto-click after moving cursor
ENABLE AUTO - Enables auto-click after moving cursor

DISABLE I COORDINATE - (default) Disables processing 'I' as a valid horizontal coordinate
ENABLE I COORDINATE - Enables processing 'I' as a valid horizontal coordinate

DISABLE READ BACK - (default) Disables reading coordinates back to you
ENABLE READ BACK - Enables reading coordinates back to you

MALE VOICE - Set computer readback voice to male
FEMALE VOICE - Set computer readback voice to female
```

*Copyright 2020 Brendan LaMarche*