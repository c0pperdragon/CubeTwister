using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace TableGenerator
{
    public class SecondPhase
    {

        static int[] cornerSymGroup;       // map from corner permutation index to one of the symmetry groups
        static int[][] symGroupMembers;    // maps from symmetry group to the corner permutation indizes

        static int[][] symOuterEdgePermutationIndex;
        static int[][] symMiddleEdgePermutationIndex;

        static SecondPhase()
        {
            BuildCornerSymGroup();
            BuildSymmetricalIndizes();
        }

        public static void nixi()
        {
        }

        private static void BuildCornerSymGroup()
        {
            cornerSymGroup = new int[40320];
            symGroupMembers = new int[10368][];
            for (int i = 0; i < cornerSymGroup.Length; i++)
            {
                cornerSymGroup[i] = -1;
            }
            int numGroups = 0;

            Cube c = new Cube();

            for (int i = 0; i < cornerSymGroup.Length; i++)
            {
                if (cornerSymGroup[i] < 0)
                {
                    c.SetCornerPermutationFromIndex(i);
                    int s0 = c.GetCornerPermutationIndex();
                    c.MirrorUD();
                    int ud = c.GetCornerPermutationIndex();
                    c.MirrorLR();
                    int ud_lr = c.GetCornerPermutationIndex();
                    c.MirrorUD();
                    int lr = c.GetCornerPermutationIndex();
                    c.MirrorLR();
                    if (s0 != c.GetCornerPermutationIndex() || i != s0)
                    {
                        Console.WriteLine("ERROR: symmetry action do not match original situation!");
                    }

                    int g = numGroups++;

                    if (cornerSymGroup[s0] >= 0 || cornerSymGroup[ud] >= 0 ||  cornerSymGroup[lr] >= 0 || cornerSymGroup[ud_lr] >= 0
                    )
                    {
                        Console.WriteLine("ERROR: Overwriting symmetry group member");
                    }

                    cornerSymGroup[s0] = g;
                    cornerSymGroup[ud] = g;
                    cornerSymGroup[lr] = g;
                    cornerSymGroup[ud_lr] = g;

                    symGroupMembers[g] = new int[] { s0, ud, lr, ud_lr };

//                        Console.Write(g + ": " + s0 + " " + ud + " " + lr + " " + ud_lr + "  ");
//                        Console.WriteLine();
                }
            }
        }

        private static void BuildSymmetricalIndizes()
        {
 
            Cube c = new Cube();

            symOuterEdgePermutationIndex = new int[40320][];
            for (int i = 0; i < symOuterEdgePermutationIndex.Length; i++)
            {
                c.SetOuterEdgePermutationFromIndex(i);
                int s0 = c.GetOuterEdgePermutationIndex();
                c.MirrorUD();
                int ud = c.GetOuterEdgePermutationIndex();
                c.MirrorLR();
                int ud_lr = c.GetOuterEdgePermutationIndex();
                c.MirrorUD();
                int lr = c.GetOuterEdgePermutationIndex();
                c.MirrorLR();
                if (i != s0 || i != c.GetOuterEdgePermutationIndex())
                {
                    Console.WriteLine("outer edge permutation mirror calcuation error");
                }
                symOuterEdgePermutationIndex[i] = new int[] { s0, ud, lr, ud_lr  };
            }

            symMiddleEdgePermutationIndex = new int[24][];
            for (int i = 0; i < symMiddleEdgePermutationIndex.Length; i++)
            {
                c.SetMiddleEdgePermutationFromIndex(i);
                int s0 = c.GetMiddleEdgePermutationIndex();
                c.MirrorUD();
                int ud = c.GetMiddleEdgePermutationIndex();
                c.MirrorLR();
                int ud_lr = c.GetMiddleEdgePermutationIndex();
                c.MirrorUD();
                int lr = c.GetMiddleEdgePermutationIndex();
                c.MirrorLR();
                if (i != s0 || i != c.GetMiddleEdgePermutationIndex())
                {
                    Console.WriteLine("Middle Edge Permutation mirror calcuation error: " + i + "!=" + s0);
                }
                symMiddleEdgePermutationIndex[i] = new int[] { s0, ud, lr, ud_lr };
            }
        }



        public static void CreateTables()
        {
            long totalcombinations = 10368L * 40320 * 12;

            // create files to write table parts to
            FileStream moves = new FileStream("c:/temp/phase2.log", FileMode.Create, FileAccess.Write);
            byte[] fbuffer = new byte[6];

            // initialize to not reached
            HugeByteArray steps = new HugeByteArray(totalcombinations);
            for (long index = 0; index < totalcombinations; index++)
            {
                steps[index] = 255;
            }

            // create the root situation for this phase
            Cube c = new Cube();

            int dummym;
            steps[getindex(c, out dummym)] = 0;

            Console.WriteLine("Start computing reachable positions...");

            // iteratively add newly reachable situations
            for (int distance = 0; distance < 250; distance++)
            {
                int currentsituations = 0;
                int foundnew = 0;
                int foundfaster = 0;

                long start = DateTime.Now.Ticks / 10000;  // ms

                for (long index = 0; index < totalcombinations; index++)
                {
                    // consider only situations with current distance to get next situations
                    if (steps[index] == distance)
                    {
                        currentsituations++;    // count situations with this distance

                        setcube(c, index);
                        for (int a = 1; a <= Cube.B3D3; a++)
                        {
                            c.DoIncrementalAction(a);

                            // disallow moves that would bring cube out of S1
                            if (!Cube.s1_actions[a])
                            {
                                continue;
                            }

                            int mir;
                            long ni = getindex(c, out mir);

                            int newdistance = distance + Cube.time_actions[a];
                            if (newdistance < steps[ni])       // found a faster route 
                            {
                                if (steps[ni] == 255)
                                {
                                    foundnew++;
                                }
                                else
                                {
                                    foundfaster++;
                                }
                                // memorize shorter distance
                                steps[ni] = (byte)newdistance;

                                // write to spool file (position: 5 byte, move: 1 byte)
                                fbuffer[0] = (byte)(ni & 0xff);
                                fbuffer[1] = (byte)((ni >> 8) & 0xff);
                                fbuffer[2] = (byte)((ni >> 16) & 0xff);
                                fbuffer[3] = (byte)((ni >> 24) & 0xff);
                                fbuffer[4] = (byte)((ni >> 32) & 0xff);
                                fbuffer[5] = (byte)(Cube.sym_actions[mir][a]);
                                moves.Write(fbuffer, 0, 6);
                            }
                        }
                    }
                }

                long end = DateTime.Now.Ticks / 10000;  // ms
                Console.WriteLine(distance + ": Situations: " + currentsituations + " New: " + foundnew + " Improved: " + foundfaster + "  (" + (end - start) + " ms)");
            }

            // finish writing
            moves.Close();
        }


        public static void JoinTableLogFile()
        {
            long totalcombinations = 10368L * 40320 * 12;

            // create the move table  (non-used elements are 0)
            HugeByteArray b = new HugeByteArray(totalcombinations);

            // read files where the table parts were written to
            Console.WriteLine("Reading phase2 parts file...");
            FileStream moves = new FileStream("c:/temp/phase2.log", FileMode.Open, FileAccess.Read);

            // read all move parts (newer overwriting older ones)
            byte[] fbuffer = new byte[6];
            for (; ; )
            {
                int len = moves.Read(fbuffer, 0, 6);
                if (len == 0)
                {
                    break;
                }
                if (len != 6)
                {
                    throw new IOException("Could not read exactly 6 bytes!");
                }
                long index = ((long)fbuffer[0])
                      + (((long)fbuffer[1]) << 8)
                      + (((long)fbuffer[2]) << 16)
                      + (((long)fbuffer[3]) << 24)
                      + (((long)fbuffer[4]) << 32);
                b[index] = fbuffer[5];
            }
            moves.Close();

            // write in new order to algorithm files
            Console.WriteLine("Start writing phase2 files...");
            FileStream o = new FileStream("c:/temp/phase2_0.bin", FileMode.Create, FileAccess.Write);
            for (long index = 0; index < totalcombinations / 2; index++)
            {
                o.WriteByte(b[index]);
            }
            o.Close();
            o = new FileStream("c:/temp/phase2_1.bin", FileMode.Create, FileAccess.Write);
            for (long index = totalcombinations / 2; index < totalcombinations; index++)
            {
                o.WriteByte(b[index]);
            }
            o.Close();
        }



        public static long getindex(Cube c, out int symtype)
        {
            int t = c.GetCornerPermutationIndex();
            int tg = cornerSymGroup[t];
            int o = c.GetOuterEdgePermutationIndex();
            int m = c.GetMiddleEdgePermutationIndex();
//       Console.WriteLine("get: corner:" + t+ " outer:"+o+" middle:"+m);

            int os = int.MaxValue;
            int ms = int.MaxValue;
            symtype = -1;

            for (int s = 0; s < 4; s++)
            {
                if (symGroupMembers[tg][s] == t)
                {
                    int otmp = symOuterEdgePermutationIndex[o][s];
                    int mtmp = symMiddleEdgePermutationIndex[m][s];
                    if ((otmp < os) || (otmp == os && mtmp < ms))
                    {
                        os = otmp;
                        ms = mtmp;
                        symtype = s;
                    }
                }
            }

            return tg + 10368L * (os + 40320 * (ms / 2));
        }

        public static void setcube(Cube c, long index)
        {
            setcube(c, index, Cube.SYMTYPE_NONE);
        }
        public static void setcube(Cube c, long index, int symtype)
        {
            int tg = (int)(index % 10368);
            int t = symGroupMembers[tg][symtype];
            index = index / 10368;
            int os = (int)(index % 40320);
            index = index / 40320;
            int ms = (int)(index * 2) + (t + os) % 2;      // match the permutation parity 

            int m = symMiddleEdgePermutationIndex[ms][symtype];
            int o = symOuterEdgePermutationIndex[os][symtype];

            c.SetCornerPermutationFromIndex(t);
            c.SetOuterEdgePermutationFromIndex(o);
            c.SetMiddleEdgePermutationFromIndex(m);
        }


        public static int Solve(Cube c, int[] histogram)
        {
            long totalcombinations = 10368L * 40320 * 12;

            FileStream file0 = new FileStream("c:/temp/phase2_0.bin", FileMode.Open, FileAccess.Read);
            FileStream file1 = new FileStream("c:/temp/phase2_1.bin", FileMode.Open, FileAccess.Read);

            int m;
            long i = getindex(c, out m);
//            Console.Write(i + ":" + m + " ");

            int turns = 0;
            int time = 0;
            for (; ; )
            {
                i = getindex(c, out m);
//                Console.Write("("+m+")");

                int b;
                if (i < totalcombinations / 2)
                {
                    file0.Seek(i, SeekOrigin.Begin);
                    b = file0.ReadByte();
                }
                else
                {
                    file1.Seek(i - (totalcombinations/2), SeekOrigin.Begin);
                    b = file1.ReadByte();
                }
                
                if (b <= 0)
                {
                    if (c.GetCornerPermutationIndex() != 0 || c.GetOuterEdgePermutationIndex() != 0 || c.GetMiddleEdgePermutationIndex()!=0)
                    {
                        Console.WriteLine("Phase 2 leads to unfinished cube");
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

//            Console.Write("  (" + turns + "/" + time + ")");

            file0.Close();
            file1.Close();

            return time;
        }

        public static void WriteSymmetries(Stream w)
        {
            for (int i = 0; i < cornerSymGroup.Length; i++)
            {
                w.Write(BitConverter.GetBytes((Single)cornerSymGroup[i]), 0, 4);
            }
            for (int i = 0; i < symGroupMembers.Length; i++)
            {
               w.Write(BitConverter.GetBytes((Single)symGroupMembers[i][0]), 0, 4);
               w.Write(BitConverter.GetBytes((Single)symGroupMembers[i][1]), 0, 4);
               w.Write(BitConverter.GetBytes((Single)symGroupMembers[i][2]), 0, 4);
               w.Write(BitConverter.GetBytes((Single)symGroupMembers[i][3]), 0, 4);
            }
            for (int i = 0; i < symOuterEdgePermutationIndex.Length; i++)
            {
                w.Write(BitConverter.GetBytes((Single)symOuterEdgePermutationIndex[i][0]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symOuterEdgePermutationIndex[i][1]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symOuterEdgePermutationIndex[i][2]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symOuterEdgePermutationIndex[i][3]), 0, 4);
            }
            for (int i = 0; i < symMiddleEdgePermutationIndex.Length; i++)
            {
                w.Write(BitConverter.GetBytes((Single)symMiddleEdgePermutationIndex[i][0]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symMiddleEdgePermutationIndex[i][1]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symMiddleEdgePermutationIndex[i][2]), 0, 4);
                w.Write(BitConverter.GetBytes((Single)symMiddleEdgePermutationIndex[i][3]), 0, 4);
            }
        }
    }
}
