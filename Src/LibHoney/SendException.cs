using System;

namespace Honeycomb
{
    /// <summary>
    /// The exception that is thrown when an Event cannot be sent.
    /// </summary>
    public class SendException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the SendException class with a string containing
        /// a message.
        /// </summary>
        /// <param name="message">The error that ocurred at send time.</param>
        public SendException (string message)
            : base (message)
        {
        }
    }
}