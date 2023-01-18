;Doug Fraker Jan 2023
;metasprite code for version 3 of SPEZ
;first 2 bytes of metasprite data
;should be x,y flip value

.segment "CODE"
	
    
   

Process_OAM_HT:
;do this at the end of each frame
;high table, roll the bits together
;assumes each value is 0-3
    php
    AXY8
    ldx #0
    txy ;=0
@loop:
    lda OAM_BUFFER3+3, x
    asl a
    asl a
    ora OAM_BUFFER3+2, x
    asl a
    asl a
    ora OAM_BUFFER3+1, x
    asl a
    asl a
    ora OAM_BUFFER3, x
    sta OAM_BUFFER2, y
    
    lda OAM_BUFFER3+7, x
    asl a
    asl a
    ora OAM_BUFFER3+6, x
    asl a
    asl a
    ora OAM_BUFFER3+5, x
    asl a
    asl a
    ora OAM_BUFFER3+4, x
    sta OAM_BUFFER2+1, y
    
    lda OAM_BUFFER3+11, x
    asl a
    asl a
    ora OAM_BUFFER3+10, x
    asl a
    asl a
    ora OAM_BUFFER3+9, x
    asl a
    asl a
    ora OAM_BUFFER3+8, x
    sta OAM_BUFFER2+2, y
    
    lda OAM_BUFFER3+15, x
    asl a
    asl a
    ora OAM_BUFFER3+14, x
    asl a
    asl a
    ora OAM_BUFFER3+13, x
    asl a
    asl a
    ora OAM_BUFFER3+12, x
    sta OAM_BUFFER2+3, y
    
    txa
    clc
    adc #$10
    tax
    iny
    iny
    iny
    iny
    cpy #32
    bcc @loop
    plp
    rts




    
    
    
    
    
OAM_Spr:
.a8
.i16
; to put one sprite on screen
; copy all the sprite values to these 8 bit variables
; spr_x - x (9 bit)
; spr_y - y
; spr_c - tile #
; spr_a - attributes, flip, palette, priority
; spr_sz = sprite size, 0 or 2
    php
	rep #$30 ;axy16
    lda sprid
    tay
	asl a
    asl a ;x4
    tax
    A8
    lda spr_x ;x low byte
	sta a:OAM_BUFFER, x
	lda spr_y ;y
	sta a:OAM_BUFFER+1, x
	lda spr_c ;tile
	sta a:OAM_BUFFER+2, x
	lda spr_a ;attribute
	sta a:OAM_BUFFER+3, x

    lda spr_x+1 ;9th x bit
	and #1 ;we only need 1 bit
	ora spr_sz ;size
    sta OAM_BUFFER3, y ;high table
    tya
    inc a
    and #$7f
    sta sprid
    plp
    rts    
    





OAM_Meta:
.a16
.i16
; spr_x - x 9 bit
; spr_y - y
; A16 = metasprite data address
; X = bank of metasprite data    
 
    php 
    AXY16
	sta temp1 ;address of metasprite
	stx temp2
    
    lda sprid
	asl a
    asl a ;x4
    tax
    
    ldy #2 ;0
@loop:
    A8
    lda [temp1], y
    cmp #128 ; end of data
	beq @done
;first byte is rel x (signed)	
	A16
	and #$00ff
	cmp #$0080 ;is negative?
	bcc @pos_x
@neg_x:
	ora #$ff00 ; extend the sign
@pos_x:
	clc
	adc spr_x
    A8
	sta a:OAM_BUFFER, x
;high byte still has 9th bit    
    iny
	lda [temp1], y ;y byte
	clc
	adc spr_y	
	sta a:OAM_BUFFER+1, x
	iny
	lda [temp1], y ;tile
	sta a:OAM_BUFFER+2, x
	iny
	lda [temp1], y ;attributes
	sta a:OAM_BUFFER+3, x
	iny
	lda [temp1], y ;size, 0 or 2
	iny
	sta spr_h
    xba
    and #1
    ora spr_h
    ldx sprid
    sta a:OAM_BUFFER3, x ;high table
    txa
    inc a
    A16
    and #$007f
    sta sprid
    asl a
    asl a ;x4
    tax
    bra @loop

@done:
    plp
    rts
    
    
    
