using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Honeycomb
{
    public sealed class LibHoney : IDisposable
    {
        public const string Version = "1.0.0.0";

        public const string DefaultApiHost = "https://api.honeycomb.io";
        public const int DefaultSampleRate = 1;
        public const int DefaultMaxConcurrentBatches = 10;
        const bool DefaultBlock = false;

        readonly FieldHolder fields = new FieldHolder ();

        Transmission transmission;
        ResponseCollection responses;

        public LibHoney (string writeKey, string dataSet)
            : this (writeKey, dataSet, DefaultApiHost, DefaultSampleRate, DefaultMaxConcurrentBatches,
                    DefaultBlock, DefaultBlock)
        {
        }

        public LibHoney (string writeKey, string dataSet, int maxConcurrentBatches)
            : this (writeKey, dataSet, DefaultApiHost, DefaultSampleRate, maxConcurrentBatches,
                    DefaultBlock, DefaultBlock)
        {
        }

        public LibHoney (string writeKey, string dataSet, string apiHost)
            : this (writeKey, dataSet, apiHost, DefaultSampleRate, DefaultMaxConcurrentBatches,
                    DefaultBlock, DefaultBlock)
        {
        }

        public LibHoney (string writeKey, string dataSet, string apiHost, int sampleRate, int maxConcurrentBatches)
            : this (writeKey, dataSet, apiHost, sampleRate, maxConcurrentBatches,
                    DefaultBlock, DefaultBlock)
        {
        }

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

        public string ApiHost {
            get;
            private set;
        }

        public bool BlockOnSend {
            get;
            private set;
        }

        public bool BlockOnResponse {
            get;
            private set;
        }

        public string DataSet {
            get;
            private set;
        }

        internal FieldHolder Fields {
            get { return fields; }
        }

        internal bool IsDisposed {
            get { return transmission == null; }
        }

        public ResponseCollection Responses {
            get {
                return responses;
            }
        }

        public int SampleRate {
            get;
            private set;
        }

        internal Transmission Transmission {
            get { return transmission; }
        }

        public string WriteKey {
            get;
            private set;
        }

        public void Add (IEnumerable<KeyValuePair<string, object>> data)
        {
            if (data == null)
                throw new ArgumentNullException (nameof (data));

            fields.Add (data);
        }

        public void AddField (string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException (nameof (name));

            fields.AddField (name, value);
        }

        public void AddDynamicField (string name, Func<object> func)
        {
            if (name == null)
                throw new ArgumentNullException (nameof (name));
            if (func == null)
                throw new ArgumentNullException (nameof (func));

            fields.AddDynamicField (name, func);
        }

        public void Dispose ()
        {
            if (transmission == null)
                return;
            
            transmission.Dispose ();
            transmission = null;
        }

        static bool IsSchemeSupported (string scheme)
        {
            return scheme == "http" || scheme == "https";
        }

        public void SendNow (IEnumerable<KeyValuePair<string, object>> data)
        {
            if (data == null)
                throw new ArgumentNullException (nameof (data));

            if (IsDisposed)
                throw new SendException ("Tried to send on a closed libhoney");

            var ev = new Event (this);
            ev.Add (data);
            ev.Send ();
        }
    }
}
