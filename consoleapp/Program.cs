using System;
using fp12lib;

namespace consoleapp
{
    class Program
    {
        static void Main(string[] args)
        {
            fp12 x = fp12.POSITIVE_ZERO;
            fp12 delta = (fp12)0.1f;

            Console.WriteLine("x = {0}", x);
            Console.WriteLine("delta = {0}", delta);

            for (int i = 0; i < 100; i++) {
                x = x + delta;
                Console.WriteLine("x = {0}", x);
            }

            return;

            while (!x.is_positive_infinity) {
                Console.WriteLine(x);
                x += delta;
            }

            Console.WriteLine("Hello World!");
        }
    }
}