OAM_Meta_H: ;horiz flip
.a16
.i16
; spr_x - x 9 bit
; spr_y - y
; A16 = metasprite data address
; X = bank of metasprite data    
 
    php 
    AXY16
	sta temp1 ;address of metasprite
	stx temp2
    
    A8
    lda [temp1]
    sta flip_x
    A16
    
    lda sprid
	asl a
    asl a ;x4
    tax
    
    ldy #2 ;0
@loop:
    A8
    lda [temp1], y
    cmp #128 ; end of data
	beq @done
;first byte is rel x (signed)	

;************************** added for flip code
    lda flip_x
    sec
    sbc [temp1], y
;**************************

	A16
	and #$00ff
	cmp #$0080 ;is negative?
	bcc @pos_x
@neg_x:
	ora #$ff00 ; extend the sign
@pos_x:
	clc
	adc spr_x
    A8
	sta a:OAM_BUFFER, x
;high byte still has 9th bit    
    iny
	lda [temp1], y ;y byte
	clc
	adc spr_y	
	sta a:OAM_BUFFER+1, x
	iny
	lda [temp1], y ;tile
	sta a:OAM_BUFFER+2, x
	iny
	lda [temp1], y ;attributes
    
;************************** added for flip code
    eor #$40 ;H FLIP
;**************************     
    
	sta a:OAM_BUFFER+3, x
	iny
	lda [temp1], y ;size, 0 or 2
	iny
	sta spr_h
    xba
    and #1
    ora spr_h
    ldx sprid
    sta a:OAM_BUFFER3, x ;high table
    txa
    inc a
    A16
    and #$007f
    sta sprid
    asl a
    asl a ;x4
    tax
    bra @loop

@done:
    plp
    rts    
    
    
    
OAM_Meta_V: ;vert flip
.a16
.i16
; spr_x - x 9 bit
; spr_y - y
; A16 = metasprite data address
; X = bank of metasprite data    
 
    php
	AXY16
	sta temp1 ;address of metasprite
	stx temp2
    
    A8
    ldy #1;
    lda [temp1], y
    sta flip_y
    A16
    
    lda sprid
	asl a
    asl a ;x4
    tax
    
    iny ;ldy #2
@loop:
    A8
    lda [temp1], y
    cmp #128 ; end of data
	beq @done
;first byte is rel x (signed)	
	A16
	and #$00ff
	cmp #$0080 ;is negative?
	bcc @pos_x
@neg_x:
	ora #$ff00 ; extend the sign
@pos_x:
	clc
	adc spr_x
    A8
	sta a:OAM_BUFFER, x
;high byte still has 9th bit  
    iny

;************************** added for flip code
    lda flip_y
    sec
    sbc [temp1], y ;y byte
;************************** 

	clc
	adc spr_y	
	sta a:OAM_BUFFER+1, x
	iny
	lda [temp1], y ;tile
	sta a:OAM_BUFFER+2, x
	iny
	lda [temp1], y ;attributes
    
;************************** added for flip code
    eor #$80 ;V FLIP
;************************** 
    
	sta a:OAM_BUFFER+3, x
	iny
	lda [temp1], y ;size, 0 or 2
	iny
	sta spr_h
    xba
    and #1
    ora spr_h
    ldx sprid
    sta a:OAM_BUFFER3, x ;high table
    txa
    inc a
    A16
    and #$007f
    sta sprid
    asl a
    asl a ;x4
    tax
    bra @loop

@done:
    plp
    rts
    
    

OAM_Meta_HV: ;horiz and vert flip
.a16
.i16
; spr_x - x 9 bit
; spr_y - y
; A16 = metasprite data address
; X = bank of metasprite data    
 
    php
	AXY16
	sta temp1 ;address of metasprite
	stx temp2
    
    A8
    lda [temp1]
    sta flip_x
    ldy #1
    lda [temp1],y
    sta flip_y
    A16
    
    lda sprid
	asl a
    asl a ;x4
    tax
    
    iny ;ldy #2
@loop:
    A8
    lda [temp1], y
    cmp #128 ; end of data
	beq @done
;first byte is rel x (signed)	

;************************** added for flip code
    lda flip_x
    sec
    sbc [temp1], y
;**************************

	A16
	and #$00ff
	cmp #$0080 ;is negative?
	bcc @pos_x
@neg_x:
	ora #$ff00 ; extend the sign
@pos_x:
	clc
	adc spr_x
    A8
	sta a:OAM_BUFFER, x
;high byte still has 9th bit    
    iny
    
;************************** added for flip code
    lda flip_y
    sec
    sbc [temp1], y ;y byte
