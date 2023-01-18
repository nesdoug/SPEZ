; variables declared

FACE_LEFT = 0
FACE_RIGHT = 1



.segment "ZEROPAGE"

in_nmi: .res 2
temp1: .res 2
temp2: .res 2
temp3: .res 2
temp4: .res 2
temp5: .res 2
temp6: .res 2

; for sprite code
sprid: .res 2 ;0-127, keep the high byte zero
spr_x: .res 2 ; 9 bit
spr_y: .res 1 
spr_c: .res 1 ; tile #
spr_a: .res 1 ; attributes
spr_sz:	.res 1 ; sprite size, 0 or 2
spr_h: .res 1 ; high 2 bits
spr_x2:	.res 2 ; for meta sprite code
spr_flip: .res 1 ;is this sprite flipped?
;flip status-- 0=no,1=x,2=y,3=xy
flip_x: .res 1
flip_y: .res 1


; for sprite collision code
obj1x: .res 1
obj1w: .res 1
obj1y: .res 1
obj1h: .res 1
obj2x: .res 1
obj2w: .res 1
obj2y: .res 1
obj2h: .res 1
collision: .res 1

pad1: .res 2
pad1_new: .res 2
pad2: .res 2
pad2_new: .res 2


;user sprite
user_x: .res 2
user_y: .res 2

;array of other sprite objects
spr_obj_x: .res 16
spr_obj_y: .res 16
spr_obj_ms: .res 16 ; which metasprite?
spr_obj_fl: .res 16 ; flip

loop16: .res 2


;for BG collision code
obj_top: .res 1
obj_bottom: .res 1
obj_left: .res 1
obj_right: .res 1

bright_var:	.res 1


.segment "BSS"

PAL_BUFFER: .res 512 ;palette

OAM_BUFFER: .res 512 ;low table
OAM_BUFFER2: .res 32 ;high table 2 bits per sprite
OAM_BUFFER3: .res 128 ;high table 1 byte per sprite
