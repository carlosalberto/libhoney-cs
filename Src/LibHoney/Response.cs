using System;
using System.Net;

namespace Honeycomb
{
    /// <summary>
    /// Encapsulates Event response information.
    /// </summary>
    public sealed class Response
    {
        internal Response ()
        {
        }

        /// <summary>
        /// The resulting status code from sending the event, if any.
        /// </summary>
        /// <value>The status code.</value>
        public HttpStatusCode StatusCode {
            get;
            internal set;
        }

        /// <summary>
        /// The duration of the send operation.
        /// </summary>
        /// <value>The duration.</value>
        public TimeSpan Duration {
            get;
            internal set;
        }

        /// <summary>
        /// Field added by the user in the Event object. It is not sent
        /// to Honeycomb with the event.
        /// </summary>
        /// <value>The metadata.</value>
        public object Metadata {
            get;
            internal set;
        }

        /// <summary>
        /// Body of the response after sending the event, if any.
        /// </summary>
        /// <value>The body.</value>
        public string Body {
            get;
            internal set;
        }

        /// <summary>
        /// If the event could not be sent to Honeycomb, contains
        /// the internal error appearing as part the current process.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage {
            get;
            internal set;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Honeycomb.Response"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Honeycomb.Response"/>.</returns>
        public override string ToString ()
        {
            return String.Format ("[Response: StatusCode={0}, Duration={1}, ErrorMessage={2}]",
                                  StatusCode, Duration, ErrorMessage);
        }
    }
}