;**************************     
    
	clc
	adc spr_y	
	sta a:OAM_BUFFER+1, x
	iny
	lda [temp1], y ;tile
	sta a:OAM_BUFFER+2, x
	iny
	lda [temp1], y ;attributes
    
;************************** added for flip code
    eor #$C0 ;H+V FLIP
;**************************     
    
	sta a:OAM_BUFFER+3, x
	iny
	lda [temp1], y ;size, 0 or 2
	iny
	sta spr_h
    xba
    and #1
    ora spr_h
    ldx sprid
    sta a:OAM_BUFFER3, x ;high table
    txa
    inc a
    A16
    and #$007f
    sta sprid
    asl a
    asl a ;x4
    tax
    bra @loop

@done:
    plp
    rts    



OAM_Meta_Flip:
.a16
.i16
;this calls one of the other meta sprite functions
;spr_flip has values 0-3, to choose which flip style
;0 = normal
;1 = h flip
;2 = v flip
;3 = hv flip
;AX has the 24 bit address of the
;metasprite data
; spr_x - x 9 bit
; spr_y - y
    XY8
    ldy spr_flip
    XY16
    dey
    bmi @normal
    beq @flip_h
    dey
    beq @flip_v
;else flip all
    jmp OAM_Meta_HV
@normal: 
    jmp OAM_Meta
@flip_h:
    jmp OAM_Meta_H
@flip_v:
    jmp OAM_Meta_V
    
    
    




    
    
	
;Clear_OAM:
;see init.asm
	

	

Meta2_Obj1:
;transfers hitbox data to sprite obj1
;put (16 bit) x in spr_x
;put (8 bit) y in spr_y
;AX = metasprite hitbox HB data	
.a16
.i16
    php
    AXY16
    sta temp1 ;address of metasprite
	stx temp2
    ldy #0
    tya ;clear the upper byte
    A8
    lda [temp1] ;rel x
    A16
    cmp #$0080
    bcc @1
    ora #$ff00 ;sign extend negative
@1:
    clc
    adc spr_x
    and #$01ff
    cmp #$0100
    bcs @skip
    A8
    sta obj1x
    iny ;y = 1
    lda [temp1], y
    clc
    adc spr_y 
;    bcs @skip ;overflow
    sta obj1y
    iny ;y = 2
    lda [temp1], y
    sta obj1w
    iny ;y = 3
    lda [temp1], y
    sta obj1h    
    plp
    rts
@skip:
    A8
    stz obj1w ; 0 = off
    plp
    rts
    


Meta2_Obj2:
;transfers hitbox data to sprite obj2
;put (16 bit) x in spr_x
;put (8 bit) y in spr_y
;AX = metasprite hitbox HB data	
.a16
.i16
    php
    AXY16
    sta temp1 ;address of metasprite
	stx temp2
    ldy #0
    tya ;clear the upper byte
    A8
    lda [temp1] ;rel x
    A16
    cmp #$0080
    bcc @1
    ora #$ff00 ;sign extend negative
@1:
    clc
    adc spr_x
    and #$01ff
    cmp #$0100
    bcs @skip
    A8
    sta obj2x
    iny ;y = 1
    lda [temp1], y
    clc
    adc spr_y 
;    bcs @skip ;overflow
    sta obj2y
    iny ;y = 2
    lda [temp1], y
    sta obj2w
    iny ;y = 3
    lda [temp1], y
    sta obj2h    
    plp
    rts
@skip:
    A8
    stz obj2w ; 0 = off
    plp
    rts


	
Check_Collision:
.a8
.i16
;copy each object's value to these variables and jsr here.
;obj1x: .res 1
;obj1w: .res 1
;obj1y: .res 1
;obj1h: .res 1
;obj2x: .res 1
;obj2w: .res 1
;obj2y: .res 1
;obj2h: .res 1
;returns collision = 1 or 0

;if 1w or 2w == zero, skip
;so, that's how you set it to not collide

	php
	A8
;first check if obj1 R (obj1 x + width) < obj2 L
    lda obj1w
    beq @no
    lda obj2w
    beq @no

	lda obj1x
	clc
	adc obj1w
	cmp obj2x
	bcc @no
		
;now check if obj1 L > obj2 R (obj2 x + width)

	lda obj2x
	clc
	adc obj2w
	cmp obj1x
	bcc @no

