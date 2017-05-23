using System;
using System.Linq;
using System.Collections.Generic;

namespace LibHoney
{
    public class Builder
    {
        Honey libHoney;
        FieldHolder fields = new FieldHolder ();

        public Builder (Honey libHoney)
            : this (libHoney,
                    Enumerable.Empty<KeyValuePair<string, object>> (), Enumerable.Empty<KeyValuePair<string, Func<object>>> ())
        {
        }

        public Builder (Honey libHoney, IEnumerable<KeyValuePair<string, object>> data)
            : this (libHoney, data, Enumerable.Empty<KeyValuePair<string, Func<object>>> ())
        {
        }

        public Builder (Honey libHoney,
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

        public string DataSet {
            get;
            private set;
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

        public void AddDynamicField (string name, Func<object> func)
        {
            if (name == null)
                throw new ArgumentNullException (nameof (name));
            if (func == null)
                throw new ArgumentNullException (nameof (func));

            fields.AddDynamicField (name, func);
        }

        public Builder Clone ()
        {
            var builder = new Builder (libHoney);
            builder.fields.Add (fields);
            builder.WriteKey = WriteKey;
            builder.DataSet = DataSet;
            builder.SampleRate = SampleRate;
            return builder;
        }

        public Event NewEvent ()
        {
            return new Event (libHoney, fields, WriteKey, DataSet, SampleRate);
        }

        public void SendNow ()
        {
            SendNow (Enumerable.Empty<KeyValuePair<string, object>> ());
        }

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
