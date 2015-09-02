using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableGenerator
{
    
    public class Cube
    {
        public const int SYMTYPE_NONE     = 0;
        public const int SYMTYPE_UD       = 1;       // mirror along U-D - axis
        public const int SYMTYPE_LR       = 2;       // mirror along L-R - axis
        public const int SYMTYPE_UD_LR    = 3;       // mirror along U-D and L-R axis

        const int ULF = 0;
        const int URF = 1;
        const int URB = 2;
        const int ULB = 3;
        const int DLF = 4;
        const int DRF = 5;
        const int DRB = 6;
        const int DLB = 7;
        const int UL = 0;
        const int UF = 1;
        const int UR = 2;
        const int UB = 3;
        const int DL = 4;
        const int DF = 5;
        const int DR = 6;
        const int DB = 7;
        const int LF = 8;
        const int RF = 9;
        const int RB = 10;
        const int LB = 11;

                                            // "0"  (no action)
        public const int E1 = 1;            // "1"
        public const int E2 = 2;            // "2"
        public const int E3 = 3;            // "3"
        public const int A0C1 = 4;          // "A"
        public const int A0C2 = 5;          // "B"
        public const int A0C3 = 6;          // "C"
        public const int A1C0 = 7;          // "D"
        public const int A1C1 = 8;          // "E"
        public const int A1C2 = 9;          // "F"
        public const int A1C3 = 10;         // "G"
        public const int A2C0 = 11;         // "H"
        public const int A2C1 = 12;         // "I"
        public const int A2C2 = 13;         // "J"
        public const int A2C3 = 14;         // "K"
        public const int A3C0 = 15;         // "L"
        public const int A3C1 = 16;         // "M"
        public const int A3C2 = 17;         // "N"
        public const int A3C3 = 18;         // "O"
        public const int B0D1 = 19;         // "a"
        public const int B0D2 = 20;         // "b"
        public const int B0D3 = 21;         // "c"
        public const int B1D0 = 22;         // "d"
        public const int B1D1 = 23;         // "e"
        public const int B1D2 = 24;         // "f"
        public const int B1D3 = 25;         // "g"
        public const int B2D0 = 26;         // "h"
        public const int B2D1 = 27;         // "i"
        public const int B2D2 = 28;         // "j"
        public const int B2D3 = 29;         // "k"
        public const int B3D0 = 30;         // "l"
        public const int B3D1 = 31;         // "m"
        public const int B3D2 = 32;         // "n"
        public const int B3D3 = 33;         // "o"

        public static String[] sym_actions =
        {       "0123ABCDEFGHIJKLMNOabcdefghijklmno",    // SYMTYPE_NONE
                "0321DHLAEIMBFJNCGKOcbalonmhkjidgfe",    // SYMTYPE_UD
                "0321CBALONMHKJIDGFEdhlaeimbfjncgko",    // SYMTYPE_LR
                "0123LHDCOKGBNJFAMIElhdcokgbnjfamie" ,   // SYMTYPE_UD_LR
        };

        public static byte[] time_actions = { 0, 6,10,6, 8,15,8, 8,9,15,9, 15,15,16,15, 8,9,15,9, 6,10,6, 6,6,10,6, 10,10,10,10, 6,6,10,6 };

        // which actions are allowed in S1
        public static bool[] s1_actions = { true, false,true,false, 
                                              true,true,true,   true,true,true,true, true,true,true,true, true,true,true,true,
                                              false,true,false, false,false,false,false, true,false,true,false, false,false,false,false };

        // transform cubies for a U-D mirror transformation of the cube: U<->D   
                                        // before: ULF  URF  URB  ULB  DLF  DRF  DRB  DLB 
        static int[] cornertransformations_ud  = { DLF, DRF, DRB, DLB, ULF, URF, URB, ULB };
                                       //  before: UL  UF  UR  UB  DL  DF  DR  DB  LF  RF  RB  LB 
        static int[] edgetransformations_ud =    { DL, DF, DR, DB, UL, UF, UR, UB, LF, RF, RB, LB   };
//        // transform cubies for a F-B mirror transformation of the cube: F<->B   
//                                       // before: ULF  URF  URB  ULB  DLF  DRF  DRB  DLB 
//        static int[] cornertransformations_fb = { ULB, URB, URF, ULF, DLB, DRB, DRF, DLF };
//                                      //  before: UL  UF  UR  UB  DL  DF  DR  DB  LF  RF  RB  LB 
//        static int[] edgetransformations_fb =   { UL, UB, UR, UF, DL, DB, DR, DF, LB, RB, RF, LF };
        // transform cubies for a L-R mirror transformation of the cube: L<->R
                                       // before: ULF  URF  URB  ULB  DLF  DRF  DRB  DLB 
        static int[] cornertransformations_lr = { URF, ULF, ULB, URB, DRF, DLF, DLB, DRB };
                                    //  before: UL  UF  UR  UB  DL  DF  DR  DB  LF  RF  RB  LB 
        static int[] edgetransformations_lr = { UR, UF, UL, UB, DR, DF, DL, DB, RF, LF, LB, RB };

        static int[] mirror_flip = { 0, 2, 1 };

        static int[] factorial;
        static int[][] n_over_k;

        static int[] tmp = new int[12];

        static Cube()
        {
            factorial = new int[] { 1, 1, 2, 6, 24, 120, 720, 5040, 40320, 362880, 3628800, 39916800, 479001600};

            n_over_k = new int[13][];
            for (int n=0; n<=12; n++)
            {
                n_over_k[n] = new int[5];
                for (int k=0; k<=4; k++)
                {
                    int p = 1;
                    for (int i = n; i >= (n - (k - 1)); i--)
                    {
                        p = p * i;
                    }
                    for (int i = 1; i <= k; i++)
                    {
                        p = p / i;
                    }
                    n_over_k[n][k] = p;
                }
            }
        }


        // -- object attributes --
        int[] cornercubie;    
        int[] cornertwist;
        int[] edgecubie;
        int[] edgeflip;
//        int rotationparity;

        public Cube()
        {
            cornercubie = new int[8];
            for (int i = 0; i < 8; i++) cornercubie[i] = i;
            cornertwist = new int[8];       // all twists 0
            edgecubie = new int[12];
            for (int i = 0; i < 12; i++) edgecubie[i] = i;
            edgeflip = new int[12];         // all flips 0
        }

        public Cube(Cube c)
        {
            cornercubie = new int[8];
            for (int i = 0; i < 8; i++) cornercubie[i] = c.cornercubie[i];
            cornertwist = new int[8];
            for (int i = 0; i < 8; i++) cornertwist[i] = c.cornertwist[i];
            edgecubie = new int[12];
            for (int i = 0; i < 12; i++) edgecubie[i] = c.edgecubie[i];
            edgeflip = new int[12];
            for (int i = 0; i < 12; i++) edgeflip[i] = c.edgeflip[i];
        }


        public Boolean IsValid()
        {
            int sumtwist = 0;
            int sumflip = 0;
            for (int i = 0; i < 8; i++)
            {
                sumtwist += cornertwist[i];
            }
            for (int i = 0; i < 12; i++)
            {
                sumflip += edgeflip[i];
            }
            int cp = permutationindex(cornercubie, 8);
            int ep = permutationindex(edgecubie, 12);
            return (sumtwist % 3 == 0) && (sumflip % 2 == 0) && cp >= 0 && ep >= 0 && (((cp + ep) & 1) == 0);
        }

        public void Randomize(Random r)
        {
            // create totally random cube
            int sumtwist = 0;
            int sumflip = 0;
            for (int i = 0; i < 8; i++)
            {
                int x = r.Next(3); 
                cornercubie[i] = i;
                cornertwist[i] = x;
                sumtwist += x;                
            }
            for (int i = 0; i < 12; i++)
            {
                int x = r.Next(2);
                edgecubie[i] = i;
                edgeflip[i] = x;
                sumflip += x;
            }
            mix(cornercubie, 0,8, r);
            mix(edgecubie,0,12,r);
            // fix up all parities
            cornertwist[0] = (cornertwist[0] + 99 - sumtwist) % 3;
            edgeflip[0] = (edgeflip[0] + 100 - sumflip) % 2;
            FixPermutationParity();
        }

        public void RandomizeS1(Random r)
        {
            // create random cube that is already in S1
            for (int i = 0; i < 8; i++)
            {
                cornercubie[i] = i;
                cornertwist[i] = 0;
            }
            for (int i = 0; i < 12; i++)
            {
                edgecubie[i] = i;
                edgeflip[i] = 0;
            }
            mix(cornercubie, 0,8, r);
            mix(edgecubie, 0,8, r);
            mix(edgecubie, 8, 4, r);
            FixPermutationParity();
        }

        public void FixPermutationParity()
        {
            int cp = permutationindex(cornercubie, 8);
            int ep = permutationindex(edgecubie, 12);
            if (((cp+ep)&1) != 0)
            {
                int dummy = cornercubie[0];
                cornercubie[0] = cornercubie[1];
                cornercubie[1] = dummy;
            }
        }


        public void TwistARight()
        {
            rot4(cornercubie, ULF, URF, URB, ULB);
            rot4(cornertwist, ULF, URF, URB, ULB);
            rot4(edgecubie, UF, UR, UB, UL);
            rot4(edgeflip, UF, UR, UB, UL);
        }
        public void TwistCRight()
        {
            rot4(cornercubie, DLF, DRF, DRB, DLB);
            rot4(cornertwist, DLF, DRF, DRB, DLB);
            rot4(edgecubie, DF, DR, DB, DL);
            rot4(edgeflip, DF, DR, DB, DL);
        }
        public void TwistBUp()
        {
            rot4(cornercubie, DRF, URF, URB, DRB);
            rot4(cornertwist, DRF, URF, URB, DRB);
            cornertwist[DRF] = (cornertwist[DRF] + 1) % 3;
            cornertwist[URF] = (cornertwist[URF] + 2) % 3;
            cornertwist[URB] = (cornertwist[URB] + 1) % 3;
            cornertwist[DRB] = (cornertwist[DRB] + 2) % 3;

            rot4(edgecubie, RF, UR, RB, DR);
            rot4(edgeflip, RF, UR, RB, DR);

            edgeflip[RF] = 1 - edgeflip[RF];
            edgeflip[UR] = 1 - edgeflip[UR];
            edgeflip[RB] = 1 - edgeflip[RB];
            edgeflip[DR] = 1 - edgeflip[DR];
        }
        public void TwistDUp()
        {
            rot4(cornercubie, DLF, ULF, ULB, DLB);
            rot4(cornertwist, DLF, ULF, ULB, DLB);
            cornertwist[DLF] = (cornertwist[DLF] + 2) % 3;
            cornertwist[ULF] = (cornertwist[ULF] + 1) % 3;
            cornertwist[ULB] = (cornertwist[ULB] + 2) % 3;
            cornertwist[DLB] = (cornertwist[DLB] + 1) % 3;

            rot4(edgecubie, LF, UL, LB, DL);
            rot4(edgeflip, LF, UL, LB, DL);

            edgeflip[LF] = 1 - edgeflip[LF];
            edgeflip[UL] = 1 - edgeflip[UL];
            edgeflip[LB] = 1 - edgeflip[LB];
            edgeflip[DL] = 1 - edgeflip[DL];
        }

        public void TwistEClockwise()
        {
            rot4(cornercubie, DLB, ULB, URB, DRB);
            rot4(cornertwist, DLB, ULB, URB, DRB);
            cornertwist[DLB] = (cornertwist[DLB] + 2) % 3;
            cornertwist[ULB] = (cornertwist[ULB] + 1) % 3;
            cornertwist[URB] = (cornertwist[URB] + 2) % 3;
            cornertwist[DRB] = (cornertwist[DRB] + 1) % 3;

            rot4(edgecubie, LB, UB, RB, DB);
            rot4(edgeflip, LB, UB, RB, DB);
        }

        public void MirrorUD() 
        {
            exchg_transform(cornercubie, ULF, DLF, cornertransformations_ud);
            exchg_transform(cornercubie, URF, DRF, cornertransformations_ud);
            exchg_transform(cornercubie, URB, DRB, cornertransformations_ud);
            exchg_transform(cornercubie, ULB, DLB, cornertransformations_ud);
            exchg_transform(cornertwist, ULF, DLF, mirror_flip);
            exchg_transform(cornertwist, URF, DRF, mirror_flip);
            exchg_transform(cornertwist, URB, DRB, mirror_flip);
            exchg_transform(cornertwist, ULB, DLB, mirror_flip);

            exchg_transform(edgecubie, UL, DL, edgetransformations_ud);
            exchg_transform(edgecubie, UF, DF, edgetransformations_ud);
            exchg_transform(edgecubie, UR, DR, edgetransformations_ud);
            exchg_transform(edgecubie, UB, DB, edgetransformations_ud);
            exchg(edgeflip, UF, DF);
            exchg(edgeflip, UR, DR);
            exchg(edgeflip, UB, DB);
            exchg(edgeflip, UL, DL);

            edgecubie[LF] = edgetransformations_ud[edgecubie[LF]];
            edgecubie[LB] = edgetransformations_ud[edgecubie[LB]];
            edgecubie[RF] = edgetransformations_ud[edgecubie[RF]];
            edgecubie[RB] = edgetransformations_ud[edgecubie[RB]];
        }
/*
        public void MirrorFB()
        {
            exchg_transform(cornercubie, ULF, ULB, cornertransformations_fb);
            exchg_transform(cornercubie, URF, URB, cornertransformations_fb);
            exchg_transform(cornercubie, DLF, DLB, cornertransformations_fb);
            exchg_transform(cornercubie, DRF, DRB, cornertransformations_fb);
            exchg_transform(cornertwist, ULF, ULB, mirror_flip);
            exchg_transform(cornertwist, URF, URB, mirror_flip);
            exchg_transform(cornertwist, DLF, DLB, mirror_flip);
            exchg_transform(cornertwist, DRF, DRB, mirror_flip);

            exchg_transform(edgecubie, UF, UB, edgetransformations_fb);
            exchg_transform(edgecubie, DF, DB, edgetransformations_fb);
            exchg_transform(edgecubie, LF, LB, edgetransformations_fb);
            exchg_transform(edgecubie, RF, RB, edgetransformations_fb);
            exchg(edgeflip, UF, UB);
            exchg(edgeflip, DF, DB);
            exchg(edgeflip, LF, LB);
            exchg(edgeflip, RF, RB);

            edgecubie[UL] = edgetransformations_fb[edgecubie[UL]];
            edgecubie[UR] = edgetransformations_fb[edgecubie[UR]];
            edgecubie[DL] = edgetransformations_fb[edgecubie[DL]];
            edgecubie[DR] = edgetransformations_fb[edgecubie[DR]];
        }
*/
        public void MirrorLR()
        {
            exchg_transform(cornercubie, ULF, URF, cornertransformations_lr);
            exchg_transform(cornercubie, ULB, URB, cornertransformations_lr);
            exchg_transform(cornercubie, DLF, DRF, cornertransformations_lr);
            exchg_transform(cornercubie, DLB, DRB, cornertransformations_lr);
            exchg_transform(cornertwist, ULF, URF, mirror_flip);
            exchg_transform(cornertwist, ULB, URB, mirror_flip);
            exchg_transform(cornertwist, DLF, DRF, mirror_flip);
            exchg_transform(cornertwist, DLB, DRB, mirror_flip);

            exchg_transform(edgecubie, UL, UR, edgetransformations_lr);
            exchg_transform(edgecubie, DL, DR, edgetransformations_lr);
            exchg_transform(edgecubie, LF, RF, edgetransformations_lr);
            exchg_transform(edgecubie, LB, RB, edgetransformations_lr);
            exchg(edgeflip, UL, UR);
            exchg(edgeflip, DL, DR);
            exchg(edgeflip, LF, RF);
            exchg(edgeflip, LB, RB);

            edgecubie[UF] = edgetransformations_lr[edgecubie[UF]];
            edgecubie[UB] = edgetransformations_lr[edgecubie[UB]];
            edgecubie[DF] = edgetransformations_lr[edgecubie[DF]];
            edgecubie[DB] = edgetransformations_lr[edgecubie[DB]];
        }



        public void DoAction(int a)
        { 
            switch (a)
            {
                case E1: TwistEClockwise();   break;
                case E2: TwistEClockwise(); TwistEClockwise(); break;
                case E3: TwistEClockwise(); TwistEClockwise(); TwistEClockwise(); break;          
                case A0C1: TwistCRight(); break;
                case A0C2: TwistCRight(); TwistCRight(); break;
                case A0C3: TwistCRight(); TwistCRight(); TwistCRight(); break;
                case A1C0: TwistARight(); break;
                case A1C1: TwistARight(); TwistCRight(); break;
                case A1C2: TwistARight(); TwistCRight(); TwistCRight(); break;
                case A1C3: TwistARight(); TwistCRight(); TwistCRight(); TwistCRight(); break;
                case A2C0: TwistARight(); TwistARight(); break;
                case A2C1: TwistARight(); TwistARight(); TwistCRight(); break;
                case A2C2: TwistARight(); TwistARight(); TwistCRight(); TwistCRight(); break;
                case A2C3: TwistARight(); TwistARight(); TwistCRight(); TwistCRight(); TwistCRight(); break;
                case A3C0: TwistARight(); TwistARight(); TwistARight(); break;
                case A3C1: TwistARight(); TwistARight(); TwistARight(); TwistCRight(); break;
                case A3C2: TwistARight(); TwistARight(); TwistARight(); TwistCRight(); TwistCRight(); break;
                case A3C3: TwistARight(); TwistARight(); TwistARight(); TwistCRight(); TwistCRight(); TwistCRight(); break;
                case B0D1: TwistDUp(); break;
                case B0D2: TwistDUp(); TwistDUp();  break;
                case B0D3: TwistDUp(); TwistDUp(); TwistDUp(); break;
                case B1D0: TwistBUp(); break;
                case B1D1: TwistBUp(); TwistDUp(); break;
                case B1D2: TwistBUp(); TwistDUp(); TwistDUp(); break;
                case B1D3: TwistBUp(); TwistDUp(); TwistDUp(); TwistDUp(); break;
                case B2D0: TwistBUp(); TwistBUp(); break;
                case B2D1: TwistBUp(); TwistBUp(); TwistDUp();  break;
                case B2D2: TwistBUp(); TwistBUp(); TwistDUp(); TwistDUp(); break;
                case B2D3: TwistBUp(); TwistBUp(); TwistDUp(); TwistDUp(); TwistDUp(); break;
                case B3D0: TwistBUp(); TwistBUp(); TwistBUp(); break;
                case B3D1: TwistBUp(); TwistBUp(); TwistBUp(); TwistDUp(); break;
                case B3D2: TwistBUp(); TwistBUp(); TwistBUp(); TwistDUp(); TwistDUp(); break;
                case B3D3: TwistBUp(); TwistBUp(); TwistBUp(); TwistDUp(); TwistDUp(); TwistDUp(); break;
            }
        }
        public void DoIncrementalAction(int a)
        {
            switch (a)
            {
                case E1: 
                case E2:
                case E3:   TwistEClockwise(); 
                           break;
                case A0C1: TwistEClockwise(); 
                           TwistCRight(); 
                           break;
                case A0C2: 
                case A0C3: TwistCRight();
                           break;
                case A1C0: TwistCRight();
                           TwistARight(); 
                           break;
                case A1C1: 
                case A1C2: 
                case A1C3: TwistCRight();
                           break;
                case A2C0: TwistCRight();
                           TwistARight();
                           break;
                case A2C1: 
                case A2C2:
                case A2C3: TwistCRight();
                           break;
                case A3C0: TwistCRight();
                           TwistARight();
                           break;
                case A3C1: 
                case A3C2:
                case A3C3: TwistCRight();
                           break;

                case B0D1: TwistCRight();
                           TwistARight();
                           TwistDUp();
                           break;
                case B0D2: 
                case B0D3: TwistDUp();
                           break;
                case B1D0: TwistDUp();
                           TwistBUp();
                           break;
                case B1D1:
                case B1D2:
                case B1D3: TwistDUp();
                           break;
                case B2D0: TwistDUp();
                           TwistBUp();
                           break;
                case B2D1: 
                case B2D2:
                case B2D3: TwistDUp();
                           break;
                case B3D0: TwistDUp();
                           TwistBUp();
                           break;
                case B3D1: 
                case B3D2: 
                case B3D3: TwistDUp();
                           break;
            }
        }

        public void DoReverseAction(int a)
        {
            switch (a)
            {
                case E1: TwistEClockwise(); TwistEClockwise(); TwistEClockwise(); break;
                case E2: TwistEClockwise(); TwistEClockwise(); break;
                case E3: TwistEClockwise(); break;

                case A0C1: TwistCRight(); TwistCRight(); TwistCRight(); break;
                case A0C2: TwistCRight(); TwistCRight(); break;
                case A0C3: TwistCRight(); break;
                case A1C0: TwistARight(); TwistARight(); TwistARight(); break;
                case A1C1: TwistARight(); TwistARight(); TwistARight();  TwistCRight(); TwistCRight(); TwistCRight(); break;
                case A1C2: TwistARight(); TwistARight(); TwistARight();  TwistCRight(); TwistCRight(); break;
                case A1C3: TwistARight(); TwistARight(); TwistARight();  TwistCRight(); break;
                case A2C0: TwistARight(); TwistARight(); break;
                case A2C1: TwistARight(); TwistARight(); TwistCRight(); TwistCRight(); TwistCRight(); break;
                case A2C2: TwistARight(); TwistARight(); TwistCRight(); TwistCRight(); break;
                case A2C3: TwistARight(); TwistARight(); TwistCRight(); break;
                case A3C0: TwistARight(); break;
                case A3C1: TwistARight(); TwistCRight(); TwistCRight(); TwistCRight(); break;
                case A3C2: TwistARight(); TwistCRight(); TwistCRight(); break;
                case A3C3: TwistARight(); TwistCRight();  break;
                case B0D1: TwistDUp(); TwistDUp(); TwistDUp(); break;
                case B0D2: TwistDUp(); TwistDUp(); break;
                case B0D3: TwistDUp(); break;
                case B1D0: TwistBUp(); TwistBUp(); TwistBUp(); break;
                case B1D1: TwistBUp(); TwistBUp(); TwistBUp(); TwistDUp(); TwistDUp(); TwistDUp(); break;
                case B1D2: TwistBUp(); TwistBUp(); TwistBUp(); TwistDUp(); TwistDUp(); break;
                case B1D3: TwistBUp(); TwistBUp(); TwistBUp(); TwistDUp(); break;
                case B2D0: TwistBUp(); TwistBUp(); break;
                case B2D1: TwistBUp(); TwistBUp(); TwistDUp(); TwistDUp(); TwistDUp(); break;
                case B2D2: TwistBUp(); TwistBUp(); TwistDUp(); TwistDUp(); break;
                case B2D3: TwistBUp(); TwistBUp(); TwistDUp(); break;
                case B3D0: TwistBUp(); break;
                case B3D1: TwistBUp(); TwistDUp(); TwistDUp(); TwistDUp(); break;
                case B3D2: TwistBUp(); TwistDUp(); TwistDUp(); break;
                case B3D3: TwistBUp(); TwistDUp(); break;
            }
        }
       
        public int GetCornerTwistIndex()
        {
            int total = 0;
            for (int i=6; i>=0; i--)      // do not calculate corner 7 - is redundant for correct cube
            {
                total = total * 3 + cornertwist[i];
            }
            return total;
        }

        public int GetEdgeFlipIndex()
        {
            int total = 0;
            for (int i=10; i>=0; i--)      // do not calculate edge 11 - is redundant for correct cube
            {
                total = total * 2 + edgeflip[i];
            }
            return total;
        }

        public int GetMiddleEdgeDistributionIndex()
        {
            for (int i = 0; i < 12; i++)
            {
                tmp[i] = edgecubie[11-i] >= 8 ? 1 : 0;
            }
            return selectionindex(tmp, 12, 4);
        }

        public int GetCornerPermutationIndex()
        {
            return permutationindex(cornercubie, 8);
        }

        public int GetMiddleEdgePermutationIndex()
        {
            for (int i = 0; i < 4; i++ )
            {
                tmp[i] = edgecubie[8 + i] - 8;
            }
            return permutationindex(tmp, 4);
        }

        public int GetOuterEdgePermutationIndex()
        {
            return permutationindex(edgecubie, 8);
        }

        public void SetCornerTwistFromIndex(int index)
        {
            int total = 0;
            for (int i = 0; i < 7; i++)      
            {
                int t = index % 3;
                index = index / 3;
                total += t;
                cornertwist[i] = t;
            }
            cornertwist[7] = (99-total) % 3;
//            if (!IsValid())
//            {
//                Console.WriteLine("Invalid after SetCornerTwistFromIndex() ");
//            }
        }

        public void SetEdgeFlipFromIndex(int index)
        {
            int total = 0;
            for (int i = 0; i < 11; i++)
            {
                int f = index % 2;
                index = index / 2;
                total += f;
                edgeflip[i] = f;
            }
            edgeflip[11] = (100 - total) % 2;
//            if (!IsValid())
//            {
//                 Console.WriteLine("Invalid after SetEdgeFlipFromIndex() ");
//            }
        }

        public void SetMiddleEdgeDistributionFromIndex(int index)
        {
            createselectionfromindex(tmp, 12, 4, index);
            int numo = 0;
            int numi = 8;
            for (int i = 0; i < 12; i++)
            {
                if (tmp[11 - i] == 0)
                {
                    edgecubie[i] = numo++;
                }
                else
                {
                    edgecubie[i] = numi++;
                }
            }
        }

        public void SetCornerPermutationFromIndex(int index)
        {
            createpermutationfromindex(cornercubie, 8, index, 0);
        }

        public void SetMiddleEdgePermutationFromIndex(int index)
        {
            createpermutationfromindex(tmp, 4, index, 0);
            for (int i = 0; i < 4; i++)
            {
                edgecubie[8 + i] = 8 + tmp[i];
            }
        }

        public void SetOuterEdgePermutationFromIndex(int index)
        {
            createpermutationfromindex(edgecubie, 8, index, 0);
        }


        public void print()
        {
            for (int i = 0; i < 8; i++)
            {
                Console.Write(cornertwist[i]);
            }
            Console.Write("(" + GetCornerTwistIndex() + ") ");
            for (int i = 0; i < 8; i++)
            {
                Console.Write(cornercubie[i]);
            }
            Console.WriteLine("("+GetCornerPermutationIndex()+")");

            for (int i = 0; i < 12; i++)
            {
                Console.Write(edgeflip[i]);
            }
            Console.Write("(" + GetEdgeFlipIndex() + ") ");

            for (int i = 0; i < 12; i++)
            {
                Console.Write((char)('A' + (edgecubie[i])));                
            }
            int mei = GetMiddleEdgeDistributionIndex();
            if (mei != 0)
            {
                Console.WriteLine("(" + mei + ")");
            }
            else
            {
                Console.WriteLine("(" + GetOuterEdgePermutationIndex() + ":" + GetMiddleEdgePermutationIndex()+ ")");
            }
//            Console.WriteLine("Rotation:" + rotationparity+" Valid:"+IsValid());
        }

        private static void rot4(int[] array, int a, int b, int c, int d)
        {
            int dummy = array[d];
            array[d] = array[c];
            array[c] = array[b];
            array[b] = array[a];
            array[a] = dummy;
        }
        private static void rot4_transform(int[] array, int a, int b, int c, int d, int[] transform)
        {
            int dummy = array[d];
            array[d] = transform[array[c]];
            array[c] = transform[array[b]];
            array[b] = transform[array[a]];
            array[a] = transform[dummy];
        }
        private static void rot4x2(int[] array, int a, int b, int c, int d)
        {
            int dummy = array[a];
            array[a] = array[c];
            array[c] = dummy;
            dummy = array[b];
            array[b] = array[d];
            array[d] = dummy; 
        }
        private static void exchg(int[] array, int a, int b)
        {
            int dummy = array[a];
            array[a] = array[b];
            array[b] = dummy;
        }
        private static void exchg_transform(int[] array, int a, int b, int[] transform)
        {
            int dummy = array[a];
            array[a] = transform[array[b]];
            array[b] = transform[dummy];
        }
        private static void mix(int[] array, int first, int n, Random r)
        {
            for (int i=0; i<n-1; i++)
            {
                int j = i+r.Next(n - i);
                int dummy = array[first+i];
                array[first+i] = array[first+j];
                array[first+j] = dummy;
            }
        }

        public static int permutationindex(int[] array, int n)
        {
            Array.Copy(array, 0, tmp, 0, n);
            return permutationindex(tmp, n, 0);
        }

        // delivers an index of the permutation. the method works in a way to 
        // generate even numbers for even permutations and odd numbers for odd permutations.
        // attention: this method will destroy the array content
        private static int permutationindex(int[] array, int len, int parity)
        {
            // short decision for only two elements (less elements do not work)
            if (len<=2)
            {
                if (array[0]==0 && array[1]==1)
                {
                    return parity;
                }
                else if (array[0]==1 && array[1]==0)
                {
                    return 1 - parity;
                }
                else
                {
                    return -1;
                }
            }

            int lm1 = len - 1;

            // when the last element is already in place, the permutation index is just the index of the sub-list
            if (array[lm1] == lm1)
            {
                return permutationindex(array, lm1, parity);
            }
            // otherwise must find the element and swap it with the currently last one, then calculate permuation index of sub-list
            else
            {
                for (int i = 0; i < lm1; i++)
                {
                    if (array[i] == lm1)
                    {
                        array[i] = array[lm1];  // bring the element from last postion to the vacant place
                        return (lm1 - i) * factorial[lm1] // the combinations that are reserved for the other possibilities
                             + permutationindex(array, lm1, 1 - parity);  // determine index of sub-list (by swapping the parity here, 
                                                                          // odd permutations will get odd index values)
                    }
                }
                // could not find the element - can not be valid permutation
                return -1;
            }
        }

        private static void createpermutationfromindex(int[] array, int len, int index, int fixparity)
        {
            if (len<=2)
            {
                if (index==fixparity)
                {
                    array[0] = 0;
                    array[1] = 1;
                }
                else
                {
                    array[0] = 1;
                    array[1] = 0;
                }
                return;
            }
            int subtotal = factorial[len - 1];
            if (index < 0 || index >= factorial[len])
            {
                return;
            }

            if (index<subtotal)
            {
                array[len - 1] = len - 1;
                createpermutationfromindex(array, len - 1, index, fixparity);
                return;
            }
            int placehighest = len-1- (index / subtotal);

            createpermutationfromindex(array, len - 1, index % subtotal, 1-fixparity);
            array[len - 1] = array[placehighest];
            array[placehighest] = len - 1;
        }


        private static int selectionindex(int[] array, int len, int numselect)
        {
            if (len<=1 || numselect<1)     // no more choices
            {
                return 0;
            }
            if (array[len-1]==0)
            {
            // when this element is not at end of sequence, determine index based on smaller array
                return selectionindex(array, len - 1, numselect);
            }
            else
            {
            // when the element is at end of sequence, must check smaller array, and looking for fewer elements
                return n_over_k[len - 1][numselect]       // must increase index by all possibilities where element was not at end of sequence
                       + selectionindex(array, len - 1, numselect - 1);
                       
            }
        }

        private static void createselectionfromindex(int[] array, int len, int numselect, int index)
        {
            // termination: no selections
            if (numselect<=0)
            {
                for (int i=0; i<len; i++)
                { 
                    array[i] = 0;
                }
                return;
            }
            // everything selected
            if (numselect>=len)
            {
                for (int i = 0; i < len; i++)
                {
                    array[i] = 1;
                }
                return;
            }
            // check in which part of the index space, a 0 of the last element must be
            int part0 = n_over_k[len - 1][numselect];
            if (index<part0)
            {
                array[len - 1] = 0;
                createselectionfromindex(array, len - 1, numselect, index);
            }
            else
            {
                array[len - 1] = 1;
                createselectionfromindex(array, len - 1, numselect-1, index-part0);
            }
        }

        public static void testpermutationindex()
        {
            int[] p = new int[5];
            int[] q = new int[5];

            for (int a=0; a<5; a++)
            {
                p[0] = a;
                for (int b=0; b<5; b++)
                {
                    if (b == a) continue;
                    p[1] = b;
                    for (int c=0; c<5; c++)
                    {
                        if (c == a || c == b) continue;
                        p[2] = c;
                        for (int d=0; d<5; d++)
                        {
                            if (d == a || d == b || d == c) continue;
                            p[3] = d;
                            for (int e=0; e<5; e++)
                            {
                                if (e == a || e == b || e == c || e==d) continue;
                                p[4] = e;

                                for (int i=0; i<5; i++)
                                {
                                    Console.Write(p[i] + " ");
                                }

                                int idx = permutationindex(p, p.Length);
                                Console.Write(" "+idx+"  ");

                                createpermutationfromindex(q, q.Length, idx, 0);

                                for (int i = 0; i < 5; i++)
                                {
                                    Console.Write(q[i] + " ");
                                }

                                Console.WriteLine();
                            }
                        }
                    }
                }
            }
        }
    }
}
