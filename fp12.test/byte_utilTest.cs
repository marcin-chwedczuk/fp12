using fp12lib;
using Xunit;

namespace fp12test {
    public class byte_utilTest {
        [Fact]
        public void properly_mirrors_byte() {
            Assert.Equal(0b1111_1111, byte_util.mirror(0b1111_1111));
            Assert.Equal(0, byte_util.mirror(0));

            Assert.Equal(0b1110_0010, byte_util.mirror(0b0100_0111));
        }
    }
}