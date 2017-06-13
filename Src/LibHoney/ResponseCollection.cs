using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Honeycomb
{
    public sealed class ResponseCollection : IEnumerable<Response>
    {
        BlockingCollection<Response> responses;

        internal ResponseCollection (BlockingCollection<Response> responses)
        {
            this.responses = responses;
        }

        internal BlockingCollection<Response> Responses {
            get { return responses; }
        }

        public int Count {
            get { return responses.Count; }
        }

        public bool IsAddingCompleted {
            get { return responses.IsAddingCompleted; }
        }

        public bool IsCompleted {
            get { return responses.IsCompleted; }
        }

        public Response Take ()
        {
            return responses.Take ();
        }

        public bool TryTake (out Response response)
        {
            return responses.TryTake (out response);
        }

        public bool TryTake (out Response response, TimeSpan timeout)
        {
            return responses.TryTake (out response, timeout);
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return ((IEnumerable)responses).GetEnumerator ();
        }

        IEnumerator<Response> IEnumerable<Response>.GetEnumerator ()
        {
            return ((IEnumerable<Response>)responses).GetEnumerator ();
        }
    }
}
