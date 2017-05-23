using System;
using Xunit;

namespace LibHoney.Tests
{
    public class LibHoneyFixture
    {
        public LibHoneyFixture ()
        {
            LibHoney = new Honey ("key1", "HelloTest");
        }

        public void Dispose ()
        {
            LibHoney.Dispose ();
        }

        public Honey LibHoney {
            get;
            set;
        }
    }
}
