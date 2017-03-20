using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LibHoney
{
    public static class Honey
    {
        public const string Version = "1.0.0.0";

        public const string DefaultApiHost = "https://api.honeycomb.io";
        public const int DefaultSampleRate = 1;
        public const int DefaultMaxConcurrentBatches = 10;
        const bool DefaultBlock = false;

        readonly static FieldHolder fields = new FieldHolder ();

        static Transmission transmission;
        static BlockingCollection<Response> responses;

        static Honey ()
        {
            ResetProperties (true);
        }

        static void ResetProperties (bool discardState)
        {
            WriteKey = DataSet = ApiHost = null;
            SampleRate = 0;
            BlockOnSend = BlockOnResponse = false;

            if (discardState) {
                fields.Clear ();
                responses = new BlockingCollection<Response> (1);
            }
        }

        public static string ApiHost {
            get;
            private set;
        }

        public static bool BlockOnSend {
            get;
            private set;
        }

        public static bool BlockOnResponse {
            get;
            private set;
        }

        public static string DataSet {
            get;
            private set;
        }

        internal static FieldHolder Fields {
            get { return fields; }
        }

        public static bool IsInitialized {
            get { return transmission != null; }
        }

        public static BlockingCollection<Response> Responses {
            get {
                return responses;
            }
        }

        public static int SampleRate {
            get;
            private set;
        }

        internal static Transmission Transmission {
            get { return transmission; }
        }

        public static string WriteKey {
            get;
            private set;
        }

        public static void Add (IEnumerable<KeyValuePair<string, object>> data)
        {
            if (data == null)
                throw new ArgumentNullException (nameof (data));

            fields.Add (data);
        }

        public static void AddField (string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException (nameof (name));

            fields.AddField (name, value);
        }

        public static void AddDynamicField (string name, Func<object> func)
        {
            if (name == null)
                throw new ArgumentNullException (nameof (name));
            if (func == null)
                throw new ArgumentNullException (nameof (func));

            fields.AddDynamicField (name, func);
        }

        public static void Close ()
        {
            Close (false);
        }

        public static void Close (bool discardState)
        {
            if (transmission != null) {
                transmission.Dispose ();
                transmission = null;
            }

            ResetProperties (discardState);
        }

        public static void Init (string writeKey, string dataSet)
        {
            Init (writeKey, dataSet, DefaultApiHost, DefaultSampleRate, DefaultMaxConcurrentBatches, DefaultBlock, DefaultBlock);
        }

        public static void Init (string writeKey, string dataSet, string apiHost)
        {
            Init (writeKey, dataSet, apiHost, DefaultSampleRate, DefaultMaxConcurrentBatches, DefaultBlock, DefaultBlock);
        }

        public static void Init (string writeKey, string dataSet, string apiHost, int sampleRate, int maxConcurrentBatches)
        {
            Init (writeKey, dataSet, apiHost, sampleRate, maxConcurrentBatches, DefaultBlock, DefaultBlock);
        }

        // XXX (calberto) expose the responses as a list, either as simple containers or Tasks
        public static void Init (string writeKey, string dataSet, string apiHost,
            int sampleRate, int maxConcurrentBatches, bool blockOnSend, bool blockOnResponse)
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

            // Prevent accidental/unintended re-initilization.
            if (transmission != null)
                throw new InvalidOperationException ("Honey is initialized already. Close it to initialize again.");

            transmission = new Transmission (maxConcurrentBatches, blockOnSend, blockOnResponse);
            responses = transmission.Responses;

            WriteKey = writeKey;
            DataSet = dataSet;
            ApiHost = apiHost;
            SampleRate = sampleRate;
            BlockOnSend = blockOnSend;
            BlockOnResponse = blockOnResponse;
        }

        static bool IsSchemeSupported (string scheme)
        {
            return scheme == "http" || scheme == "https";
        }

        public static void SendNow (IEnumerable<KeyValuePair<string, object>> data)
        {
            if (data == null)
                throw new ArgumentNullException (nameof (data));

            if (!IsInitialized)
                throw new SendException ("Tried to send on a closed or uninitialized libhoney");

            var ev = new Event ();
            ev.Add (data);
            ev.Send ();
        }
    }
}
