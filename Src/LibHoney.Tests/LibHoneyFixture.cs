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
            LibHoney.Dispose ();
        }

        public LibHoney LibHoney {
            get;
            set;
        }
    }
}
