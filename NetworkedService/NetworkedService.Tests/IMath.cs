using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Tests
{
    public interface IAddition
    {
        int Add(int a, int b);
    }

    public interface ISubtraction
    {
        int Sub(int a, int b);
    }

    public interface IMath : IAddition, ISubtraction
    {
        int Rand();
        void Noop(int n);
    }
}
