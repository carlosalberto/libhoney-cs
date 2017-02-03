using System;
using System.Collections.Generic;
using System.Linq;

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

        public void AddField (string name, object value)
        {
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
                this.fields [kvp.Key] = kvp.Value;
        }

        public void AddDynamic (IEnumerable<KeyValuePair<string, Func<object>>> dynFields)
        {
            foreach (var kvp in dynFields)
                this.dynFields [kvp.Key] = kvp.Value;
        }

        public void EvaluateDynamicFields ()
        {
            var evaluatedFields = dynFields.Select (kvp => new KeyValuePair<string, object> (kvp.Key, kvp.Value ()));
            Add (evaluatedFields);
        }

        public void Clear ()
        {
            fields.Clear ();
            dynFields.Clear ();
        }
    }
}
