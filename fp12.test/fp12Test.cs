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

                Assert.Equal(6.103515625E-05, fx);
            }
 
            [Fact]
            public void can_convert_max_positive_value() {
                fp12 x = fp12.MAX_POSITIVE_VALUE;
                
                float fx = (float)x;

                Assert.Equal(255f, fx);
            }

            [Fact]
            public void zero_converts_to_zero() {
                fp12 x = fp12.ZERO;

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

                Assert.True(fp12.ZERO == fp12.ZERO);
            }
        }
     }
}
