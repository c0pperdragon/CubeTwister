using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace TableGenerator
{
    public class FirstPhase
    {

        static int[] twistSymGroup;        // map from twist index to one of the symmetry groups
        static int[][] symGroupMembers;    // maps from symmetry group to the twist indizes

        static int[][] symEdgeFlipIndex;
        static int[][] symMiddleEdgeDistributionIndex;

        static FirstPhase()
        {
            BuildTwistSymGroup();
            BuildSymmetricalIndizes();
        }

        public static void nixi()
        {
        }

        private static void BuildTwistSymGroup()
        {
            twistSymGroup = new int[2187];
            symGroupMembers = new int[594][];
            for (int i = 0; i < twistSymGroup.Length; i++)
            {
                twistSymGroup[i] = -1;
            }
            int numGroups = 0;

            Cube c = new Cube();

            for (int i=0; i<twistSymGroup.Length; i++)
            {
                if (twistSymGroup[i]<0)
                {
                    c.SetCornerTwistFromIndex(i);
                    int s0 = c.GetCornerTwistIndex();
                    c.MirrorUD();
                    int ud = c.GetCornerTwistIndex();
                    c.MirrorLR();
                    int ud_lr = c.GetCornerTwistIndex();
                    c.MirrorUD();
                    int lr = c.GetCornerTwistIndex();
                    c.MirrorLR();
                    if (s0!=c.GetCornerTwistIndex() || i!=s0)
                    {
                        Console.WriteLine("ERROR: symmetry action do not match original situation!");
                    }

                    int g = numGroups++;

                    if (twistSymGroup[s0] >= 0 || twistSymGroup[ud]>=0 || twistSymGroup[lr]>=0 || twistSymGroup[ud_lr]>= 0)
                    {
                        Console.WriteLine("ERROR: Overwriting symmetry group member");
                    }

                    twistSymGroup[s0] = g;
                    twistSymGroup[ud] = g;
                    twistSymGroup[lr] = g;
                    twistSymGroup[ud_lr] = g;

                    symGroupMembers[g] = new int[] { s0, ud, lr, ud_lr };

//                    Console.Write(g + ": " + s0 + " " + ud+ " " + lr+ " " + ud_lr + "  ");
//                    if ((numGroups % 2) == 0) Console.WriteLine();
                }
            }
        }

        private static void BuildSymmetricalIndizes()
        {
            Cube c = new Cube();

            symEdgeFlipIndex = new int[2048][];
            for (int i = 0; i < symEdgeFlipIndex.Length; i++)
            {
                c.SetEdgeFlipFromIndex(i);
                int s0 = c.GetEdgeFlipIndex();
                c.MirrorUD();
                int ud = c.GetEdgeFlipIndex();
                c.MirrorLR();
                int ud_lr = c.GetEdgeFlipIndex();
                c.MirrorUD();
                int lr = c.GetEdgeFlipIndex();
                c.MirrorLR();
                if (i != s0 || i!=c.GetEdgeFlipIndex())
                {
                    Console.WriteLine("Edge flip mirror calcuation error");
                }
                symEdgeFlipIndex[i] = new int[] { s0, ud, lr, ud_lr };
            }

            symMiddleEdgeDistributionIndex = new int[495][];
            for (int i = 0; i < symMiddleEdgeDistributionIndex.Length; i++)
            {
                c.SetMiddleEdgeDistributionFromIndex(i);
                int s0 = c.GetMiddleEdgeDistributionIndex();
                c.MirrorUD();
                int ud = c.GetMiddleEdgeDistributionIndex();
                c.MirrorLR();
                int ud_lr = c.GetMiddleEdgeDistributionIndex();
                c.MirrorUD();
                int lr = c.GetMiddleEdgeDistributionIndex();
                c.MirrorLR();
                if (i != s0 || i != c.GetMiddleEdgeDistributionIndex())
                {
                    Console.WriteLine("Middle Edge Distribution mirror calcuation error: "+i+"!="+s0);
                }
                symMiddleEdgeDistributionIndex[i] = new int[] { s0, ud, lr, ud_lr };
            }
        }

        public static void CreateTables()
        {
            int totalcombinations = 594 * 2048 * 495;

            // create files to write table to
            byte[] zeroes = new byte[594 * 495];
            FileStream moves = new FileStream("c:/temp/phase1.bin", FileMode.Create, FileAccess.ReadWrite);
            for (int i = 0; i < 2048; i++)
            {
                moves.Write(zeroes, 0, zeroes.Length);
            }

            Cube c = new Cube();

            // initialize to not reached
            byte[] steps = new byte[totalcombinations];
            for (int index=0; index<steps.Length; index++)
            {
                steps[index] = 255;
            }

            // create the root situation for this phase
            int dummym;
            steps[getindex(c, out dummym)] = 0;

            // iteratively add newly reachable situations
            for (int distance=0; distance<250; distance++)
            {
                int currentsituations = 0;
                int foundnew = 0;
                int foundfaster = 0;

                long start = DateTime.Now.Ticks / 10000;  // ms

                for (int index = 0; index < totalcombinations; index++)
                {
                    // consider only situations with current distance to get next situations
                    if (steps[index] == distance)
                    {
                        currentsituations++;    // count situtaions with this distance

                        setcube(c, index);
                        for (int a= 1; a <= Cube.B3D3; a++)
                        {
                            c.DoIncrementalAction(a);

                            int mir;
                            int ni = getindex(c, out mir);

                            int newdistance = distance+Cube.time_actions[a];
                            if (newdistance < steps[ni])       // found a faster route 
                            {
                                if (steps[ni]==255)
                                {   foundnew++;
                                }
                                else
                                {   foundfaster++;
                                }
                                // memorize shorter distance
                                steps[ni] = (byte) newdistance;

                                // write to file
                                moves.Seek(ni, SeekOrigin.Begin);
                                moves.WriteByte((byte) (Cube.sym_actions[mir][a]));
                            }
                        }
                    }
                }

                long end = DateTime.Now.Ticks / 10000;  // ms
                Console.WriteLine(distance+": Situations: "+currentsituations+" New: " + foundnew+" Improved: "+foundfaster+"  ("+(end-start)+" ms)");
            }

            // finish writing
            moves.Close();
        }

        public static int getindex(Cube c, out int symtype)
        {
            int t = c.GetCornerTwistIndex();
            int tg = twistSymGroup[t];
            int e = c.GetEdgeFlipIndex();
            int d = c.GetMiddleEdgeDistributionIndex();

            int es = int.MaxValue;
            int ds = int.MaxValue;
            symtype = -1;

            for (int s=0; s<4; s++)
            { 
                if (symGroupMembers[tg][s]==t)
                {
                    int etmp = symEdgeFlipIndex[e][s];
                    int dtmp = symMiddleEdgeDistributionIndex[d][s];
                    if ( (etmp < es) || (etmp==es && dtmp<ds) )
                    {
                        es = etmp;
                        ds = dtmp;
                        symtype = s;
                    } 
                }
            }

            return tg + 594 * (es + 2048 * ds);
        }
        public static void setcube(Cube c, int index)
        {
            setcube(c, index, Cube.SYMTYPE_NONE);
        }
        public static void setcube(Cube c, int index, int symtype)
        {
            int tg = (index % 594);
            index = index / 594;
            int e = (index % 2048);
            index = index / 2048;
            int d = index;

            c.SetCornerTwistFromIndex(symGroupMembers[tg][symtype]);
            c.SetEdgeFlipFromIndex(symEdgeFlipIndex[e][symtype]);
            c.SetMiddleEdgeDistributionFromIndex(symMiddleEdgeDistributionIndex[d][symtype]);
        }


        public static int Solve(Cube c, int[] histogram)
        {
            FileStream file = new FileStream("c:/temp/phase1.bin", FileMode.Open, FileAccess.Read);

            int m;
            int i = getindex(c, out m);
            Console.Write(i + ":" + m + " ");

            int turns = 0;
            int time = 0;
            for (;;)
            {
                i = getindex(c, out m);
//                Console.Write("("+m+")");

                file.Seek(i, SeekOrigin.Begin);
                int b = file.ReadByte();
                if (b <= 0)
                {
                    if (c.GetCornerTwistIndex() != 0 || c.GetEdgeFlipIndex() != 0 || c.GetMiddleEdgeDistributionIndex()!=0)
                    {
                        Console.WriteLine("Phase 1 leads to unfinished cube");
                        return -1;
                    }
                    break;  // no more moves - finish operation
                }
                int a = Cube.sym_actions[m].IndexOf((char)b);

                turns++;
                time += Cube.time_actions[a];

                histogram[a]++;

                Console.Write(Cube.sym_actions[0][a]);
                c.DoReverseAction(a);

                if (turns==35)
                {
                    Console.WriteLine("Not finding solution...");
                }
            }
//            Console.Write("  ("+turns + "/"+time+")");
            file.Close();

            return time;
        }

        public static void WriteSymmetries(Stream w)
        {
            for (int i=0; i<twistSymGroup.Length; i++)
            {
                w.Write(BitConverter.GetBytes((Single)twistSymGroup[i]), 0, 4);
            }
            for (int i=0; i<symGroupMembers.Length; i++)
            {
                w.Write(BitConverter.GetBytes((Single)symGroupMembers[i][0]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symGroupMembers[i][1]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symGroupMembers[i][2]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symGroupMembers[i][3]), 0, 4);
            }
            for (int i = 0; i < symEdgeFlipIndex.Length; i++)
            {
                w.Write(BitConverter.GetBytes((Single)symEdgeFlipIndex[i][0]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symEdgeFlipIndex[i][1]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symEdgeFlipIndex[i][2]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symEdgeFlipIndex[i][3]), 0, 4);
            }
            for (int i = 0; i < symMiddleEdgeDistributionIndex.Length; i++)
            {
                w.Write(BitConverter.GetBytes((Single)symMiddleEdgeDistributionIndex[i][0]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symMiddleEdgeDistributionIndex[i][1]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symMiddleEdgeDistributionIndex[i][2]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symMiddleEdgeDistributionIndex[i][3]), 0, 4);
            }
        }
  
    }
}
