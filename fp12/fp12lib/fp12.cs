namespace fp12lib {
    public struct fp12 {
        /*
         * Implementation of 12-bit floating point numbers.
         * __value field structure is as follows:
         * 
         * |        MSB        |        LSB       |
         *  X X X X [ S | E E E E | M M M M M M M ]
         * 
         * X - unused
         * S - sign bit, 0 - positive, 1 - negative.
         * E - exponent bits. Exponent has range [0; 15].
         *     Value E=0 is reserved to represent 0 and
         *     denormalized numbers (0.MMMMMMM).
         *     E=15 is reserved to represent NaN and infinities.
         *     Otherwise E represent a valid exponend of normalized
         *     values (1.MMMMMMM)
         * M - mantissa bits (1.MMMMMMM)
         * 
         * Normalized values are represented as follows:
         *   value = (-1)^S * 2^(E - B) * (1.MMMMMMM)
         * where B=7 is bias.
         * Values with E=0 are used to represent denormalized values:
         *   denorm_value = (-1)^S * 2^(-6) * 0.MMMMMMM
         */

        internal const int MANTISSA_BIT_COUNT = 7;
        internal const int EXPONENT_BIT_COUNT = 4;

        internal const int BIAS = 7;

        private ushort __value;

        public fp12(ushort value) { __value = value; }

        private const ushort SIGN_MASK = 0b00001000_00000000;
        public int __sign => 
            ((__value & SIGN_MASK) >> (EXPONENT_BIT_COUNT + MANTISSA_BIT_COUNT));

        internal const int SIGN_POSITIVE = 0;
        internal const int SIGN_NEGATIVE = 1;

        public const ushort EXPONENT_MASK = 0b00000111_10000000;
        public int __exponent =>
            ((__value & EXPONENT_MASK) >> MANTISSA_BIT_COUNT);

        public int __unbiased_exponent =>
            (__exponent - BIAS);

        public const ushort MANTISSA_MASK = 0b00000000_01111111;
        public int __mantissa =>
            (__value & MANTISSA_MASK);

        internal const int EXPONENT_MIN = 1;
        internal const int EXPONENT_MAX = 14;

        public bool is_nan =>
            (__exponent == (EXPONENT_MAX + 1)) &&
            (__mantissa != 0);

        public bool is_positive_infinity =>
            (__exponent == (EXPONENT_MAX + 1)) &&
            (__mantissa == 0) &&
            (__sign == SIGN_POSITIVE);

        public bool is_negative_infinity =>
            (__exponent == (EXPONENT_MAX + 1)) &&
            (__mantissa == 0) &&
            (__sign == SIGN_NEGATIVE);

        public static readonly fp12 NaN = new fp12(0b00000111_11111111);
        public static readonly fp12 POSTIVE_INFINITY = new fp12(0b00000111_10000000);
        public static readonly fp12 NEGATIVE_INFINITY = new fp12(0b00001111_10000000);

        // Conversion fp12 -> float
        public static explicit operator float(fp12 value) {
            // TODO: Implement infinities and NaNs

            float f = new float_bytes()
                .with_sign((uint) value.__sign)
                .with_unbiased_exponent(value.__unbiased_exponent)
                .with_mantissa(((uint)value.__mantissa) << (float_bytes.MANTISSA_BIT_COUNT - MANTISSA_BIT_COUNT))
                .to_float();

            return f;
        }

        public static explicit operator fp12(float f) {
            // TODO: Infs and NaN

            var fb = new float_bytes(f);


            // TODO: Check out of range

            ushort fp12 = 0;

            // Sign
            if (fb.sign == 1) {
                fp12 |= 0b10000000_00000000;
            }

            // Exponent
            uint exponent = (uint)(fb.unbiased_exponent + BIAS);
            fp12 |= (ushort)((exponent & (EXPONENT_MASK >> MANTISSA_BIT_COUNT)) << MANTISSA_BIT_COUNT);

            // Mantissa
            uint mantissa = fb.mantissa >> (float_bytes.MANTISSA_BIT_COUNT - MANTISSA_BIT_COUNT);
            fp12 |= (ushort)mantissa;

            return new fp12(fp12);
        }
    }
}