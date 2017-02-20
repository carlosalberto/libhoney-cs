using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LibHoney
{
    class Transmission : IDisposable
    {
        Task sendTask;
        BlockingCollection<Event> pending;
        HttpClient client;

        const int TimeoutInSeconds = 10;
        const int MaxPendingEvents = 1000;

        const string HoneyTeamKey = "X-Hny-Team";
        const string HoneySamplerate = "X-Hny-Samplerate";
        const string HoneyEventTime = "X-Hny-Event-Time";
        const string HoneyUserAgent = "libhoney-net/" + Honey.Version;

        const string HoneyEventsUrl = "/1/events/";

        public Transmission (bool blockOnSend, bool blockOnResponse)
        {
            BlockOnSend = blockOnSend;
            BlockOnResponse = blockOnResponse;

            pending = new BlockingCollection<Event> (MaxPendingEvents);

            client = new HttpClient ();
            client.DefaultRequestHeaders.UserAgent.ParseAdd (HoneyUserAgent);
            client.Timeout = TimeSpan.FromSeconds (TimeoutInSeconds);

            InitBackgroundSendTask ();
        }

        // Used a single thread, from which we do all the delivering,
        // as we dont want to pollute the threadpool. See:
        // https://channel9.msdn.com/Events/TechEd/Europe/2013/DEV-B318
        void InitBackgroundSendTask ()
        {
            sendTask = Task.Run (() => {
                while (true) {
                    var ev = pending.Take ();
                    if (ev == null) // Signaled we are stopping the task.
                        return;

                    DoSend (ev);
                }
            });
        }

        public bool BlockOnSend {
            get;
            private set;
        }

        public bool BlockOnResponse {
            get;
            private set;
        }

        public void Dispose ()
        {
            // Let the send task know we are done (without forcing a cancellation).
            pending.Add (null);

            // Wait for it to finish - no need to dispose it. See:
            // https://blogs.msdn.microsoft.com/pfxteam/2012/03/25/do-i-need-to-dispose-of-tasks/
            try {
                sendTask.Wait ();
            } catch (AggregateException) {
            }

            pending.Dispose ();
            client.Dispose ();
        }

        public void Send (Event ev)
        {
            bool success = true;

            if (BlockOnSend)
                pending.Add (ev);
            else
                success = pending.TryAdd (ev);

            if (!success) {
                // XXX (calberto): Add an overflow error to a responses queue.
            }
        }

        void DoSend (Event ev)
        {
            var uri = new Uri (ev.ApiHost + HoneyEventsUrl + ev.DataSet);

            var req = new HttpRequestMessage () {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = new StringContent (ev.ToJSON (), Encoding.UTF8, "application/json")
            };
            req.Headers.Add (HoneyTeamKey, ev.WriteKey);
            req.Headers.Add (HoneySamplerate, ev.SampleRate.ToString ());
            req.Headers.Add (HoneyEventTime, ev.CreatedAtISO);

            // XXX (calberto): Report errors here as a response available to the user:
            // 1. status code != 200
            // 2. TaskCanceledOperation (timeout)
            // 3. WebException (connection refused)
            try {
                // Get the Result right away to hint the scheduler to run the task inline.
                client.SendAsync (req).Wait ();
            } catch (AggregateException exc) {
                exc.Handle ((arg) => {
                    return arg.InnerException is WebException || arg.InnerException is TaskCanceledException;
                });
            }
        }
    }
}
