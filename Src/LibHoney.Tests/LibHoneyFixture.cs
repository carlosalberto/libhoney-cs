﻿using System;
using Xunit;

namespace Honeycomb.Tests
{
    public class LibHoneyFixture
    {
        public LibHoneyFixture ()
        {
            LibHoney = new LibHoney ("key1", "HelloTest");
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
