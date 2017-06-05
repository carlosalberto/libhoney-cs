using System;
using System.Collections.Generic;
using System.Linq;

namespace Honeycomb
{
    public class Event
    {
        LibHoney libHoney;
        FieldHolder fields = new FieldHolder ();

        static readonly Random rand = new Random ();

        public Event (LibHoney libHoney)
            : this (libHoney,
                    Enumerable.Empty<KeyValuePair<string, object>> (), Enumerable.Empty<KeyValuePair<string, Func<object>>> ())
        {
        }

        public Event (LibHoney libHoney, IEnumerable<KeyValuePair<string, object>> data)
            : this (libHoney,
                    data, Enumerable.Empty<KeyValuePair<string, Func<object>>> ())
        {
        }

        public Event (LibHoney libHoney,
                      IEnumerable<KeyValuePair<string, object>> data, IEnumerable<KeyValuePair<string, Func<object>>> dynFields)
        {
            if (libHoney == null)
                throw new ArgumentNullException (nameof (libHoney));
            if (data == null)
                throw new ArgumentNullException (nameof (data));
            if (dynFields == null)
                throw new ArgumentNullException (nameof (dynFields));

            this.libHoney = libHoney;

            fields.Add (libHoney.Fields);
            fields.Add (data);
            fields.AddDynamic (dynFields);
            fields.EvaluateDynamicFields (); // Evalute all the dynamic fields

            // Stash these values away for Send()
            CreatedAt = DateTime.Now;
            WriteKey = libHoney.WriteKey;
            DataSet = libHoney.DataSet;
            ApiHost = libHoney.ApiHost;
            SampleRate = libHoney.SampleRate;
        }

        internal Event (LibHoney libHoney, FieldHolder fh, string writeKey, string dataSet, int sampleRate)
            : this (libHoney, fh.Fields, fh.DynamicFields)
        {
            WriteKey = writeKey;
            DataSet = dataSet;
            SampleRate = sampleRate;
        }

        internal Event (Event ev)
        {
            libHoney = ev.libHoney;
            fields.Add (ev.Fields);

            CreatedAt = ev.CreatedAt;
            WriteKey = ev.WriteKey;
            DataSet = ev.DataSet;
            ApiHost = ev.ApiHost;
            SampleRate = ev.SampleRate;
            Metadata = ev.Metadata;
        }

        // Transmission testing purposes.
        internal Event ()
        {
        }

        public string ApiHost {
            get;
            internal set;
        }

        public DateTime CreatedAt {
            get;
            internal set;
        }

        public string CreatedAtISO {
            get { return CreatedAt.ToString ("O"); }
        }

        public string DataSet {
            get;
            internal set;
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
            set;
        }

        public string WriteKey {
            get;
            internal set;
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

        // Convenience method.
        public Event Clone ()
        {
            return new Event (this);
        }

        public void Send ()
        {
            if (libHoney.IsDisposed)
                throw new SendException ("Tried to send on a closed libhoney");

            if (ShouldDrop (SampleRate)) {
                SendDroppedResponse ();
                return;
            }

            SendPreSampled ();
        }

        public void SendPreSampled ()
        {
            if (libHoney.IsDisposed)
                throw new SendException ("Tried to send on a closed libhoney");
            if (fields.IsEmpty)
                throw new SendException ("No metrics added to event. Will not send empty event");

            libHoney.Transmission.Send (this);
        }

        void SendDroppedResponse ()
        {
            libHoney.Responses.Add (new Response () {
                Metadata = Metadata,
                ErrorMessage = "Event dropped due to sampling"
            });
        }

        static bool ShouldDrop (int rate)
        {
            return rand.Next (1, rate + 1) != 1;
        }

        public string ToJSON ()
        {
            return fields.ToJSON ();
        }
    }
}