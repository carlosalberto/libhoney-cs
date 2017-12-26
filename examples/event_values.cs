using System;
using System.Collections.Generic;
using System.Threading;

using Honeycomb;


public class EventValues
{
    static void Main ()
    {
        string writeKey = "abcabc123123defdef456456";
        string dataSet = "event_values";

        var honey = new LibHoney(writeKey, dataSet);
        var rand = new Random ();
        
        // Send 3 events with a random number.
        for (int i = 0; i < 10; i++) {
            honey.SendNow (new Dictionary<string, object> () {
                ["counter"] = rand.Next (100),
            });
        }

        // Send an event with the same writeKey but different
        // dataSet.
        var ev = new Event (honey);
        ev.DataSet = "master_values";
        ev.AddField ("latest_event", DateTime.Now.ToString ("O"));
        ev.Send ();

        // Close the client.
        honey.Close ();
    }
}
