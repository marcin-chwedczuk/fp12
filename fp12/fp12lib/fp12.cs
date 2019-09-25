using System;

namespace fp12lib {
    public struct fp12: IEquatable<fp12> {
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

        public fp12(uint sign, uint exponent, uint mantissa) {
            sign = (sign == 0) ? 0u : 0b0000_1000__0000_0000u;
            sign <<= (EXPONENT_BIT_COUNT + MANTISSA_BIT_COUNT);

            exponent = exponent & 0b0000_1111u;
            exponent <<= MANTISSA_BIT_COUNT;

            mantissa = mantissa & 0b0111_1111;

            __value = (ushort)(sign | exponent | mantissa);
        }

        private const ushort SIGN_MASK = 0b0000_1000__0000_0000;
        public int __sign => 
            ((__value & SIGN_MASK) >> (EXPONENT_BIT_COUNT + MANTISSA_BIT_COUNT));

        internal const int SIGN_POSITIVE = 0;
        internal const int SIGN_NEGATIVE = 1;

        public const ushort EXPONENT_MASK = 0b0000_0111__1000_0000;
        public int __exponent =>
            ((__value & EXPONENT_MASK) >> MANTISSA_BIT_COUNT);

        public int __unbiased_exponent =>
            (__exponent - BIAS);

        public const ushort MANTISSA_MASK = 0b0000_0000__0111_1111;
        public int __mantissa =>
            (__value & MANTISSA_MASK);

        // Mantissa with leading 1 or 0 (in case of denormalized values).
        public uint __full_mantissa {
            get {
                if (is_denormalized) {
                    return (uint)__mantissa;
                }
                else {
                    return (uint)__mantissa | (1u << MANTISSA_BIT_COUNT);
                }
            }
        }

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

        public bool is_denormalized =>
            (__exponent < EXPONENT_MIN) &&
            (__mantissa != 0);

        public fp12 abs() {
            if (this.is_nan) return this;

            // This handles +/- Inf too.
            ushort new_value = __value;
            new_value = (ushort) (new_value & ~SIGN_MASK);
            return new fp12(new_value);
        }

        public bool is_zero() {
            return (this.__value == POSITIVE_ZERO.__value)
                || (this.__value == NEGATIVE_ZERO.__value);
        }

        public bool Equals(fp12 other) {
            return (this == other);
        }

        public override string ToString() {
            return ((float)this).ToString("R") + "fp";
        }
 
        public static readonly fp12 NaN = new fp12(0b00000111_11111111);
        public static readonly fp12 POSTIVE_INFINITY = new fp12(0b00000111_10000000);
        public static readonly fp12 NEGATIVE_INFINITY = new fp12(0b00001111_10000000);

        public static readonly fp12 MAX_POSITIVE_VALUE = new fp12(0b0000_0111__0111_1111);
        public static readonly fp12 MIN_POSITIVE_VALUE = new fp12(0b0000_0000__0000_0001);

        public static readonly fp12 POSITIVE_ZERO = new fp12(0b0000_0000__0000_0000);
        public static readonly fp12 NEGATIVE_ZERO = new fp12(0b0000_1000__0000_0000);

        public static bool operator ==(fp12 left, fp12 right) {
            if (left.is_nan || right.is_nan) return false;

            if (left.__value == right.__value) return true;

            // +0 == -0 and -0 == +0
            if (left.is_zero() && right.is_zero()) return true;

            return false;
        }

        public static bool operator !=(fp12 left, fp12 right) {
            if (left.is_nan || right.is_nan) return false;

            return !(left == right);
        }

        public static bool operator >(fp12 left, fp12 right) {
            if (left.is_nan || right.is_nan) return false;

            if (left.is_positive_infinity && right.is_negative_infinity) return true;
            if (left.is_negative_infinity && right.is_positive_infinity) return false;

            if (left.is_positive_infinity && right.is_positive_infinity) return false;
            if (left.is_negative_infinity && right.is_negative_infinity) return false;

            if (left.is_positive_infinity || right.is_negative_infinity) return true;
            if (left.is_negative_infinity || right.is_positive_infinity) return false;

            // Finite values here.
            
            // +/-0 > +/-0
            if (left.is_zero() && right.is_zero()) return false;

            if (left.__sign == SIGN_POSITIVE && right.__sign == SIGN_NEGATIVE) return true;
            if (left.__sign == SIGN_NEGATIVE && right.__sign == SIGN_POSITIVE) return false;

            // The same sign from here on.

            bool leftBigger = (left.__exponent > right.__exponent)
                            || ((left.__exponent == right.__exponent) && (left.__full_mantissa > right.__full_mantissa));

            if (leftBigger) return (left.__sign == SIGN_POSITIVE);

            bool rightBigger = (right.__exponent > left.__exponent)
                            || ((right.__exponent == left.__exponent) && (right.__full_mantissa > left.__full_mantissa));

            if (rightBigger) return (right.__sign == SIGN_NEGATIVE);

            return false;
        }

