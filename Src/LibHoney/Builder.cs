using System;
using System.Linq;
using System.Collections.Generic;

namespace Honeycomb
{
    /// <summary>
    /// Create templates for new events, specifying default fields and override settings.
    /// </summary>
    public class Builder
    {
        LibHoney libHoney;
        FieldHolder fields = new FieldHolder ();

        /// <summary>
        /// Initializes Builder belong to a LibHoney object.
        /// </summary>
        public Builder (LibHoney libHoney)
            : this (libHoney,
                    Enumerable.Empty<KeyValuePair<string, object>> (), Enumerable.Empty<KeyValuePair<string, Func<object>>> ())
        {
        }

        /// <summary>
        /// Initializes Builder belong to a LibHoney object.
        /// </summary>
        public Builder (LibHoney libHoney, IEnumerable<KeyValuePair<string, object>> data)
            : this (libHoney, data, Enumerable.Empty<KeyValuePair<string, Func<object>>> ())
        {
        }

        /// <summary>
        /// Initializes Builder belong to a LibHoney object.
        /// </summary>
        public Builder (LibHoney libHoney,
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

            // Stash these values away for Send()
            WriteKey = libHoney.WriteKey;
            DataSet = libHoney.DataSet;
            SampleRate = libHoney.SampleRate;
        }

        /// <summary>
        /// Name of the Honeycomb dataset to which to send these events.
        /// </summary>
        /// <value>The data set.</value>
        public string DataSet {
            get;
            private set;
        }

        /// <summary>
        /// Rate at which to sample this event.
        /// </summary>
        /// <value>The sample rate.</value>
        public int SampleRate {
            get;
            private set;
        }

        /// <summary>
        /// Honeycomb authentication token.
        /// </summary>
        /// <value>The write key.</value>
        public string WriteKey {
            get;
            private set;
        }

        /// <summary>
        /// Adds data to the scope of this Builder. These metrics will
        /// be inherited by all events.
        /// </summary>
        /// <param name="data">Data.</param>
        public void Add (IEnumerable<KeyValuePair<string, object>> data)
        {
            if (data == null)
                throw new ArgumentNullException (nameof (data));

            fields.Add (data);
        }

        /// <summary>
        /// Adds data to the scope of this Builder. This metric will
        /// be inherited by all events.
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
        /// created and added as a field (with name as the key) to the
        /// newly created event.
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
        /// Clones this Builder, including its default metrics, WriteKey,
        /// DataSet and SampleRate.
        /// </summary>
        public Builder Clone ()
        {
            var builder = new Builder (libHoney);
            builder.fields.Add (fields);
            builder.WriteKey = WriteKey;
            builder.DataSet = DataSet;
            builder.SampleRate = SampleRate;
            return builder;
        }

        /// <summary>
        /// Creates a new event prepopulated with any metric present in
        /// this Builder.
        /// </summary>
        /// <returns>The event.</returns>
        public Event NewEvent ()
        {
            return new Event (libHoney, fields, WriteKey, DataSet, SampleRate);
        }

        /// <summary>
        /// Shortcut to NewEvent() and send the event.
        /// </summary>
        public void SendNow ()
        {
            SendNow (Enumerable.Empty<KeyValuePair<string, object>> ());
        }

        /// <summary>
        /// Shortcut to NewEvent(), add data, and send the event.
        /// </summary>
        /// <param name="data">Data.</param>
        public void SendNow (IEnumerable<KeyValuePair<string, object>> data)
        {
            if (data == null)
                throw new ArgumentNullException (nameof (data));

            var ev = NewEvent ();
            ev.Add (data);
            ev.Send ();
        }
    }
}
