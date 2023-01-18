; example SNES code

.p816
.smart



.include "regs.asm"
.include "variables.asm"
.include "macros.asm"
.include "init.asm"
.include "library.asm"





.segment "CODE"

; enters here in forced blank
Main:
.a16 ; the setting from init code
.i16
	phk
	plb
	
	
; COPY PALETTES to PAL_BUFFER	
;	BLOCK_MOVE  length, src_addr, dst_addr
	BLOCK_MOVE  512, BG_Palette, PAL_BUFFER
	A8 ;block move will put AXY16. Undo that.
	
; DMA from PAL_BUFFER to CGRAM
	jsr DMA_Palette ; in init.asm
	
	
; DMA from Tiles to VRAM	
	lda #V_INC_1 ; the value $80
	sta VMAIN  ; $2115 = set the increment mode +1

	DMA_VRAM  32768, BG_Tiles, $0000
	
	
; DMA from Spr_Tiles to VRAM $4000
	
	DMA_VRAM  (End_Spr_Tiles - Spr_Tiles), Spr_Tiles, $4000
	
	
; DMA from Map1 to VRAM	$6000 
	
	DMA_VRAM  $800, Map1, $6000



	
	
	
	A8

	lda #1 ; mode 1, tilesize 8x8 all
	sta BGMODE ; $2105
	
	stz BG12NBA ; $210b BG 1 and 2 TILES at VRAM address $0000
	
	lda #$60 ; bg1 map at VRAM address $6000
	sta BG1SC ; $2107
	
	lda #$68 ; bg2 map at VRAM address $6800
	sta BG2SC ; $2108
	
	lda #$70 ; bg3 map at VRAM address $7000
	sta BG3SC ; $2109	
	
	lda #2 ;sprite tiles at $4000
	sta OBSEL ;= $2101

;allow everything on the main screen	
	lda #BG1_ON|SPR_ON ; $11
	sta TM ; $212c
    
    lda #$ff
    sta BG1VOFS ;set BG1 Y scroll to -1
    sta BG1VOFS
	
	;turn on NMI interrupts and auto-controller reads
	lda #NMI_ON|AUTO_JOY_ON
	sta NMITIMEN ;$4200
	
	lda #FULL_BRIGHT ; $0f = turn the screen on, full brighness
	sta INIDISP ; $2100
    sta bright_var


;set some initial values
	lda #$10
	sta user_x
	sta user_y
	
    ldy #00
    tyx ;zero
@load_spr_loop:
    lda Object_List, y
    sta spr_obj_x, x
    iny
    lda Object_List, y
    sta spr_obj_y, x
    iny
    lda Object_List, y
    sta spr_obj_ms, x
    iny
    lda Object_List, y
    sta spr_obj_fl, x
    iny
    inx
    cpy #32
    bcc @load_spr_loop
    jmp Infinite_Loop
    
Object_List:
;x, y, metasprite, x/y flip status
;0=no flip
;1=x flip
;2=y flip
;3=xy flip
.byte $30,$20,0,0
.byte $60,$20,0,1
.byte $90,$20,0,2
.byte $c0,$20,0,3
.byte $70,$60,2,0
.byte $80,$80,2,1
.byte $90,$a0,2,2
.byte $a0,$c0,2,3   
    
	
	
Infinite_Loop:	
;game loop a8 xy16
	A8
	XY16
	jsr Wait_NMI ;wait for the beginning of v-blank
	jsr DMA_OAM  ;copy the OAM_BUFFER to the OAM
    lda bright_var
    sta INIDISP ; $2100
	jsr Pad_Poll ;read controllers
	jsr Clear_OAM

;	A16
;	lda pad1
;	and #KEY_DOWN


	AXY16
;move the user's box
    lda pad1
    and #KEY_UP
    beq @not_up
    A8
    dec user_y
    A16
@not_up: 

    lda pad1
    and #KEY_DOWN
    beq @not_down
    A8
    inc user_y
    A16
