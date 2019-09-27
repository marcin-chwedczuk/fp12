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
        internal const int FULL_MANTISSA_BIT_COUNT = MANTISSA_BIT_COUNT + 1;
        internal const int EXPONENT_BIT_COUNT = 4;

        internal const int BIAS = 7;

        private ushort __value;

        public fp12(ushort value) { __value = value; }

        public fp12(uint sign, uint exponent, uint mantissa) {
            sign = (sign == 0) ? 0u : 0b0000_1000__0000_0000u;

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
            is_denormalized ? (EXPONENT_MIN - BIAS) : (__exponent - BIAS);

        public const ushort MANTISSA_MASK = 0b0000_0000__0111_1111;
        public int __mantissa =>
            (__value & MANTISSA_MASK);
        
        private const uint MANTISSA_IMPLICIT_1 = 0b0000_0000__1000_0000u;

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
            (__exponent < EXPONENT_MIN);

        public fp12 abs() {
            if (this.is_nan) return this;

            // This handles +/- Inf too.
            ushort new_value = __value;
            new_value = (ushort) (new_value & ~SIGN_MASK);
            return new fp12(new_value);
        }

        public bool is_positive_zero =>
            (this.__value == POSITIVE_ZERO.__value);

        public bool is_negative_zero =>
            (this.__value == NEGATIVE_ZERO.__value);

        public bool is_zero() =>
            is_positive_zero || is_negative_zero;

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

            if (value.is_positive_zero) { return +0.0f; }
            if (value.is_negative_zero) { return -0.0f; }

            uint fMantissa = ((uint)value.__mantissa) << (float_bytes.MANTISSA_BIT_COUNT - MANTISSA_BIT_COUNT);
            int fUnbiasedExponent = value.__unbiased_exponent;

            if (value.is_denormalized && (fMantissa != 0)) {
                // Shift mantissa left until we get a 1 at the
                // begining to get a normalized value.
                // Float has much broader range so there
                // should be no problem with exponend out of range.
                // TODO: Check above

                uint m = fMantissa;
                while ((m & float_bytes.MANTISSA_IMPLICIT_1) == 0) {
                    m <<= 1;
                    fUnbiasedExponent--;
                }

                // Clear top '1' that is implicit.
                m &= ~float_bytes.MANTISSA_IMPLICIT_1;

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

            // Sign
            uint sign = fb.sign == 0 ? 0u : 1u;

            // Exponent
            int exponent = fb.unbiased_exponent + BIAS;
            if (exponent > EXPONENT_MAX) {
                // OVERFLOW
                return sign == 0 ? POSTIVE_INFINITY : NEGATIVE_INFINITY;
            }

            uint full_mantissa = fb.full_mantissa >> (float_bytes.MANTISSA_BIT_COUNT - MANTISSA_BIT_COUNT);

            if (exponent == 0) {
                // Denormalized values are in form
                // 0.MMMMM x 2-6
                // Value here is in form:
                // 1.XXXX x 2-7
                // We just need to shift it right
                full_mantissa >>= 1;
            }
            else if (exponent < 0) {
                // Convert to denormalized value if possible
                while ((exponent < 0) && (full_mantissa > 0)) {
                    exponent++;
                    full_mantissa >>= 1;
                }

                // We have value in form:
                // 0.XXXXX x 2-7
                // Denormalized values are multplied by 2-6, we
                // need to adjust a*2^-7 = (a/2)*2^-6
                full_mantissa >>= 1;
 
                if (exponent < 0 || full_mantissa == 0) {
                    // UNDERFLOW
                    return sign == 0 ? POSITIVE_ZERO : NEGATIVE_ZERO;
                }
           }

            // Remove leading 1 from mantissa
            full_mantissa &= ~MANTISSA_IMPLICIT_1;

            return new fp12(sign, (uint)exponent, full_mantissa);
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
            uint subtractM;
            uint signM;
            if (lm >= rm) {
                subtractM = lm - rm;
                signM = (uint) left.__sign;
            }
            else {
                subtractM = rm - lm;
                signM = (uint) right.__sign;
            }

            // Mantissa may may be too small. Normalized up to 2^-6
            while ((subtractM < MANTISSA_IMPLICIT_1) && sumexp > 1) {
                subtractM <<= 1;
                sumexp--;
            }

            // If there is no 1 at the mantissa beginning
            // then we are in denormalized range:
            if ((subtractM & MANTISSA_IMPLICIT_1) == 0) {
                sumexp = 0; // mark as denormalized
            }

            // Remove start 1 from the mantissa (if it exists).
            subtractM &= ~MANTISSA_IMPLICIT_1;

            return new fp12(signM, (uint)sumexp, subtractM);
        }

        public static fp12 operator -(fp12 value) {
            return new fp12(negate_sign(value.__sign), (uint)value.__exponent, (uint)value.__mantissa);
        }

        public static fp12 operator -(fp12 left, fp12 right) {
            return left + (-right);
        }

        public static fp12 operator *(fp12 left, fp12 right) {
            // TODO: NaNs + Infinities

            uint lm = left.__full_mantissa;
            uint rm = right.__full_mantissa;

            int lexp = left.__unbiased_exponent;
            int rexp = right.__unbiased_exponent;

            // Mantissas are 7-bit long, we can safely multiply them
            // using integer multiplication
            uint mult_full_mantissa = lm * rm;
            int mult_unbiased_exp = lexp + rexp;
            uint mult_sign = (left.__sign == right.__sign) ? 0u : 1u;

            if (mult_full_mantissa == 0) {
                return mult_sign == 0u ? POSITIVE_ZERO : NEGATIVE_ZERO;
            }

            // To avoid precission lost, we will try to operate on
            // denormalized values mantissa - before rejecting extra bits.
            uint MANTISSA_SQ_IMPLICIT_1 = MANTISSA_IMPLICIT_1 * MANTISSA_IMPLICIT_1;
            while (mult_full_mantissa < MANTISSA_SQ_IMPLICIT_1) {
                mult_full_mantissa <<= 1;
                mult_unbiased_exp--;
            }

            // remove non significant digits
            // left*2^7 * right*2^7 = left*right*2^14 =
            // = left*right*2^7 * 2^7 (7 = MANTISSA_BIT_COUNT)
            mult_full_mantissa >>= MANTISSA_BIT_COUNT;
            if (mult_full_mantissa == 0) {
                // Underflow 1.
                return mult_sign == 0u ? POSITIVE_ZERO : NEGATIVE_ZERO;
            }

            // Mantissa too big
            while (mult_full_mantissa > (MANTISSA_MASK + MANTISSA_IMPLICIT_1)) {
                mult_full_mantissa >>= 1;
                mult_unbiased_exp++;
            }

            if ((mult_unbiased_exp + BIAS) > EXPONENT_MAX) {
                // Overflow
                return mult_sign == SIGN_POSITIVE ? POSTIVE_INFINITY : NEGATIVE_INFINITY;
            }

            // Mantissa too small?
            while ((mult_unbiased_exp + BIAS) < EXPONENT_MIN && (mult_full_mantissa != 0)) {
                mult_full_mantissa >>= 1;
                mult_unbiased_exp++;
            }

            if ((mult_unbiased_exp + BIAS) < EXPONENT_MIN || mult_full_mantissa == 0) {
                // Underflow 2
                return mult_sign == SIGN_POSITIVE ? POSITIVE_ZERO : NEGATIVE_ZERO;
            }

            // Adjust exponent for denormalized values
            if ((mult_full_mantissa & MANTISSA_IMPLICIT_1) == 0) {
                mult_unbiased_exp--; // Move one down
            }

            // Remove one (if it exists)
            uint mult_mantissa = mult_full_mantissa & ~MANTISSA_IMPLICIT_1;

            return new fp12(mult_sign, (uint)(mult_unbiased_exp + BIAS), mult_mantissa);
        }

        public static fp12 operator /(fp12 left, fp12 right) {
            // TODO: Handle NaNs and infinities

            uint div_sign = (left.__sign == right.__sign) ? 0u : 1u;

            if (right.is_zero()) {
                // TODO: Handle 0.0 / 0.0
                return div_sign == SIGN_POSITIVE ? POSTIVE_INFINITY : NEGATIVE_INFINITY;
            }

            uint lm = left.__full_mantissa;
            uint rm = right.__full_mantissa;

            int lexp = left.__unbiased_exponent;
            int rexp = right.__unbiased_exponent;

            // To obtain result we will preform integer
            // division on lm an drm:
            // (left*2^7)*2^7 / (right*2^7) = (left/right)*2^7

            // Since full mantissas have only 8-bits we can
            // use uint's to perform the division (or even ushorts + overflow).
            uint div_full_mantissa = (lm << MANTISSA_BIT_COUNT) / rm;
            int div_unbiased_exp = lexp - rexp;

            if (div_full_mantissa == 0) {
                return div_sign == SIGN_POSITIVE ? POSITIVE_ZERO : NEGATIVE_ZERO;
            }

            // Normalize non-zero mantissa, exp will be adjusted later.
            // This operation will not cause prcision lost (we only add digits).
            while (div_full_mantissa < MANTISSA_IMPLICIT_1) {
                div_full_mantissa <<= 1;
                div_unbiased_exp--;
            }

            // Mantissa too big
            while (div_full_mantissa > (MANTISSA_MASK + MANTISSA_IMPLICIT_1)) {
                div_full_mantissa >>= 1;
                div_unbiased_exp++;
            }

            // Exponent too big
            if ((div_unbiased_exp + BIAS) > EXPONENT_MAX) {
                // Overflow
                return div_sign == SIGN_POSITIVE ? POSTIVE_INFINITY : NEGATIVE_INFINITY;
            }

            // Exponent is too small
            while ((div_unbiased_exp + BIAS) < EXPONENT_MIN && (div_full_mantissa != 0)) {
                div_full_mantissa >>= 1;
                div_unbiased_exp++;
            }

            // Cannot normalize too small exponent or mantissa == 0 after/during normalization
            if ((div_unbiased_exp + BIAS) < EXPONENT_MIN || div_full_mantissa == 0) {
                // Underflow
                return div_sign == SIGN_POSITIVE ? POSITIVE_ZERO : NEGATIVE_ZERO;
            }

            // Adjust exponent for denormalized values
            uint div_exp;
            if ((div_full_mantissa & MANTISSA_IMPLICIT_1) == 0) {
                div_exp = (uint) (div_unbiased_exp + BIAS - 1); // Encode denorm value
            }
            else {
                div_exp = (uint) (div_unbiased_exp + BIAS);
            }

            // Remove one (if it exists)
            uint div_mantissa = div_full_mantissa & ~MANTISSA_IMPLICIT_1;

            return new fp12(div_sign, div_exp, div_mantissa);
        }

        private static uint negate_sign(int sign)
            => (sign == 0) ? 1u : 0u;
   }
}