namespace SNSPED
{
    public class Metasprites
    {
        public int[] tile = new int[Form1.MAX_SPRITE];
        //tile can be value 0-255
        public int[] set = new int[Form1.MAX_SPRITE]; // 0-1

        public int[] rel_x = new int[Form1.MAX_SPRITE]; //-64 to 64
                                           // (-128 means end of set)
        public int[] rel_y = new int[Form1.MAX_SPRITE]; //-64 to 64

        public int[] palette = new int[Form1.MAX_SPRITE]; // 0-7
        public int[] h_flip = new int[Form1.MAX_SPRITE]; // 0-1
        public int[] v_flip = new int[Form1.MAX_SPRITE]; // 0-1
        public int[] size = new int[Form1.MAX_SPRITE]; // 0-1

        // just 1 priority for the whole metasprite
        public int priority; // 0-3, optional
        public int sprite_count; // starts at zero = null

        public int hitbox_x;
        public int hitbox_y;
        public int hitbox_x2; // convert to width later
        public int hitbox_y2; // convert to height later

        public string name;

        

        public void Metasprite()
        {
            // nothing
        }
    }

    
}