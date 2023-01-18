# SNES
SNES example code

2023 Doug Fraker

Testing the new sprite code, with
Hitboxes and sprite flipping
new functions to work with the SPEZ 3.0
(will break using metasprite data from earlier SPEZ)

Also, changed the way the high table is handled...
-at the start of each loop, call Clear_OAM and 
 set sprid to zero
-then draw each sprite
-then call Process_OAM_HT to combine the high
 table bits
 

To draw 1 sprite, use OAM_Spr

To draw 1 metasprite, normal, use OAM_Meta

To draw 1 metasprite, flipped, use OAM_Meta_Flip
and have the spr_flip variable to indicate how
it's flipped.

To draw 1 metasprite with a different palette
or priority, use OAM_Meta_A

To do that, but flipped, use OAM_Meta_Flip_A
and have the spr_flip variable to indicate how
it's flipped.

Meta2_Obj1 and Meta2_Obj2 are for setting up
a sprite collision check.

Check_Collision will actually do a collision
check and set the collision variable 0 or 1

nesdoug.com