@not_down:
    
    lda pad1
    and #KEY_LEFT
    beq @not_left
;    A8
    dec user_x ; allow 9 bits 
;    A16
@not_left: 

    lda pad1
    and #KEY_RIGHT
    beq @not_right
;    A8
    inc user_x
;    A16
@not_right:



	jsr Draw_Sprites
	jmp Infinite_Loop
	
	
	

	
	
	

	
	

	
Draw_Sprites:
	php
	
	A8
	XY16
	stz sprid ;0-127


;put one sprite  
    A8
    lda #$10 ;x 8 bits
    sta spr_x
    stz spr_x+1
    lda #$b0 ;y
    sta spr_y
    lda #6 ;tile
    sta spr_c
    lda #SPR_PRIOR_2+SPR_PAL_3 ;attributes
    sta spr_a
;    lda #0 ;small
    stz spr_sz
    jsr OAM_Spr
    
    ;A8
    lda in_nmi
    and #$10
    beq :+
    jsr draw_normal
:    
    lda in_nmi
    and #$10
    bne :+
    jsr draw_flip
:    

   
;put one meta sprite - the user controlled one
    A8
	ldx user_x
	stx spr_x
	lda user_y
	sta spr_y 
	AXY16
	lda #.loword(Meta_01) ;right
	ldx #^Meta_01
	jsr OAM_Meta
    
    
;loop through each of 8 sprite objects
    ldx #0
    stx loop16
@loop:
    A8
    lda spr_obj_x, x
    sta spr_x
    stz spr_x+1 ;always zero
    lda spr_obj_y, x
    sta spr_y
    lda spr_obj_fl, x ; flip status
    sta spr_flip
    lda spr_obj_ms, x
    AXY16
    and #$00ff ;clear top byte
    asl a ;x2
    tay
    lda Meta_List, y
    ldx #^Meta_01
    jsr OAM_Meta_Flip
    inc loop16
    ldx loop16
    cpx #8
    bcc @loop
    


;meta sprites - with attribute override	
    A8
	lda #$20
	sta spr_x
    stz spr_x+1 ;always zero
	lda #$b0
	sta spr_y 
    lda #SPR_PRIOR_3+SPR_PAL_1
    sta spr_a ;attributes
	AXY16
	lda #.loword(Meta_00)
	ldx #^Meta_00
	jsr OAM_Meta_A
    
    A8
    lda #$30
	sta spr_x
    lda #SPR_PRIOR_3+SPR_PAL_2
    sta spr_a ;attributes
	AXY16
	lda #.loword(Meta_00)
	ldx #^Meta_00
	jsr OAM_Meta_H_A

    A8
    lda #$40
	sta spr_x
    lda #SPR_PRIOR_3+SPR_PAL_3
    sta spr_a ;attributes
	AXY16
	lda #.loword(Meta_00)
	ldx #^Meta_00
	jsr OAM_Meta_V_A
    
    A8
    lda #$50
	sta spr_x
    lda #SPR_PRIOR_3+SPR_PAL_2
    sta spr_a ;attributes
	AXY16
	lda #.loword(Meta_00)
	ldx #^Meta_00
	jsr OAM_Meta_HV_A


    A8
    lda user_x+1
    and #1
    bne @1 ;skip if negative high byte
    
    jsr Check_Collz
    bcs @2
@1:
    lda #FULL_BRIGHT
    bra @3
@2:    
    lda #7 ;half bright
@3:
    sta bright_var
    jsr Process_OAM_HT

	plp
	rts
	


draw_normal:
.a8
.i16
    php
    A8
    stz spr_flip
    stz spr_x+1
    lda #$10
    sta spr_x
    lda #$30
    sta spr_y
    A16
    lda #.loword(Meta_03)
	ldx #^Meta_03
    jsr OAM_Meta_Flip
    
    lda #$10
    sta spr_x
    lda #$50
    sta spr_y
    A16
    lda #.loword(Meta_04)
	ldx #^Meta_04
    jsr OAM_Meta_Flip
    plp
    rts

