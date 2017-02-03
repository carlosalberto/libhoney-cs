using System;

namespace LibHoney
{
    public class SendException : Exception
    {
        public SendException (string message)
            : base (message)
        {
        }
    }
}