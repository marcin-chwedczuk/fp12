using System;
using System.Text;
using System.IO;
using fp12lib;

namespace consoleapp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Draw using gnuplot> plot 'chart.txt' using 1:2, "" u 3:4

            StringBuilder sb = new StringBuilder();
            sb.Append("x float xfp12 fp12").AppendLine();

            int POINTS_COUNT = 100;
            for (int i = 0; i < POINTS_COUNT; i++) {
                float x = -1.0f + i * 2.0f / POINTS_COUNT;

                float c3f = 4*x*x*x - 3*x;

                fp12 xfp12 = (fp12)x;
                fp12 c3fp12 = ((fp12)4.0f) *xfp12*xfp12*xfp12 - ((fp12)3.0f)*xfp12;

                sb.AppendFormat("{0} {1} {2} {3}", x, c3f, (float)xfp12, (float)c3fp12).AppendLine();
            }

            File.WriteAllText("/home/mc/chart.txt", sb.ToString());
        }
    }
}
