using System;
using System.Linq;

namespace fp12lib {
    public static class byte_util {
        /* Returns mirror flipped byte.
         * For example for 11100010 byte it will return
         * 01000111 byte.
         */
        public static byte mirror(byte b) {
            // Can be precomputed and put into
            // an: mirror[byte] = result
            // if performance is important.

            uint r = 0;

            for (int i = 0; i < 8; i++) {
                if ((b & (1 << i)) != 0) {
                    r |= (1u << (8 - i - 1));
                }
            }

            return (byte)r;
        }

        public static byte[] switch_endiannes(byte[] bytes) {
            // make defensive copy
            bytes = bytes.ToArray();

            Array.Reverse(bytes);

            for (int i = 0; i < bytes.Length; i++) {
                bytes[i] = mirror(bytes[i]);
            }

            return bytes;
        }
    }   
}