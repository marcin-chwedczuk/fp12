using System;
using Xunit;
using Xunit.Abstractions;
using fp12lib;

namespace fp12test
{
    public class fp12Test
    {
        [Fact]
        public void nan_is_recognized_as_nan()
        {
            fp12 x = fp12.NaN;

            Assert.True(x.is_nan);
            Assert.False(x.is_positive_infinity);
            Assert.False(x.is_negative_infinity);
        }


        [Fact]
        public void positive_infinity_is_recognized_as_positive_infinity()
        {
            fp12 x = fp12.POSTIVE_INFINITY;

            Assert.False(x.is_nan);
            Assert.True(x.is_positive_infinity);
            Assert.False(x.is_negative_infinity);
        }

        [Fact]
        public void negative_infinity_is_recognized_as_negative_infinity()
        {
            fp12 x = fp12.NEGATIVE_INFINITY;

            Assert.False(x.is_nan);
            Assert.False(x.is_positive_infinity);
            Assert.True(x.is_negative_infinity);
        }


        [Fact]
        public void abs_works() {
            fp12 m3 = (fp12)(-3.0f);

            fp12 m3abs = m3.abs();
            Assert.Equal((fp12)3.0f, m3abs);

            m3abs = m3abs.abs();
            Assert.Equal((fp12)3.0f, m3abs);
        }

        public class conversions_between_float_and_fp12 {

            [Fact]
            public void can_convert_finite_value() {
                fp12 x = (fp12)3.1415f;
                float fx = (float)x;

                // TODO: Check this is legit
                Assert.Equal(3.140625f, fx);
            }
    
            [Fact]
            public void can_convert_min_positive_value() {
                fp12 x = fp12.MIN_POSITIVE_VALUE;
                
                float fx = (float)x;

                Assert.Equal(0.0001220703125f, fx);
            }
 
            [Fact]
            public void can_convert_max_positive_value() {
                fp12 x = fp12.MAX_POSITIVE_VALUE;
                
                float fx = (float)x;

                Assert.Equal(255f, fx);
            }

            [Fact]
            public void zero_converts_to_zero() {
                fp12 x = fp12.POSITIVE_ZERO;

                float fx = (float)x;

                Assert.Equal(0.0f, fx);
            }
        }

        public class equality_operators {
            [Fact]
            public void equals_works() {
                fp12 pi = (fp12)3.14159;
                fp12 pi2 = (fp12)3.14159;
                fp12 e = (fp12)2.7183;

                Assert.True(pi == pi2);
                Assert.True(e == e);

                Assert.False(e == pi);
                Assert.False(pi == e);

                Assert.False(pi == fp12.NaN);
                Assert.False(fp12.NaN == fp12.NaN);

                Assert.True(fp12.POSTIVE_INFINITY == fp12.POSTIVE_INFINITY);
                Assert.False(fp12.POSTIVE_INFINITY == fp12.NEGATIVE_INFINITY);

                Assert.True(fp12.POSITIVE_ZERO == fp12.POSITIVE_ZERO);
            }
        }

        public class arithmentic_operations {
            [Fact]
            public void operator_plus_works() {
                fp12 n1 = (fp12)1.0f;
                fp12 n2 = (fp12)2.0f;
                fp12 n3 = (fp12)3.0f;
                fp12 n8 = (fp12)8.0f;
                fp12 n9 = (fp12)9.0f;

                Assert.Equal(n3, n1 + n2);
                Assert.Equal(n3, n1 + n1 + n1);
                Assert.Equal(n9, n8 + n1);
                Assert.Equal(n9, n3 + n2 + n1 + n2 + n1);
            }

            [Fact]
            public void bug_small_value_converted_to_infinity() {
                fp12 b = (fp12)0.00390625559f;

                Assert.False(b.is_positive_infinity);
            }

            [Fact]
            public void bug_wrong_normalization_after_addition() {
                fp12 a = (fp12)0.01f;
                fp12 b = (fp12)0.00390625f;

                fp12 result = a + b;

                fp12 expected = (fp12)0.01390625f;
                Assert.Equal(expected, result);
            }

            [Fact]
            public void negation_works() {
                fp12 a = (fp12)0.01f;
                fp12 negA = (fp12)(-0.01f);

                Assert.Equal(negA, -a);
            }


            [Fact]
            public void can_subtract_one_from_one() {
                var zero = fp12.POSITIVE_ZERO;
                var one = (fp12)1.0f;

                var result = one - one;

                Assert.Equal(zero, result);
            }

            [Fact]
            public void subtraction_works() {
                fp12 n1 = (fp12)1.0f;
                fp12 n2 = (fp12)2.0f;
                fp12 n3 = (fp12)3.0f;
                fp12 n8 = (fp12)8.0f;
                fp12 n9 = (fp12)9.0f;

                Assert.Equal(n1, n2 - n1);
                Assert.Equal(n3, n8 - n3 - n2);
                Assert.Equal(n8, n9 - n1);
                Assert.Equal(fp12.POSITIVE_ZERO, n9 - n3 - n2 - n1 - n2 - n1);
            }
        }
     }
}
