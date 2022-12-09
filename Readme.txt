SPEZ - SNES Sprite Editor ver 2.4
Dec 9, 2022
.NET 4.5.2 (works with MONO on non-Windows systems)
For SNES game development.
Freeware by Doug Fraker

The MIT License (MIT)


This app is for generating, editing, and arranging 
SNES tiles for use as Sprites.


version history
1.0 - initial
1.1 - save tiles with RLE option
1.2 - can use all key commands outside tile edit form
    - fixed a load session bug (palette of sprite)
    - sprite list, now can select multi/arbitrary and apply
      palette and shift and flipping to those selected
    - fixed a rounding error
    - added slider bars for color
1.3 - fixed tilemap image zoom code that cut off 1/2
      a pixel at the top and left, that effected exported
      pictures also.
    - fixed bug, double clicking in a dialogue box caused
      a mouse event on the tilemap below it.
    - added hotkeys to change the tileset-number keys 1 and 2
1.4 - fix, rt click on tile editor wasn't updating 
      palette values
2.0 - Import palette and CHR from an image (128x128)
    - undo function
    - several minor UI changes
    - changed icon
    - save tiles in range
    - load tiles to selected tile
2.1 - added a "smart import" for metatiles
    - output says 8x8 and 16x16 istead of 8 and 16
    - white box on tile selection resizes to the
      currently selected sprite size (apply large, eg)
    - clicking on a listbox will highlight a tile
    - minor change on "use top left pixel as transparent"
    - moving metasprites past the edge won't deform them
    - less strict in moving sprites past edges
    - default priority has been changed to 2
    - allow small images to be imported as palettes
      (as small as 2x1) to allow 16x1 images as a palette
    - importing images only blanks the tiles it needs
2.2 - changed key commands, and added some...
    - a=select all, x=cut, v=paste, y=vert flip
    - multi-tile select mode added (= default)
    - allows cut/paste/flip/etc with multiple tiles
    - and adding multiple tiles into metasprite at once
2.3 - slight change to "best color" formula
      (should prefer a color closer to the original hue
      rather than a wrong color of the same brightness)
2.4 - slight change (form2 code)

Note, the RLE is a special compression format that I wrote, 
specifically for SNES maps (but could be used for tiles).
See unrle.txt (or my SNES projects) for decompression code.


Undo
----
Type Z or choose Edit/Undo
Only edits to the tiles and the current metasprite 
can be undone. Some things may not be reverted on undo.


Import Image
------------
Resize the image to 128x128 or less. Import the palette
first, then import the CHR / tiles.
There are some options, default has...
-use the top left pixel as the transparent color.
-unchecked, it will organize the colors by darkness and
use the darkest color as the transparent color.
! if it imports incorrectly, try without this checked


Smart Import Image
------------------
This will import an image (up to 128x128) as tiles, 
starting at the currently selected tile, and the 
currently selected metasprite, and then auto-generate 
a metasprite(s) from it.

There are 2 "Smart Options" - One and Multi
-One will assume the entire image is part of the
metasprite (the size options will be ignored)
-Multi is for a sprite sheet... one image with
multiple sprites that use the same palette that
are arranged at regular intervals.
In the options box, the size means the size in
the original image, for each metasprite.
You can have them arranged horiz/vert/or both.
And it will auto-generate multiple metasprites.

You are going to want to save before doing this.
I think I have all the bugs out, but the Smart
Import Feature is highly prone to user error.
-it may overwrite tiles if you didn't select 
a free area of the tileset
-it may overwrite a metasprite, if you forget
to select a blank one first
-it may completely screw up if sprite size isn't
correct in 2 different places.

So... the steps are...
-Save your file
-Import your palette (maybe from the image)
-Select the tile you want to start at
-Select the first blank metasprite in the list
-Check the Smart Import settings, it should
 match what the imported image is doing
-Check the Sprite Size Settings from top menu*
-Now use the Smart Import

*keep in mind that sprite size is a global setting
that affects all sprites on the screen, and you
probably want to keep it 8x8 and 16x16 even though
your metasprite may be 64x64




Metasprites
-----------
A metasprite is a table of hardware sprites that represents one object,
such as a character or something animated on the screen. The max number
of metasprites is 100 and the max number of sprites is 100 per meta-
sprite. The list on the top is the metasprite list. The list on the bottom
is the sprites in the selected metasprite.

Left click on the main window (top left) to add a sprite. Right click and
drag to move that sprite (or use the arrows to nudge it 1 pixel). Click
on the sprite list (bottom left) to select a sprite.
** You can now CTRL click to select/deselect specific sprites
and Shift-Click to select a block of sprites. Then you can nudge and
Rt-Click-Drag the selected sprites. However, only select 1 sprite to
reorder up/down or "delete selected".