;first check if obj1 Bottom (obj1 y + height) < obj2 Top
	
	lda obj1y
	clc
	adc obj1h
	cmp obj2y
	bcc @no
		
;now check if obj1 Top > obj2 Bottom (obj2 y + height)

	lda obj2y
	clc
	adc obj2h
	cmp obj1y
	bcc @no
	
@yes:
	lda #1
	sta collision
	plp
	rts
	
@no:
	stz collision
	plp
	rts












    






;-------------------------------------------
;below are optional versions of the
;metasprite functions, which can
;manually set the attribute values
;for palette and priority
;-------------------------------------------



Combine_A:
;a = palette 0-7
;x = priority 0-3
;and puts it into spr_a
    php
    AXY8
    sta temp1
    txa
    asl a
    asl a
    asl a
    ora temp1
    asl a
    sta spr_a
    plp
    rts
    
    

OAM_Meta_A:
.a16
.i16
; this version has an override for attributes
; palette and priority combined to spr_a
; it error checks spr_a for incorrect bits
; spr_x - x 9 bit
; spr_y - y
; spr_a - attributes (palette and priority)
; A16 = metasprite data address
; X = bank of metasprite data    
 
    php 
    AXY16
	sta temp1 ;address of metasprite
	stx temp2
    
    lda sprid
	asl a
    asl a ;x4
    tax
    
    A8
    lda spr_a
    and #$3e ;only these bits
    sta spr_a
    
    ldy #2 ;0
@loop:
    A8
    lda [temp1], y
    cmp #128 ; end of data
	beq @done
;first byte is rel x (signed)	
	A16
	and #$00ff
	cmp #$0080 ;is negative?
	bcc @pos_x
@neg_x:
	ora #$ff00 ; extend the sign
@pos_x:
	clc
	adc spr_x
    A8
	sta a:OAM_BUFFER, x
;high byte still has 9th bit    
    iny
	lda [temp1], y ;y byte
	clc
	adc spr_y	
	sta a:OAM_BUFFER+1, x
	iny
	lda [temp1], y ;tile
	sta a:OAM_BUFFER+2, x
	iny
	lda [temp1], y ;attributes
    and #$c1
    ora spr_a ;attribute override
	sta a:OAM_BUFFER+3, x
	iny
	lda [temp1], y ;size, 0 or 2
	iny
	sta spr_h
    xba
    and #1
    ora spr_h
    ldx sprid
    sta a:OAM_BUFFER3, x ;high table
    txa
    inc a
    A16
    and #$007f
    sta sprid
    asl a
    asl a ;x4
    tax
    bra @loop

@done:
    plp
    rts
    

OAM_Meta_H_A:
.a16
.i16
; attribute override, and h flipped   
 
    php 
    AXY16
	sta temp1 ;address of metasprite
	stx temp2
    
    lda sprid
	asl a
    asl a ;x4
    tax
    
    A8
    lda spr_a
    and #$3e ;only these bits
    sta spr_a
    
    lda [temp1]
    sta flip_x
    
    ldy #2 ;0
@loop:
    A8
    lda [temp1], y
    cmp #128 ; end of data
	beq @done
;first byte is rel x (signed)	

;************************** added for flip code
    lda flip_x
    sec
    sbc [temp1], y
;**************************

	A16
	and #$00ff
	cmp #$0080 ;is negative?
	bcc @pos_x
@neg_x:
	ora #$ff00 ; extend the sign
@pos_x:
	clc
	adc spr_x
    A8
	sta a:OAM_BUFFER, x
;high byte still has 9th bit    
    iny
	lda [temp1], y ;y byte
	clc
	adc spr_y	
	sta a:OAM_BUFFER+1, x
	iny
	lda [temp1], y ;tile
	sta a:OAM_BUFFER+2, x
	iny
	lda [temp1], y ;attributes
    and #$c1
    ora spr_a ;attribute override
    
;************************** added for flip code
    eor #$40 ;H FLIP
;**************************     
    
    and #$c1
    ora spr_a ;attribute override
	sta a:OAM_BUFFER+3, x
	iny
	lda [temp1], y ;size, 0 or 2
	iny
	sta spr_h
    xba
    and #1
    ora spr_h
    ldx sprid
    sta a:OAM_BUFFER3, x ;high table
    txa
    inc a
    A16
    and #$007f
    sta sprid
    asl a
    asl a ;x4
    tax
    bra @loop

@done:
    plp
    rts    
    

