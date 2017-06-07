using System;
using System.Net;

namespace Honeycomb
{
    public sealed class Response
    {
        internal Response ()
        {
        }

        public HttpStatusCode StatusCode {
            get;
            internal set;
        }

        public TimeSpan Duration {
            get;
            internal set;
        }

        public object Metadata {
            get;
            internal set;
        }

        public string Body {
            get;
            internal set;
        }

        public string ErrorMessage {
            get;
            internal set;
        }

        public override string ToString ()
        {
            return String.Format ("[Response: StatusCode={0}, Duration={1}, ErrorMessage={2}]",
                                  StatusCode, Duration, ErrorMessage);
        }
    }
}
