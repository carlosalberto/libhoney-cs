using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

using Honeycomb;


public class FactorialExample
{
    static long Factorial (int n)
    {
        if (n < 0)
            return -1 * Factorial (Math.Abs (n));
        if (n == 0)
            return 1;

        return n * Factorial (n - 1);
    }

    static void ReadResponses (ResponseCollection responses)
    {
        Response res;
        while (responses.TryTake (out res, -1))
            Console.WriteLine ("sending event with metadata {0} took {1}ms and got response code {2} with message \"{3}\"",
                                    res.Metadata, res.Duration.TotalMilliseconds, (int)res.StatusCode, res.Body);
    }

    static void RunFact (int low, int high, Builder libhBuilder)
    {
        for (int i = low; i < high; i++) {
            var ev = libhBuilder.NewEvent ();
            ev.Metadata = String.Format ("fn=runfact, i={0}", i);

            var start = DateTime.Now;
            long retval = Factorial (10 + i);
            var elapsed = DateTime.Now - start;

            ev.AddField ("fact", elapsed.TotalMilliseconds);
            ev.AddField ("retval", retval);
            ev.Send ();
            Console.WriteLine ("About to send event: " + ev.ToJSON ());
        }
    }

    static void Main ()
    {
        string writeKey = "abcabc123123defdef456456";
        string dataSet = "factorial";

        var honey = new LibHoney (writeKey, dataSet);
        Task.Run (() => ReadResponses (honey.Responses));

        // Attach fields to the top-level instance.
        honey.AddField ("version", "3.4.5");
        honey.AddDynamicField ("num_threads", () => Process.GetCurrentProcess ().Threads.Count);

        // Sends an event with "version", "num_threads", and "status" fields.
        honey.SendNow ("status", "starting run");

        RunFact (1, 2, new Builder (honey, new Dictionary<string, object> () { ["range"] = "low" }));
        RunFact (31, 32, new Builder (honey, new Dictionary<string, object> () { ["range"] = "high" }));

        honey.SendNow ("status", "sending now");
        honey.Dispose ();
    }
}