OAM_Meta_V_A:
.a16
.i16
;attribute override, with V flip   
 
    php
	AXY16
	sta temp1 ;address of metasprite
	stx temp2
    
    A8
    ldy #1
    lda [temp1], y
    sta flip_y
    
;   A8
    lda spr_a
    and #$3e ;only these bits
    sta spr_a    
    
    A16
    lda sprid
	asl a
    asl a ;x4
    tax
    
    iny ;ldy #2
@loop:
    A8
    lda [temp1], y
    cmp #128 ; end of data
	beq @done
;first byte is rel x (signed)	
	A16
	and #$00ff
	cmp #$0080 ;is negative?
	bcc @pos_x
@neg_x:
	ora #$ff00 ; extend the sign
@pos_x:
	clc
	adc spr_x
    A8
	sta a:OAM_BUFFER, x
;high byte still has 9th bit  
    iny

;************************** added for flip code
    lda flip_y
    sec
    sbc [temp1], y ;y byte
;************************** 
    
	clc
	adc spr_y	
	sta a:OAM_BUFFER+1, x
	iny
	lda [temp1], y ;tile
	sta a:OAM_BUFFER+2, x
	iny
	lda [temp1], y ;attributes
    and #$c1
    ora spr_a ;attribute override    
    
;************************** added for flip code
    eor #$80 ;V FLIP
;************************** 
    
	sta a:OAM_BUFFER+3, x
	iny
	lda [temp1], y ;size, 0 or 2
	iny
	sta spr_h
    xba
    and #1
    ora spr_h
    ldx sprid
    sta a:OAM_BUFFER3, x ;high table
    txa
    inc a
    A16
    and #$007f
    sta sprid
    asl a
    asl a ;x4
    tax
    bra @loop

@done:
    plp
    rts
    
    

OAM_Meta_HV_A:
.a16
.i16
;attribute override, with H + V flip 
 
    php
	AXY16
	sta temp1 ;address of metasprite
	stx temp2
    
    A8
    lda [temp1]
    sta flip_x
    ldy #1
    lda [temp1],y
    sta flip_y
    
;    A8
    lda spr_a
    and #$3e ;only these bits
    sta spr_a
    
    A16
    lda sprid
	asl a
    asl a ;x4
    tax
    
    iny ;ldy #2
@loop:
    A8
    lda [temp1], y
    cmp #128 ; end of data
	beq @done
;first byte is rel x (signed)	

;************************** added for flip code
    lda flip_x
    sec
    sbc [temp1], y
;**************************


	A16
	and #$00ff
	cmp #$0080 ;is negative?
	bcc @pos_x
@neg_x:
	ora #$ff00 ; extend the sign
@pos_x:
	clc
	adc spr_x
    A8
	sta a:OAM_BUFFER, x
;high byte still has 9th bit    
    iny
    
;************************** added for flip code
    lda flip_y
    sec
    sbc [temp1], y ;y byte
;**************************     
    
	clc
	adc spr_y	
	sta a:OAM_BUFFER+1, x
	iny
	lda [temp1], y ;tile
	sta a:OAM_BUFFER+2, x
	iny
	lda [temp1], y ;attributes
    and #$c1
    ora spr_a ;attribute override 
    
;************************** added for flip code
    eor #$C0 ;H+V FLIP
;**************************     
    
	sta a:OAM_BUFFER+3, x
	iny
	lda [temp1], y ;size, 0 or 2
	iny
	sta spr_h
    xba
    and #1
    ora spr_h
    ldx sprid
    sta a:OAM_BUFFER3, x ;high table
    txa
    inc a
    A16
    and #$007f
    sta sprid
    asl a
    asl a ;x4
    tax
    bra @loop

@done:
    plp
    rts
    
    
    
OAM_Meta_Flip_A:
.a16
.i16
;with attribute override
;this calls one of the other meta sprite functions
;spr_flip has values 0-3, to choose which flip style
;0 = normal
;1 = h flip
;2 = v flip
;3 = hv flip
;AX has the 24 bit address of the
;metasprite data
; spr_x - x 9 bit
; spr_y - y
    XY8
    ldy spr_flip
    XY16
    dey
    bmi @normal
    beq @flip_h
    dey
    beq @flip_v
;else flip all
    jmp OAM_Meta_HV_A
@normal: 
    jmp OAM_Meta_A
@flip_h:
    jmp OAM_Meta_H_A
@flip_v:
    jmp OAM_Meta_V_A
    
    
    
    
    