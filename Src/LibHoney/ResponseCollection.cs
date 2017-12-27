using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Honeycomb
{
    /// <summary>
    /// Responses collection linked to a LibHoney instance.
    /// Thread-safe operations are provided for enumerating,
    /// counting and popping out Response objects.
    /// 
    /// The interface is similar to System.Collections.Concurrent.BlockingCollection,
    /// without adding capabilities.
    /// </summary>
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

        /// <summary>
        /// Gets the number of items contained in the ResponseCollection.
        /// </summary>
        /// <value>The number of items contained in the ResponseCollection.</value>
        public int Count {
            get { return responses.Count; }
        }

        /// <summary>
        /// Gets whether this ResponseCollection has been marked as complete for adding.
        /// </summary>
        /// <value>Whether this collection has been marked as complete for adding.</value>
        public bool IsAddingCompleted {
            get { return responses.IsAddingCompleted; }
        }

        /// <summary>
        /// Gets whether this ResponseCollection has been marked as complete for adding and is empty.
        /// </summary>
        /// <value>Whether this collection has been marked as complete for adding and is empty.</value>
        public bool IsCompleted {
            get { return responses.IsCompleted; }
        }

        /// <summary>
        /// Removes an item from the ResponseCollection.
        /// </summary>
        public Response Take ()
        {
            return responses.Take ();
        }

        /// <summary>
        /// Tries to remove an item from the ResponseCollection.
        /// </summary>
        /// <returns>true if an item could be removed; otherwise, false.</returns>
        /// <param name="response">Response.</param>
        public bool TryTake (out Response response)
        {
            return responses.TryTake (out response);
        }

        /// <summary>
        /// Tries to remove an item from the ResponseCollection in the specified time period.
        /// </summary>
        /// <returns>
        /// true if an item could be removed from the collection within the specified time; otherwise, false.
        /// </returns>
        /// <param name="response">Response.</param>
        /// <param name="timeout">
        /// An object that represents the number of milliseconds to wait, or an object
        /// that represents -1 milliseconds to wait indefinitely.
        /// </param>
        public bool TryTake (out Response response, TimeSpan timeout)
        {
            return responses.TryTake (out response, timeout);
        }

        /// <summary>
        /// Tries to remove an item from the ResponseCollection in the specified time period.
        /// </summary>
        /// <returns>
        /// true if an item could be removed from the collection within the specified time; otherwise, false.
        /// </returns>
        /// <param name="response">Response.</param>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or -1 to wait indefinitely.
        /// </param>
        public bool TryTake (out Response response, int millisecondsTimeout)
        {
            return responses.TryTake (out response, millisecondsTimeout);
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