        public static bool operator <(fp12 left, fp12 right) {
            return false;
        }

        // Conversion fp12 -> float
        public static explicit operator float(fp12 value) {
            if (value.is_nan) { return float.NaN; }
            if (value.is_positive_infinity) { return float.PositiveInfinity; }
            if (value.is_negative_infinity) { return float.NegativeInfinity; }
            if (value == POSITIVE_ZERO) { return 0.0f; }

            uint fMantissa = ((uint)value.__mantissa) << (float_bytes.MANTISSA_BIT_COUNT - MANTISSA_BIT_COUNT);
            int fUnbiasedExponent = value.__unbiased_exponent;

            if (value.is_denormalized) {
                // Shift mantissa left untill we get a 1 at the
                // begining to get a normalized value.
                // Float has much broader range so there
                // should be no problem with exponend out of range.
                // TODO: Check above

                uint m = (uint) value.__mantissa;
                while ((m & (1u << MANTISSA_BIT_COUNT)) == 0) {
                    m <<= 1;
                    fUnbiasedExponent--;
                }

                // Clear top '1' that is implicit.
                m &= ~(1u << MANTISSA_BIT_COUNT);

                fMantissa = m;
            }

            float f = new float_bytes()
                .with_sign((uint) value.__sign)
                .with_unbiased_exponent(fUnbiasedExponent)
                .with_mantissa(fMantissa)
                .to_float();

            return f;
        }

        public static explicit operator fp12(float f) {
            if (float.IsNaN(f)) { return NaN; }
            if (float.IsPositiveInfinity(f)) { return POSTIVE_INFINITY; }
            if (float.IsNegativeInfinity(f)) { return NEGATIVE_INFINITY; }

            
            var fb = new float_bytes(f);

            // TODO: Denormalized values
            // TODO: Check out of range

            ushort fp12 = 0;

            // Sign
            if (fb.sign == 1) {
                fp12 |= 0b00001000_00000000;
            }

            // Exponent
            uint exponent = (uint)(fb.unbiased_exponent + BIAS);
            fp12 |= (ushort)((exponent & (EXPONENT_MASK >> MANTISSA_BIT_COUNT)) << MANTISSA_BIT_COUNT);

            // Mantissa
            uint mantissa = fb.mantissa >> (float_bytes.MANTISSA_BIT_COUNT - MANTISSA_BIT_COUNT);
            fp12 |= (ushort)mantissa;

            return new fp12(fp12);
        }

        public static fp12 operator +(fp12 left, fp12 right) {
            if (left.is_nan || right.is_nan) return NaN;
            if (left.is_positive_infinity && right.is_negative_infinity) return NaN;
            if (left.is_negative_infinity && right.is_positive_infinity) return NaN;

            if (left.is_positive_infinity && right.is_positive_infinity) return POSTIVE_INFINITY;
            if (left.is_negative_infinity && right.is_negative_infinity) return NEGATIVE_INFINITY;

            if (left.is_positive_infinity || right.is_positive_infinity) return POSTIVE_INFINITY;
            if (left.is_negative_infinity || right.is_negative_infinity) return NEGATIVE_INFINITY;

            uint lm = left.__full_mantissa;
            uint rm = right.__full_mantissa;

            int lexp = left.__exponent;
            int rexp = right.__exponent;

            int sumexp = Math.Max(lexp, rexp);

            // Make exponents exal by changing to
            // higher magnitute exponent.
            if (lexp < rexp) {
                while (lexp < rexp) {
                    lm >>= 1;
                    lexp++;
                }
            }
            else if (lexp > rexp) {
                while (rexp < lexp) {
                    rm >>= 1;
                    rexp++;
                }
            }

            // Mantissas may be normalized (with 1s at the beginning)
            // or denormalized here.

            uint MANTISSA_IMPLICIT_1 = 0b0000_0000__1000_0000u;

            if (left.__sign == right.__sign) {
                // Unnormalized mantissa because of possible
                // sum of the 1s at the beginnings of the mantissas.
                uint summ = lm + rm;

                // Mantissa too big
                while (summ > (MANTISSA_MASK + MANTISSA_IMPLICIT_1)) {
                    summ >>= 1;
                    sumexp++;
                }

                // Mantissa too small (we have either 0 or denormalized mantissa).
                while ((summ < MANTISSA_IMPLICIT_1) && sumexp > 0) {
                    summ <<= 1;
                    sumexp--;
                }

                if (sumexp > EXPONENT_MAX) {
                    if (left.__sign == SIGN_POSITIVE) { return POSTIVE_INFINITY; }
                    else { return NEGATIVE_INFINITY; }
                }

                // Remove start 1 from the mantissa (if it exists).
                summ = summ & ~MANTISSA_IMPLICIT_1;

                return new fp12((uint)left.__sign, (uint)sumexp, summ);
            }

            // Different signs we will deduct the smaller value
            // from the bigger one.
            throw new NotImplementedException("");

            return fp12.NaN;
        }
   }
}