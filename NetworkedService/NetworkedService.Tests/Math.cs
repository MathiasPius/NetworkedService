using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Tests
{
    public class Math : IMath
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public int Sub(int a, int b)
        {
            return a - b;
        }

        public int Rand()
        {
            return 4;
        }

        public void Noop(int n)
        {
            Console.WriteLine("Server: Noop: " + n);
        }
    }
}
