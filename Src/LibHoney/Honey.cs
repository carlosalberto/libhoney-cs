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
        public const int DefaultMaxCurrentBatches = 10;
        const bool DefaultBlock = false;

        readonly static FieldHolder fields = new FieldHolder ();
        readonly static ConcurrentQueue<object> responses = new ConcurrentQueue<object> ();

        //static Transmission transmission;

        static void Reset ()
        {
            IsInitialized = false;
            WriteKey = DataSet = ApiHost = null;
            SampleRate = MaxConcurrentBatches = 0;
            BlockOnSend = BlockOnResponse = false;
            fields.Clear ();
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
            get;
            private set;
        }

        public static int MaxConcurrentBatches {
            get;
            private set;
        }

        public static ConcurrentQueue<object> Responses {
            get { return responses; }
        }

        public static int SampleRate {
            get;
            private set;
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
            // TODO - close the transmission object
            Reset ();
        }

        public static void Init (string writeKey, string dataSet)
        {
            Init (writeKey, dataSet, DefaultApiHost, DefaultSampleRate, DefaultMaxCurrentBatches,
                DefaultBlock, DefaultBlock);
        }

        public static void Init (string writeKey, string dataSet, string apiHost)
        {
            Init (writeKey, dataSet, apiHost, DefaultSampleRate, DefaultMaxCurrentBatches,
                DefaultBlock, DefaultBlock);
        }

        public static void Init (string writeKey, string dataSet, string apiHost,
            int sampleRate, int maxConcurrentBatches)
        {
            Init (writeKey, dataSet, apiHost, sampleRate, maxConcurrentBatches,
                DefaultBlock, DefaultBlock);
        }

        // TODO - retrieve the global responses field from the Transmission object.
        public static void Init (string writeKey, string dataSet, string apiHost,
            int sampleRate, int maxConcurrentBatches,
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

            WriteKey = writeKey;
            DataSet = dataSet;
            ApiHost = apiHost;
            SampleRate = sampleRate;
            MaxConcurrentBatches = maxConcurrentBatches;
            BlockOnSend = blockOnSend;
            BlockOnResponse = blockOnResponse;

            //transmission = new Transmission (maxConcurrentBatches, blockOnSend, blockOnWrite);

            IsInitialized = true;
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