draw_flip:
.a8
.i16
    php
    A8
    lda #1 ;h flip;
    sta spr_flip
    stz spr_x+1
    lda #$10
    sta spr_x
    lda #$30
    sta spr_y
    A16
    lda #.loword(Meta_03)
	ldx #^Meta_03
    jsr OAM_Meta_Flip
    
    lda #2 ;v flip;
    sta spr_flip
    stz spr_x+1
    lda #$10
    sta spr_x
    lda #$50
    sta spr_y
    A16
    lda #.loword(Meta_04)
	ldx #^Meta_04
    jsr OAM_Meta_Flip
    plp
    rts




Check_Collz:
;handle sprite collision
;a8 xy16
.a8
.i16
    php
    A8
    XY16
    lda user_x
    sta spr_x
    lda user_y
    sta spr_y
    A16
    lda #.loword(Meta_01_HB)
	ldx #^Meta_01_HB
    jsr Meta2_Obj1

    ldy #0;zero
    sty loop16
@loop2:
;y is loop16
    A8
    lda spr_obj_x, y
    sta spr_x
    stz spr_x+1
    lda spr_obj_y, y
    sta spr_y
    lda spr_obj_ms, y
    AXY16
    and #$00ff ;clear top byte
    asl a ;x2
    tay
    lda Meta_List_HB, y ;list of hitboxes
    ldx #^Meta_01_HB
    jsr Meta2_Obj2
;    sec
;    sbc #4 ;6
;    sta temp1
;    lda #^Meta_01
;    sta temp2
;    A8
;    lda [temp1]
;    clc
;    adc obj2x
;    sta obj2x
;    inc temp1
;    lda [temp1]
;    clc
;    adc obj2y
;    sta obj2y
;    inc temp1
;    lda [temp1]
;    sta obj2w
;    inc temp1
;    lda [temp1]
;    sta obj2h
    jsr Check_Collision
    A8
    lda collision
    bne @yes
;    iny
    inc loop16
    ldy loop16
    cpy #8
    bcc @loop2
@no:
    plp
    clc
    rts
@yes:
    plp
    sec
    rts
	
	
Wait_NMI:
.a8
.i16
;should work fine regardless of size of A
	lda in_nmi ;load A register with previous in_nmi
@check_again:	
	WAI ;wait for an interrupt
	cmp in_nmi	;compare A to current in_nmi
				;wait for it to change
				;make sure it was an nmi interrupt
	beq @check_again
	rts

	

	
	
Pad_Poll:
.a8
.i16
; reads both controllers to pad1, pad1_new, pad2, pad2_new
; auto controller reads done, call this once per main loop
; copies the current controller reads to these variables
; pad1, pad1_new, pad2, pad2_new (all 16 bit)
	php
	A8
@wait:
; wait till auto-controller reads are done
	lda $4212
	lsr a
	bcs @wait
	
	A16
	lda pad1
	sta temp1 ; save last frame
	lda $4218 ; controller 1
	sta pad1
	eor temp1
	and pad1
	sta pad1_new
	
	lda pad2
	sta temp1 ; save last frame
	lda $421a ; controller 2
	sta pad2
	eor temp1
	and pad2
	sta pad2_new
	plp
	rts
	

.include "Sprites/Metasprites.asm"	
;Meta_00 tall
;Meta_01 square
;Meta_02 wide

;metasprites	
Meta_List:
.addr Meta_00, Meta_01, Meta_02

;hitboxes
Meta_List_HB:
.addr Meta_00_HB, Meta_01_HB, Meta_02_HB

	



.include "header.asm"	



.segment "RODATA1"

BG_Tiles:
.incbin "Background/BG_Tiles.chr"


.segment "RODATA2"

Spr_Tiles:
.incbin "Sprites/Sprites.chr"
End_Spr_Tiles:

BG_Palette:
.incbin "Background/BG.pal"

Spr_Palette:
.incbin "Sprites/Sprites.pal"

Map1:
.incbin "Background/BG1.map"

