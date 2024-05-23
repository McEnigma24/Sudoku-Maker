using System;
using System.Collections.Generic;
using System.Text;

namespace Sudoku_Maker
{
    class Coordinet
    {
        int first;
        int second;

        public Coordinet(int a, int b)
        {
            first = a;
            second = b;
        }

        public int First()
        {
            return first;
        }
        public int Second()
        {
            return second;
        }
    }
}
