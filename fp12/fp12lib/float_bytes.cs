using System;

namespace fp12lib {
    public struct float_bytes {
        private const uint SIGN_MASK = 0b10000000_00000000_00000000_00000000;
        private const uint EXPONENT_MASK = 0b01111111_10000000_00000000_00000000;
        private const uint MANTISSA_MASK = 0b00000000_01111111_11111111_11111111;

        private const int BIAS = 127;
        private const int EXPONENT_BIT_COUNT = 8;
        private const int MANTISSA_BIT_COUNT = 23;

        // Float bytes in **big endian** format.
        // S (1 bit) | E (8 bit) | M (23 bit)
        private uint __bytes;

        public uint sign =>
            ((__bytes & SIGN_MASK) >> (EXPONENT_BIT_COUNT + MANTISSA_BIT_COUNT));

        public uint exponent =>
            ((__bytes & EXPONENT_MASK) >> MANTISSA_BIT_COUNT);

        public int unbiased_exponent =>
            ((int)exponent - BIAS);

        public uint mantissa =>
            (__bytes & MANTISSA_MASK);

        public float_bytes(float f) {
            var tmp = BitConverter.GetBytes(f);

            __bytes = BitConverter.ToUInt32(tmp, 0);
        }

        private float_bytes(uint bytes) {
            __bytes = bytes;
        }        

        public float_bytes with_sign(uint new_sign) {
            uint new_bytes = __bytes & ~SIGN_MASK;
            new_bytes |= ((new_sign == 0 ? 0u : 1u) << (EXPONENT_BIT_COUNT + MANTISSA_BIT_COUNT));
            return new float_bytes(new_bytes);
        }

        public float_bytes with_unbiased_exponent(int exponent) {
            if (exponent < -126 || exponent > 127)
                throw new ArgumentOutOfRangeException("Invalid exponent:"  + exponent + ".");

            uint new_bytes = __bytes & ~EXPONENT_MASK;
            
            uint biased = (uint)(exponent + BIAS);
            new_bytes |= (biased << MANTISSA_BIT_COUNT);

            return new float_bytes(new_bytes);
        }

        public float_bytes with_mantissa(uint mantissa) {
            if (mantissa < 0 || mantissa >= (2 << (MANTISSA_BIT_COUNT + 1)))
                throw new ArgumentOutOfRangeException("Invalid mantissa: " + mantissa + ".");
            
            uint new_bytes = __bytes & ~MANTISSA_MASK;
            new_bytes |= mantissa;

            return new float_bytes(new_bytes);
 
        }

        public float to_float() {
            var tmp = BitConverter.GetBytes(__bytes);

            return BitConverter.ToSingle(tmp, 0);
        }
    }
}