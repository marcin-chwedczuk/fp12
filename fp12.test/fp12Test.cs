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
      }
}
