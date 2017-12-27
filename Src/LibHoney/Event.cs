using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Honeycomb
{
    /// <summary>
    /// Hold data that can be sent to Honeycomb. It can also
    /// specify overrides of the config settings.
    /// </summary>
    public class Event
    {
        LibHoney libHoney;
        FieldHolder fields = new FieldHolder ();

        static readonly ThreadLocal<Random> rand = new ThreadLocal<Random> (() => new Random ());

        /// <summary>
        /// Initializes Event belonging to a LibHoney object.
        /// </summary>
        public Event (LibHoney libHoney)
            : this (libHoney,
                    Enumerable.Empty<KeyValuePair<string, object>> (), Enumerable.Empty<KeyValuePair<string, Func<object>>> ())
        {
        }

        /// <summary>
        /// Initializes Event belonging to a LibHoney object.
        /// </summary>
        public Event (LibHoney libHoney, IEnumerable<KeyValuePair<string, object>> data)
            : this (libHoney,
                    data, Enumerable.Empty<KeyValuePair<string, Func<object>>> ())
        {
        }

        /// <summary>
        /// Initializes Event belonging to a LibHoney object.
        /// </summary>
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
            Timestamp = DateTime.Now;
            WriteKey = libHoney.WriteKey;
            DataSet = libHoney.DataSet;
            ApiHost = libHoney.ApiHost;
            SampleRate = libHoney.SampleRate;
        }

        internal Event (LibHoney libHoney, FieldHolder fh, string writeKey, string dataSet, string apiHost, int sampleRate)
            : this (libHoney, fh.Fields, fh.DynamicFields)
        {
            WriteKey = writeKey;
            DataSet = dataSet;
            ApiHost = apiHost;
            SampleRate = sampleRate;
        }

        internal Event (Event ev)
        {
            libHoney = ev.libHoney;
            fields.Add (ev.Fields);

            Timestamp = ev.Timestamp;
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

        /// <summary>
        /// Hostname for the Honeycomb API server to which to send this event.
        /// </summary>
        /// <value>The API hostname.</value>
        public string ApiHost {
            get;
            set;
        }

        /// <summary>
        /// DateTime containing the creation time of this Event.
        /// </summary>
        /// <value>The creation time.</value>
        public DateTime Timestamp {
            get;
            set;
        }

        /// <summary>
        /// Returns the creation time of this Event as a ISO
        /// datetime string.
        /// </summary>
        /// <value>The creation time as a ISO datetime string.</value>
        public string TimestampISO {
            get { return Timestamp.ToString ("O"); }
        }

        /// <summary>
        /// Name of the Honeycomb dataset to which to send these events.
        /// </summary>
        /// <value>The data set.</value>
        public string DataSet {
            get;
            set;
        }

        internal FieldHolder Fields {
            get { return fields; }
        }

        /// <summary>
        /// Field for the user to add in data that will be handed back on
        /// Response object read off the responses queue. It is not sent to
        /// Honeycomb with the event.
        /// </summary>
        /// <value>The metadata.</value>
        public object Metadata {
            get;
            set;
        }

        /// <summary>
        /// Rate at which to sample this event.
        /// </summary>
        /// <value>The sample rate.</value>
        public int SampleRate {
            get;
            set;
        }

        /// <summary>
        /// Honeycomb authentication token.
        /// </summary>
        /// <value>The write key.</value>
        public string WriteKey {
            get;
            set;
        }

        /// <summary>
        /// Adds data to this event.
        /// </summary>
        /// <param name="data">Data.</param>
        public void Add (IEnumerable<KeyValuePair<string, object>> data)
        {
            if (data == null)
                throw new ArgumentNullException (nameof (data));

            fields.Add (data);
        }

        /// <summary>
        /// Adds a single metric to this event.
        /// </summary>
        /// <param name="name">Name of the metric.</param>
        /// <param name="name">Value of the metric.</param>
        public void AddField (string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException (nameof (name));

            fields.AddField (name, value);
        }

        /// <summary>
        /// Clones this Event.
        /// </summary>
        public Event Clone ()
        {
            return new Event (this);
        }

        /// <summary>
        /// Dispatches the event to be sent to Honeycomb, sampling if necessary.
        /// 
        /// If you have sampling enabled
        /// (i.e. SampleRate >1), Send will only actually transmit data with a
        /// probability of 1/SampleRate. No error is returned whether or not traffic
        /// is sampled, however, the Response sent down the response queue will
        /// indicate the event was sampled in the ErrorMessage property.
        /// </summary>
        public void Send ()
        {
            if (libHoney.IsClosed)
                throw new SendException ("Tried to send on a closed libhoney");

            if (ShouldDrop (SampleRate)) {
                SendDroppedResponse ();
                return;
            }

            SendPreSampled ();
        }

        /// <summary>
        /// Dispatches the event to be sent to Honeycomb.
        /// 
        /// Sampling is assumed to have already happened. SendPresampled will dispatch
        /// every event handed to it, and pass along the sample rate. Use this instead of
        /// Send() when the calling function handles the logic around which events to
        /// drop when sampling.
        /// </summary>
        public void SendPreSampled ()
        {
            if (libHoney.IsClosed)
                throw new SendException ("Tried to send on a closed libhoney");
            
            if (fields.IsEmpty)
                throw new SendException ("No metrics added to event. Will not send empty event");
            if (ApiHost == null)
                throw new SendException ("No ApiHost for Honeycomb. Will not send event");
            if (WriteKey == null)
                throw new SendException ("No WriteKey for Honeycomb. Will not send event");
            if (DataSet == null)
                throw new SendException ("No DataSet for Honeycomb. Will not send event");

            libHoney.Transmission.Send (this);
        }

        void SendDroppedResponse ()
        {
            libHoney.Transmission.EnqueueResponse (new Response () {
                Metadata = Metadata,
                ErrorMessage = "Event dropped due to sampling"
            });
        }

        static bool ShouldDrop (int rate)
        {
            if (rate <= 1) // No need to generate a random.
                return false;
            
            return rand.Value.Next (1, rate + 1) != 1;
        }

        /// <summary>
        /// Returns the data of this event as a JSON string.
        /// </summary>
        /// <returns>The json.</returns>
        public string ToJSON ()
        {
            return fields.ToJSON ();
        }
    }
}
