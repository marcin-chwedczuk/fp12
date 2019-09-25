using fp12lib;
using Xunit;

namespace fp12test {
    public class float_bytesTest {
        [Fact]
        public void can_convert_float_to_bytes_and_back_again() {
            var fb = new float_bytes(3.1415f);

            var f = fb.to_float();

            Assert.Equal(3.1415f, f);
        }

        [Fact]
        public void can_change_float_components() {
            var fb = new float_bytes(0.0f);

            // 1.5 x 2^2 (dec)
            var f = fb.with_sign(1)
                .with_unbiased_exponent(2)
                .with_mantissa(1 << 22)
                .to_float();


            Assert.Equal(-6.0f, f);
        }
    }
}