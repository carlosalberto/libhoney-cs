using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Honeycomb;


public class ResponsesExample
{
    static void ReadResponses (ResponseCollection responses)
    {
        Response res;
        while (responses.TryTake (out res, -1))
            Console.WriteLine (res);
    }

    static void Main ()
    {
        string writeKey = "abcabc123123defdef456456";
        string dataSet = "responses";

        var honey = new LibHoney (writeKey, dataSet);
        var responses = honey.Responses;

        Task.Run (() => ReadResponses (responses));

        var tasks = new Task [8];
        foreach (int i in Enumerable.Range (0, tasks.Length)) {
            var t = Task.Run (async () => {
                // Simulate work.
                var start = DateTime.Now;
                await Task.Delay (new Random ().Next (100));

                // Work is done, send the task id and its elapsed time.
                var ev = new Event (honey);
                ev.AddField ("id", i);
                ev.AddField ("elapsed", (DateTime.Now - start).TotalMilliseconds);
                ev.Send ();
            });
            tasks [i] = t;
        }

        Task.WaitAll (tasks);
        honey.Close ();
    }
}
