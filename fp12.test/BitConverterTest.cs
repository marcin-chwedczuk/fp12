using System;
using System.Linq;
using System.Text;
using Xunit;

namespace fp12test {
    public class BitConverterTest {
        [Fact]
        public void defnesive_copy_using_ToArray() {
            var bytes = new byte[] { 1, 2, 3 };
            var copy = bytes.ToArray();

            copy[1] = 100;

            Assert.Equal(2, bytes[1]);
        }

        [Fact]
        public void layout_of_returned_float_bytes() {
            float f = -255.0f;
            string fBits = ToBitsString(f);

            // e=7 but bias=127 for float, so E:
            // 10000110 = 127 + 7
            Assert.Equal("11000011 01111111 00000000 00000000", fBits);
        }

        private string ToBitsString(float f) {
            StringBuilder sb = new StringBuilder();

            var bytes = BitConverter.GetBytes(f);

            // Change Little endian to Big endian encoding so
            // that sign bit is first. After reversal:
            //
            // MSB ------------------ LSB
            // S | E (8 bit) | M (23 bit)
            Array.Reverse(bytes);

            foreach (byte b in bytes) {
                var tmp = Convert.ToString(b, 2).PadLeft(8, '0');
                sb.Append(tmp).Append(" ");
            }

            if (sb.Length > 0) {
                // Remove last space
                sb.Remove(sb.Length-1, 1);
            }

            return sb.ToString(); 
        }
    }
}