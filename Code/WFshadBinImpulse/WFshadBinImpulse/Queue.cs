using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WFshadBinImpulse
{
  class Queue
  { public
     int input, output, Len, full;
    public
      int[] Array;
    ~Queue() { }
    public Queue(int len)  // Constructor
    {
      this.Len = len;
      this.input = 0;
      this.output = 0;
      this.full = 0;
      this.Array = new int[Len];
    }

    public int Put(int V)
    {
      if (input == Len - 1)
      {
          full = 1;
          return -1;
      }
      Array[input] = V;
      input++;
      return 1;
    }

    public int Get()
    {
        if (Empty() == 1)
        {
            return -1;
        }
        int i = Array[output];
        if (output == Len - 1) output = 0;
        else output++;
        if (full == 1) full = 0;
        return i;
    }

    public int Empty()
    {
        if (input == output && full != 1)
        {
            return 1;
        }
        return 0;
    }
  } //************************ end class Queue *******************************
}

