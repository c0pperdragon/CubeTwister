using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TableGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Stream w = new FileStream("c:/temp/symmetries.bin", FileMode.Create, FileAccess.Write);
            FirstPhase.WriteSymmetries(w);
            SecondPhase.WriteSymmetries(w);
            w.Close();
/*
 *          Cube c = new Cube();
            Random r = new Random();

            c.print();
            c.DoAction(Cube.A0C1);
            c.print();
            c.DoAction(Cube.B0D2);
            c.print();
            c.DoAction(Cube.E2);
            c.print();
            c.DoAction(Cube.A1C3);
            c.print();
            c.DoAction(Cube.E1);
            c.print();
            c.DoAction(Cube.B1D0);
            c.print();
            c.DoAction(Cube.A1C3);
            c.print();
            c.DoAction(Cube.B1D3);
            c.print();
*/
            /*
                         int[] histogram = new int[34];
                         int tries = 10;
                         double total = 0;
                         for (int i = 0; i < tries; i++)
                         {
                             c.Randomize(r);
                             int t1 = FirstPhase.Solve(c,histogram);
                             int t2 = SecondPhase.Solve(c, histogram);
                             if (t1+t2 < 0) break;
                             Console.WriteLine();
                             total += (t1 + t2);
                         }
                         Console.WriteLine("Average: " + (total / tries));

                         for (int i = 0; i < histogram.Length; i++)
                         {
                             Console.WriteLine("Movement: " + i + " Usage: " + histogram[i]);
                         }
             */
            Console.WriteLine("Press enter...");
            Console.ReadLine();
        }

    }
}
