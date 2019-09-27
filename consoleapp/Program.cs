using System;
using System.Text;
using System.IO;
using fp12lib;

namespace consoleapp
{
    class Program
    {
        public static void Main() { 
            ReciprocalX();
        }

        static void ReciprocalX()
        {
            // Draw using gnuplot> plot 'recX.txt' using 1:2, "" u 3:4
            // set key autotitle columnheader

            StringBuilder sb = new StringBuilder();
            sb.Append("x float xfp12 fp12").AppendLine();

            int POINTS_COUNT = 200;
            float X_MIN = 0.01f, X_MAX = 0.3f;
            for (int i = 0; i < POINTS_COUNT; i++) {
                float x = X_MIN + i * (X_MAX - X_MIN) / POINTS_COUNT;

                // https://en.wikipedia.org/wiki/Chebyshev_polynomials

                float r = 1.0f / x;

                fp12 X = (fp12)x;
                fp12 R = ((fp12)1.0f) / X;

                sb.AppendFormat("{0} {1} {2} {3}", x, r, (float)X, (float)R).AppendLine();
            }

            File.WriteAllText("/home/mc/recX.txt", sb.ToString());
        }

        static void Chebyshev5()
        {
            // Draw using gnuplot> plot 'chart.txt' using 1:2, "" u 3:4

            StringBuilder sb = new StringBuilder();
            sb.Append("x float xfp12 fp12").AppendLine();

            int POINTS_COUNT = 100;
            for (int i = 0; i < POINTS_COUNT; i++) {
                float x = -1.0f + i * 2.0f / POINTS_COUNT;

                // https://en.wikipedia.org/wiki/Chebyshev_polynomials

                float c5 = 16*pow(x, 5) - 20*pow(x, 3) + 5*x;

                fp12 X = (fp12)x;
                fp12 C5 = ((fp12)16.0f)*X*X*X*X*X - ((fp12)20.0f)*X*X*X + ((fp12)5.0f)*X;

                sb.AppendFormat("{0} {1} {2} {3}", x, c5, (float)X, (float)C5).AppendLine();
            }

            File.WriteAllText("/home/mc/chart.txt", sb.ToString());
        }

        private static float pow(float f, int n) => (float)Math.Pow(f, n);
    }
}
