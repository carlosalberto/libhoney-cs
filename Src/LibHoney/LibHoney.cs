using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Honeycomb
{
    /// <summary>
    /// Send events to Honeycomb from within a .Net application.
    /// </summary>
    public sealed class LibHoney : IDisposable
    {
        /// <summary>
        /// Honeycomb API version.
        /// </summary>
        public const string Version = "1.0.0";

        /// <summary>
        /// Default api host for Honeycomb.
        /// </summary>
        public const string DefaultApiHost = "https://api.honeycomb.io";

        /// <summary>
        /// Default sample rate for events. Value is 1.
        /// </summary>
        public const int DefaultSampleRate = 1;

        /// <summary>
        /// Default number of threads used for sending events. Value is 10.
        /// </summary>
        public const int DefaultMaxConcurrentBatches = 10;

        /// <summary>
        /// Default value for blocking on send and responses. Value is false.
        /// </summary>
        const bool DefaultBlock = false;

        readonly FieldHolder fields = new FieldHolder ();

        Transmission transmission;
        ResponseCollection responses;

        /// <summary>
        /// Initializes LibHoney and prepare it to send events to HoneyComb.
        /// </summary>
        public LibHoney (string writeKey, string dataSet)
            : this (writeKey, dataSet, DefaultApiHost, DefaultSampleRate, DefaultMaxConcurrentBatches,
                    DefaultBlock, DefaultBlock)
        {
        }

        /// <summary>
        /// Initializes LibHoney and prepare it to send events to HoneyComb.
        /// </summary>
        public LibHoney (string writeKey, string dataSet, int maxConcurrentBatches)
            : this (writeKey, dataSet, DefaultApiHost, DefaultSampleRate, maxConcurrentBatches,
                    DefaultBlock, DefaultBlock)
        {
        }

        /// <summary>
        /// Initialized LibHoney and prepare it to send events to HoneyComb.
        /// </summary>
        public LibHoney (string writeKey, string dataSet, string apiHost)
            : this (writeKey, dataSet, apiHost, DefaultSampleRate, DefaultMaxConcurrentBatches,
                    DefaultBlock, DefaultBlock)
        {
        }

        /// <summary>
        /// Initializes LibHoney and prepare it to send events to HoneyComb.
        /// </summary>
        public LibHoney (string writeKey, string dataSet, string apiHost, int sampleRate, int maxConcurrentBatches)
            : this (writeKey, dataSet, apiHost, sampleRate, maxConcurrentBatches,
                    DefaultBlock, DefaultBlock)
        {
        }

        /// <summary>
        /// Initializes LibHoney and prepare it to send events to HoneyComb.
        /// </summary>
        public LibHoney (string writeKey, string dataSet, string apiHost, int sampleRate, int maxConcurrentBatches,
                      bool blockOnSend, bool blockOnResponse)
        {
            if (writeKey == null)
                throw new ArgumentNullException (nameof (writeKey));
            if (dataSet == null)
                throw new ArgumentNullException (nameof (dataSet));
            if (apiHost == null)
                throw new ArgumentNullException (nameof (apiHost));

            if (sampleRate < 1)
                throw new ArgumentOutOfRangeException (nameof (sampleRate));
            if (maxConcurrentBatches < 1)
                throw new ArgumentOutOfRangeException (nameof (maxConcurrentBatches));

            Uri apiHostUri;
            if (!Uri.TryCreate (apiHost, UriKind.Absolute, out apiHostUri) || !IsSchemeSupported (apiHostUri.Scheme))
                throw new ArgumentException (nameof (apiHost));

            transmission = new Transmission (maxConcurrentBatches, blockOnSend, blockOnResponse);
            responses = new ResponseCollection (transmission.Responses);

            WriteKey = writeKey;
            DataSet = dataSet;
            ApiHost = apiHost;
            SampleRate = sampleRate;
            BlockOnSend = blockOnSend;
            BlockOnResponse = blockOnResponse;
        }

        internal void Reset ()
        {
            fields.Clear ();
        }

        /// <summary>
        /// Hostname for the Honeycomb API server to which to send events.
        /// </summary>
        /// <value>The API hostname.</value>
        public string ApiHost {
            get;
            private set;
        }

        /// <summary>
        /// Determines if libhoney should block or drop packets that exceed the size
        /// of the events queue. Default to false - events overflowing the send queue
        /// will be dropped.
        /// </summary>
        /// <value><c>true</c> if block on send; otherwise, <c>false</c>.</value>
        public bool BlockOnSend {
            get;
            private set;
        }

        /// <summary>
        /// Determines if libhoney should block trying to hand responses back to the caller.
        /// If this is true and there is nothing reading from Responses, it will fill up
        /// and prevent events from being sent to Honeycomb. Defaults to false - if you don't
        /// read from Responses it will be ok.
        /// </summary>
        /// <value><c>true</c> if block on response; otherwise, <c>false</c>.</value>
        public bool BlockOnResponse {
            get;
            private set;
        }

        /// <summary>
        /// Name of the Honeycomb dataset to which to send these events.
        /// </summary>
        /// <value>The dataset name.</value>
        public string DataSet {
            get;
            private set;
        }

        internal FieldHolder Fields {
            get { return fields; }
        }

        internal bool IsClosed {
            get { return transmission == null; }
        }

        /// <summary>
        /// Responses queue. Retrieving responses from it will be
        /// blocking or not depending on the BlockOnResponses value.
        /// </summary>
        /// <value>The responses queue.</value>
        public ResponseCollection Responses {
            get {
                return responses;
            }
        }

        /// <summary>
        /// Rate at which to sample this event. Default is 1, meaning no sampling.
        /// If you want to send one event out of every 250 times Send() is called,
        /// you would specify 250 here.
        /// </summary>
        /// <value>The sample rate.</value>
        public int SampleRate {
            get;
            private set;
        }

        internal Transmission Transmission {
            get { return transmission; }
        }

        /// <summary>
        /// Honeycomb authentication token.
        /// </summary>
        /// <value>The authentication token.</value>
        public string WriteKey {
            get;
            private set;
        }

        /// <summary>
        /// Adds data to the global scope of this LibHoney object. These metrics will
        /// be inherited by all builders and events.
        /// </summary>
        /// <param name="data">Data.</param>
        public void Add (IEnumerable<KeyValuePair<string, object>> data)
        {
            if (data == null)
                throw new ArgumentNullException (nameof (data));

            fields.Add (data);
        }

        /// <summary>
        /// Adds data to the global scope of this LibHoney object. This metric will
        /// be inherited by all builders and events.
        /// </summary>
        /// <param name="name">Name of the metric.</param>
        /// <param name="value">Value of the metric.</param>
        public void AddField (string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException (nameof (name));

            fields.AddField (name, value);
        }

        /// <summary>
        /// Takes a field name and a func that will generate values
        /// for that metric. func is called once every time a NewEvent() is
        /// created and added as a field (with name as the key) to the newly created
        /// event.
        /// </summary>
        /// <param name="name">Name of the metric.</param>
        /// <param name="func">Func object to call to get the value of the metric.</param>
        public void AddDynamicField (string name, Func<object> func)
        {
            if (name == null)
                throw new ArgumentNullException (nameof (name));
            if (func == null)
                throw new ArgumentNullException (nameof (func));

            fields.AddDynamicField (name, func);
        }

        /// <summary>
        /// Close waits for all in-flight messages to be sent. You should
        /// call Close() before app termination. Note: this method is not thread-safe.
        /// </summary>
        public void Close ()
        {
            if (transmission == null)
                return;

            transmission.Dispose ();
            transmission = null;
        }

        void IDisposable.Dispose ()
        {
            Close ();
        }

        static bool IsSchemeSupported (string scheme)
        {
            return scheme == "http" || scheme == "https";
        }

        /// <summary>
        /// Shortcut to create an event, add data, and send the event.
        /// </summary>
        /// <param name="data">Data.</param>
        public void SendNow (IEnumerable<KeyValuePair<string, object>> data)
        {
            if (data == null)
                throw new ArgumentNullException (nameof (data));

            if (IsClosed)
                throw new SendException ("Tried to send on a closed libhoney");

            var ev = new Event (this);
            ev.Add (data);
            ev.Send ();
        }

        /// <summary>
        /// Shortcut to create an event, add a single metric, and send the event.
        /// </summary>
        /// <param name="name">Name of the single metric to send.</param>
        /// <param name="value">Value of the single metric to send.</param>
        public void SendNow (string name, object value)
        {
            if (IsClosed)
                throw new SendException ("Tried to send on a closed libhoney");

            var ev = new Event (this);
            ev.AddField (name, value);
            ev.Send ();
        }
    }
}
