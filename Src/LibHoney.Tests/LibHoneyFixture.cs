using System;
using Xunit;

namespace Honeycomb.Tests
{
    public class LibHoneyFixture : IDisposable
    {
        public LibHoneyFixture ()
        {
            LibHoney = new LibHoney ("key1", "HelloTest", 1);
        }

        public void Dispose ()
        {
            LibHoney.Close ();
        }

        public LibHoney LibHoney {
            get;
            set;
        }
    }
}
