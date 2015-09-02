using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TableGenerator
{

    public class HugeByteArray
    {
        const long partsize = 1 << 28;
        byte[][] data;

        public HugeByteArray(long size)
        {
            data = new byte[(int)((size + (partsize - 1)) / partsize)][];   // round up to full parts
            for (int i = 0; i < data.Length - 1; i++)
            {
                data[i] = new byte[partsize];
                size -= partsize;
            }
            data[data.Length - 1] = new byte[((int)size)];
        }

        // Indexer declaration. 
        public byte this[long index]
        {
            get
            {
                return data[(int)(index>>28)][(int)(index & (partsize-1))];
            }

            set
            {
                data[(int)(index>>28)][(int)(index & (partsize-1))] = value;
            }
        }
       
    }
}

