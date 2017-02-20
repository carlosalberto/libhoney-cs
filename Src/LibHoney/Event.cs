using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace LibHoney
{
    public class Event
    {
        FieldHolder fields = new FieldHolder ();

        static readonly Random rand = new Random ();

        public Event ()
            : this (Enumerable.Empty<KeyValuePair<string, object>> (), Enumerable.Empty<KeyValuePair<string, Func<object>>> ())
        {
        }

        public Event (IEnumerable<KeyValuePair<string, object>> data)
            : this (data, Enumerable.Empty<KeyValuePair<string, Func<object>>> ())
        {
        }

        public Event (IEnumerable<KeyValuePair<string, object>> data, IEnumerable<KeyValuePair<string, Func<object>>> dynFields)
        {
            if (data == null)
                throw new ArgumentNullException (nameof (data));
            if (dynFields == null)
                throw new ArgumentNullException (nameof (dynFields));

            fields.Add (Honey.Fields); // Bring the global fields
            fields.Add (data);
            fields.AddDynamic (dynFields);
            fields.EvaluateDynamicFields (); // Evalute all the dynamic fields

            // Stash these values away for Send()
            CreatedAt = DateTime.Now;
            WriteKey = Honey.WriteKey;
            DataSet = Honey.DataSet;
            ApiHost = Honey.ApiHost;
            SampleRate = Honey.SampleRate;
        }

        internal Event (FieldHolder fh, string writeKey, string dataSet, int sampleRate)
            : this (fh.Fields, fh.DynamicFields)
        {
            WriteKey = writeKey;
            DataSet = dataSet;
            SampleRate = sampleRate;
        }

        public string ApiHost {
            get;
            private set;
        }

        public DateTime CreatedAt {
            get;
            private set;
        }

        public string CreatedAtISO {
            get { return CreatedAt.ToString ("O"); }
        }

        public string DataSet {
            get;
            private set;
        }

        internal FieldHolder Fields {
            get { return fields; }
        }

        public object Metadata {
            get;
            set;
        }

        public int SampleRate {
            get;
            private set;
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

        public void Send ()
        {
            if (!Honey.IsInitialized)
                throw new SendException ("Tried to send on a closed or uninitialized libhoney");

            if (ShouldDrop (SampleRate)) {
                SendDroppedResponse ();
                return;
            }

            SendPreSampled ();
        }

        public void SendPreSampled ()
        {
            if (!Honey.IsInitialized)
                throw new SendException ("Tried to send on a closed or uninitialized libhoney");
            if (fields.IsEmpty)
                throw new SendException ("No metrics added to event. Will not send empty event");

            Honey.Transmission.Send (this);
        }

        void SendDroppedResponse ()
        {
            // XXX (calberto) Add the response object to the responses queue.
        }

        static bool ShouldDrop (int rate)
        {
            return rand.Next (1, rate + 1) != 1;
        }

        public string ToJSON ()
        {
            return JsonConvert.SerializeObject (fields.Fields);
        }
    }
}