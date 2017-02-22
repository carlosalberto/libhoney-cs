using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace LibHoney
{
    class FieldHolder
    {
        Dictionary<string, object> fields = new Dictionary<string, object> ();
        Dictionary<string, Func<object>> dynFields = new Dictionary<string, Func<object>> ();

        public Dictionary<string, Func<object>> DynamicFields {
            get { return dynFields; }
        }

        public Dictionary<string, object> Fields {
            get { return fields; }
        }

        public bool IsEmpty {
            get { return fields.Count == 0; }
        }

        static bool IsObjectSupported (object obj)
        {
            if (obj == null)
                return true;

            Type t = obj.GetType ();
            if (t.IsPrimitive || t.IsEnum)
                return true;

            return t == typeof (string) || t == typeof (DateTime) || t == typeof (TimeSpan) || t == typeof (Decimal);
        }

        public void AddField (string name, object value)
        {
            if (!IsObjectSupported (value))
                value = JsonConvert.SerializeObject (value);
            
            fields [name] = value;
        }

        public void AddDynamicField (string name, Func<object> func)
        {
            dynFields [name] = func;
        }

        internal void Add (FieldHolder fh)
        {
            Add (fh.fields);
            AddDynamic (fh.dynFields);
        }

        public void Add (IEnumerable<KeyValuePair<string, object>> data)
        {
            foreach (var kvp in data)
                AddField (kvp.Key, kvp.Value);
        }

        public void AddDynamic (IEnumerable<KeyValuePair<string, Func<object>>> dynFields)
        {
            foreach (var kvp in dynFields)
                this.dynFields [kvp.Key] = kvp.Value;
        }

        public void EvaluateDynamicFields ()
        {
            var evaluatedFields = dynFields.Select (kvp => new KeyValuePair<string, object> (kvp.Key, kvp.Value ()));
            foreach (var kvp in evaluatedFields)
                AddField (kvp.Key, kvp.Value);
        }

        public void Clear ()
        {
            fields.Clear ();
            dynFields.Clear ();
        }

        public string ToJSON ()
        {
            return JsonConvert.SerializeObject (fields);
        }
    }
}
