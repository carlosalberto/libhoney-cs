using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Honeycomb;


public class DynamicExample
{
    static void Main ()
    {
        string writeKey = "abcabc123123defdef456456";
        string dataSet = "dynamic";

        var honey = new LibHoney (writeKey, dataSet);
        var builder = new Builder (honey);

        // Attach fields to the Builder instance
        builder.AddField ("start", DateTime.Now);
        builder.AddDynamicField ("end", () => DateTime.Now);
 
        var tasks = new Task [8];

        foreach (int i in Enumerable.Range (0, tasks.Length)) {
            var t = Task.Run (async () => {
                // Simulate some work.
                await Task.Delay (new Random ().Next (100));

                // builder comes with the "start" and "end" fields
                // already populated ("end" being a dynamic field)
                var ev = builder.NewEvent ();
                ev.AddField ("id", i);
                ev.Send ();
            });
            tasks [i] = t;
        }

        Task.WaitAll (tasks);
        honey.Dispose ();
    }
}