There is only 1 priority setting per metasprite, which is
optional. It doesn't make sense for an object to be on multiple layers
at once. Besides, the main program should preferably decide which layer 
an object should be.

Saving/loading to .bin is just a way to copy one metasprite from one 
file to another, and is not useful outside this program.

Saving to .asm is a SNES standard byte layout, 5 bytes per sprite.
1.relative X
2.relative Y
3.tile #
4.attributes (V-flip, H-flip, priority, palette, tileset)
5.size
(the 9th X bit is not used)

These asm files work with the easySNES code, also by Doug Fraker.




Tilesets
--------
2 sets for 4bpp, (same amount SNES can use at once)

Left click to open an editing box.
(see Key Commands below)
the "MANY" checkbox is for multi-tile editing mode
(uncheck for original "ONE" tile editing mode)
-you can now do cut/paste/flip/shift/etc with
multiple tiles selected at once.
-you can add all selected tiles to the metasprite
Left-Click and drag over multiple tiles from
top left to bottom right, to select many.




Tile Edit Box
-------------
Left click - place the currently selected color on the grid
Right click - get the color under the pointer

Key Commands
------------
Numberpad 2,4,6,8 to move to adjacent tile (One Tile Mode Only)
Arrow keys to shift the image.
F - fills a tile with selected color
H - flip horizontal
Y - flip vertical
R - rotate clockwise
L - rotate counter clockwise
Delete - fills with color 0 (transparent)
C - copy
X - cut
V - paste.
A - select all tiles


Palette
-------
In 4bpp we have 16 colors (15 + transparent) per palette.
The 0th color is always transparent, maybe set it to pink to
avoid thinking that 0 color black will show as black.

Left/Right click - select a color
R - edit red
G - edit green
B - edit blue
Hex - manually type the SNES color code (2 bytes)
(the color doesn't update until you hit Return in one of these boxes)

Key presses...(click a palette color first)
Q = copy selected color
W = paste to the selected color
E = delete the selected color (sets 0000 black)

* use caution naming palettes the same as your tileset, if you use YY-CHR
like I do. YY-CHR will auto-create a palette, if you load a .chr and it
also finds a .pal of the same name. However, it assumes RGB and not the
15 bit SNES style palette, so the palette will be junk colors.
The load/save palette as RGB options are specifically for YY-CHR. THAT
palette can be the same name as the tileset.



Menu
----
All the menu options should be self-explanatory. Some of them won't work if
you are in the wrong mode. The message box should explain the problem.

Loading just 32 bytes palette loads to the currently selected palette row.
Same with saving 32 bytes. It saves the currently selected palette row.

Loading/Saving 1 tileset will load/save the currently selected set. The bit
depth needs to match, so consider marking each tileset with a 4 to
keep them separate from other bit depths (such as 2 or 3 or 8).

File/Export Image saves the current view on the Tilemap as .png .bmp or .jpg



WHAT IS THAT?
-------------
-On the left side, the palette box is for changing the palette
of a sprite already in the list (and selected) 
-Apply H, Apply V, Apply Large will do those transformations
to any future sprite added... I guess Apply Large only really
works in the old One Tile Mode, as the Multi Tile Mode will
guess what you want based on how many tiles selected.
-H Flip, V Flip, Resize, (arrow) buttons will modify a
sprite that is already on the list (and selected) 
-select all, delete selected, delete all buttons applies to
the sprite list, as does the "reorder" arrow buttons


native .SPZ file format
16 byte header = 
 3 bytes "SPZ"
 1 byte # of file version (should be 1)
 1 byte # of palettes (should be 1)
 1 byte # of 4bpp tilesets (should be 2)
 1 byte # of metasprites (should be 100)
 1 byte # of sprites per metasprite (should be 100)
 pad 8 zeros
256 bytes per palette (should total 256)
8192 bytes per 4bpp tileset (x2 = 16384 total)
100x100x4 = metasprites (40000 total)
  -rel x, rel y, tile, VH-ZpppS = 
   v flip 1, h flip 1, size, palette 3, set - 4 bytes
priority (100 total)
sprite count (100 total)
name (20 chars) x 100 = (2000 total)
16+256+16384+40000+100+100+2000 = 58856 bytes.



///////////////////////////////////////////////
TODO-
-see NOTES.txt
///////////////////////////////////////////////



Credits -
I used Klarth's Console Graphics Document...
https://mrclick.zophar.net/TilEd/download/consolegfx.txt
in making this software. 

