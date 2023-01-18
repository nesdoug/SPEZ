; first
; Sprite mode is 8 x 8 and 16 x 16
.byte $02, $00 ; hitbox x,y
.byte $1A, $1F ; hitbox w,h
Meta_00:
.byte $10, $10 ; flip x,y
.byte $00, $00, $00, $20, $02
.byte $10, $00, $02, $20, $02
.byte $00, $10, $20, $20, $02
.byte $10, $10, $22, $20, $02
.byte $80 ;end of data


; second
; Sprite mode is 8 x 8 and 16 x 16
.byte $F2, $F0 ; hitbox x,y
.byte $1A, $3F ; hitbox w,h
Meta_01:
.byte $F0, $10 ; flip x,y
.byte $F0, $F0, $00, $20, $02
.byte $00, $F0, $02, $20, $02
.byte $F0, $00, $20, $20, $02
.byte $00, $00, $22, $20, $02
.byte $F0, $10, $00, $20, $02
.byte $00, $10, $02, $20, $02
.byte $F0, $20, $20, $20, $02
.byte $00, $20, $22, $20, $02
.byte $80 ;end of data




