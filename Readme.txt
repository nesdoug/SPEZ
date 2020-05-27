SPEZ - SNES Sprite Editor ver 1.2
May 26, 2020
.NET 4.5.2 (works with MONO on non-Windows systems)
For SNES game development.
Freeware by Doug Fraker

Permission is granted to use and copy, but not to sell this software. 
It is free. Permission is granted to modify and reuse the code, in
whole or in part, as long as the the original author is credited.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE.


version history
1.0 - initial
1.1 - save tiles with RLE option
1.2 - can use all key commands outside tile edit form
    - fixed a load session bug (palette of sprite)
    - sprite list, now can select multi/arbitrary and apply
      palette and shift and flipping to those selected
    - fixed a rounding error
    - added slider bars for color


Note, the RLE is a special compression format that I wrote, 
specifically for SNES maps (but could be used for tiles).
See unrle.txt (or my SNES projects) for decompression code.


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

Left/Right click to open an editing box.
Numberpad 2,4,6,8 to move to adjacent tile.
C - copy, P - paste.

(these only work if focus is not on one of the text boxes on the form)
*note - 2bpp SNES tilesets are NOT like NES. They are like Gameboy, GB,
so, if you use YY-CHR, set the tile mode to 2bpp GB. You can easily
convert NES to SNES in YY-CHR by loading NES, copy all the visible tiles,
switch to 2bpp GB, then paste the tiles again.



Tile Edit Box
-------------
Left click - place the currently selected color on the grid
Right click - get the color under the pointer
Numberpad 2,4,6,8 to move to adjacent tile.
Arrow keys to shift the image.
F - fills a tile with selected color
H - flip horizontal (notice the symmetric shape of the letter W)
V - flip vertical (notice the symmetric shape of the letter E)
R - rotate clockwise
L - rotate counter clockwise
Delete - fills with color 0 (transparent)
C - copy, P - paste.
(! these only work if this box is clicked/active !)



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

Saving Maps only saves the currently selected map. Loading maps only loads to
the currently selected map.

Loading/Saving 1 tileset will load/save the currently selected set. The bit
depth needs to match, so consider marking each tileset with a 4 to
keep them separate from other bit depths (such as 2 or 3 or 8).

File/Export Image saves the current view on the Tilemap as .png .bmp or .jpg




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
-3bpp load / save / edit mode
///////////////////////////////////////////////



Credits -
I used Klarth's Console Graphics Document...
https://mrclick.zophar.net/TilEd/download/consolegfx.txt
in making this software. 

