using System;
using System.Runtime.InteropServices;

namespace ThreeTileRenderer.DataTypes
{
    public enum TileRotation
    {
        None,
        CW90,
        CW180,
        CW270
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tile
    {
        /* |Strict tile
         * |free bits             
         * |                 |rotation number
         * |                 |  |collision \ solid
         * |                 |  ||flip z
         * |                 |  |||flip x
         * |                 |  ||| |id number
         * 0000 0000 0000 0000 0000 0000 0000 0000
         * 32   28   24   20   16   12   8    4
         */

        private const int ROT_MASK = 0x3;
        private const int ROT_MASK_INVERT = ~ROT_MASK;
        private const int ID_MASK = 0xFFF;
        private const int ID_MASK_INVERT = ~ID_MASK;

        private const int FLIP_X_OFFSET = 12;
        private const int FLIP_Y_OFFSET = 13;
        private const int COLLISION_OFFSET = 14;
        private const int ROT_OFFSET = 15;

        [MarshalAs(UnmanagedType.I4)] private int raw;

        public int Id
        {
            get { return raw & ID_MASK; }
            set
            {
#if !TTR_UNSAFE
                if (Id >= 4096 || Id < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value),
                        "Id shoudn't be more than 4096 or less than 0");
                }
#endif
                raw &= ID_MASK_INVERT;
                raw |= value & ID_MASK;
            }
        }

        public bool Collision
        {
            get { return GetBitfield(1 << COLLISION_OFFSET); }
            set { SetBitfield(1 << COLLISION_OFFSET, value); }
        }

        public bool FlipX
        {
            get { return GetBitfield(1 << FLIP_X_OFFSET); }
            set { SetBitfield(1 << FLIP_X_OFFSET, value); }
        }

        public bool FlipY
        {
            get { return GetBitfield(1 << FLIP_Y_OFFSET); }
            set { SetBitfield(1 << FLIP_Y_OFFSET, value); }
        }

        public TileRotation Rotation
        {
            get { return (TileRotation) ((raw >> ROT_OFFSET) & ROT_MASK); }
            set
            {
                // two bits (0..4) 
                // 0: 0 CW 
                // 1: 90 CW
                // 2: 180 CW
                // 3: 270 CW
                raw &= ~(ROT_MASK << ROT_OFFSET);
                raw |= ((int)value & ROT_MASK) << ROT_OFFSET;
            }
        }

        private void SetBitfield(int mask, bool value)
        {
            if (value)
            {
                raw = raw | mask;
            }
            else
            {
                raw = raw & ~mask;
            }
        }

        private bool GetBitfield(int mask)
        {
            return (raw & mask) != 0;
        }

        public override string ToString()
        {
            return string.Format("ID: {0}, Collision: {1}, Flip: {2}, Rotate: {3}\n\n{4}",
                Id,
                Collision,
                FlipX ? FlipY ? "Both" : "X" :
                FlipY ? "Y" : "None",
                new[] {"0 deg", "90 deg", "180 deg", "270 deg"}[(int) Rotation],
                PrintBits(raw));
        }

        private static string PrintBits(int raw)
        {
            var output = new System.Text.StringBuilder();
            var offset = 0;
            
            for (var index = 0; index < FLIP_X_OFFSET; index++) {
                output.Append((raw >> offset++) & 1);
            }
            output.Append(' ');
            output.Append((raw >> offset++) & 1);
            output.Append(' ');
            output.Append((raw >> offset++) & 1);
            output.Append(' ');
            output.Append((raw >> offset++) & 1);
            output.Append(' ');
            for (var index = 0; index < 2; index++) {
                output.Append((raw >> offset++) & 1);
            }
            
            output.Append(' ');
            for (var index = 0; index < 5; index++) {
                output.Append((raw >> offset++) & 1);
            }
            
            output.Append(' ');
            for (var index = 0; index < 5; index++) {
                output.Append((raw >> offset++) & 1);
            }
            
            output.Append(' ');
            output.Append(' ');
            while (offset < 32) {
                output.Append((raw >> offset++) & 1);
            }
            
            var charArray = output.ToString().ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}