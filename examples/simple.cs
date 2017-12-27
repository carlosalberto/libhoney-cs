using System;
using System.Collections.Generic;
using System.Threading;

using Honeycomb;


public class SimpleExample
{
    static void Main ()
    {
        string writeKey = "abcabc123123defdef456456";
        string dataSet = "simple";

        var honey = new LibHoney(writeKey, dataSet);
        var rand = new Random ();
        
        // Send 10 events with a random number.
        for (int i = 0; i < 10; i++) {
            honey.SendNow (new Dictionary<string, object> () {
                ["message"] = "Diagnostic #" + i,
                ["counter"] = rand.Next (100),
            });

            Thread.Sleep (TimeSpan.FromMilliseconds (100));
        }

        // Close the client.
        honey.Close ();
    }
}
