using System;
using System.Text;
using System.IO;
using fp12lib;
using System.Collections.Generic;
using System.Linq;

namespace consoleapp
{
    class Program
    {
        public static void Main() { 
            // ReciprocalX();
            // SinePolynomial();
            EnumerateAllValues();
        }

        static void EnumerateAllValues() {
            uint MAX = (1u << 12);

            var values = new List<float>();

            for (uint b = 0; b < MAX; b++) {
                
                fp12 f = new fp12((ushort)b);
                if (f.is_nan || f.is_negative_infinity || f.is_positive_infinity) {
                    continue;
                }

                if (f.__sign == 1) continue;

                values.Add((float)f);
            }

            values.Sort();

            File.WriteAllText(
                "/home/mc/fp12_values.txt",
                string.Join(
                    Environment.NewLine, 
                    values.Select(f => f.ToString("R")).ToArray()));
        }

        static void SinePolynomial() {
            float s1  = -1.66666666666666324348e-01f;
            float s2  =  8.33333333332248946124e-03f;
            float s3  = -1.98412698298579493134e-04f;

            fp12 S1 = (fp12)s1;
            fp12 S2 = (fp12)s2;
            fp12 S3 = (fp12)s3;

            S3 = fp12.POSITIVE_ZERO;

            // x in [-pi/4; pi/4]
            fp12 sine(fp12 x) {
                fp12 x2 = x * x;
                fp12 x3 = x2 * x;

                fp12 sine_aprox = x + x3*(S1 + x2*(S2 + x2*S3));
                return sine_aprox;
            }

            float sine_float(float x) {
                float x2 = x*x;
                float x3 = x2*x;
                
                float sine_aprox = x + x3*(s1 + x2*(s2 + x2*s3));
                return sine_aprox;
            }


            StringBuilder sb = new StringBuilder();
            sb.Append("x float xfp12 fp12").AppendLine();

            int POINTS_COUNT = 200;
            float X_MIN = -0.7854f, X_MAX = 0.7854f * 2;
            for (int i = 0; i < POINTS_COUNT; i++) {
                float x = X_MIN + i * (X_MAX - X_MIN) / POINTS_COUNT;

                // https://en.wikipedia.org/wiki/Chebyshev_polynomials

                float sin = sine_float(x);

                fp12 X = (fp12)x;
                fp12 R = sine(X);

                sb.AppendFormat("{0} {1} {2} {3}", x, sin, (float)X, (float)R).AppendLine();
            }

            File.WriteAllText("/home/mc/sine.txt", sb.ToString());
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
