using System;

namespace Honeycomb
{
    public class SendException : Exception
    {
        public SendException (string message)
            : base (message)
        {
        }
    }
}