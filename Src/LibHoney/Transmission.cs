using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibHoney
{
    class Transmission : IDisposable
    {
        int maxConcurrentBatches;
        BlockingCollection<Event> pending;
        BlockingCollection<Response> responses;
        CountdownEvent countdownEv;
        HttpClient client;

        const int TimeoutInSeconds = 10;
        const int MaxPendingEvents = 1000;
        const int MaxPendingResponses = 2000;

        const string HoneyTeamKey = "X-Hny-Team";
        const string HoneySamplerate = "X-Hny-Samplerate";
        const string HoneyEventTime = "X-Hny-Event-Time";
        const string HoneyUserAgent = "libhoney-net/" + Honey.Version;

        const string HoneyEventsUrl = "/1/events/";

        public Transmission (int maxConcurrentBatches, bool blockOnSend, bool blockOnResponse)
        {
            BlockOnSend = blockOnSend;
            BlockOnResponse = blockOnResponse;

            pending = new BlockingCollection<Event> (MaxPendingEvents);
            responses = new BlockingCollection<Response> (MaxPendingResponses);

            client = new HttpClient ();
            client.DefaultRequestHeaders.UserAgent.ParseAdd (HoneyUserAgent);
            client.Timeout = TimeSpan.FromSeconds (TimeoutInSeconds);

            this.maxConcurrentBatches = maxConcurrentBatches;
            InitBackgroundSendThreads ();
        }

        // Use old nice Thread objects, as we do not want to touch
        // the threadpool, and Task objects make use of it.
        // Also catch all the exceptions, as not handling them would
        // crash the *entire* application to crash.
        void InitBackgroundSendThreads ()
        {
            countdownEv = new CountdownEvent (maxConcurrentBatches);

            for (int i = 0; i < maxConcurrentBatches; i++) {
                var t = new Thread (() => {
                    try {
                        var ev = pending.Take ();
                        if (ev == null) // Signaled we are stopping the task.
                            return;

                        DoSend (ev);
                    } catch (Exception) {
                        // (xxx) calberto: Where should we log this error?
                    } finally {
                        countdownEv.Signal ();
                    }
                });

                t.IsBackground = true;
                t.Start ();
            }
        }

        public bool BlockOnSend {
            get;
            private set;
        }

        public bool BlockOnResponse {
            get;
            private set;
        }

        public BlockingCollection<Response> Responses {
            get { return responses; }
        }

        public void Dispose ()
        {
            // Let our threads know we are done (without forcing a cancellation).
            for (int i = 0; i < maxConcurrentBatches; i++)
                pending.Add (null);

            // Wait for our threads to be done and be signaled.
            countdownEv.Wait ();

            // Try to signal to the responses queue that nothing more is coming
            responses.TryAdd (null);

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
                var res = new Response () {
                    Metadata = ev.Metadata,
                    ErrorMessage = "Event dropped; queue overflow"
                };
                if (BlockOnResponse)
                    responses.Add (res);
                else
                    responses.TryAdd (res);
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

            HttpResponseMessage result = null;
            DateTime start = DateTime.Now;

            // XXX (calberto): Report/log errors:
            // * TaskCanceledOperation (timeout)
            // * WebException (connection refused)
            try {
                // Get the Result right away to hint the scheduler to run the task inline.
                result = client.SendAsync (req).Result;
            } catch (AggregateException exc) {
                exc.Handle ((arg) => {
                    return arg.InnerException is WebException || arg.InnerException is TaskCanceledException;
                });
                return;
            }

            var res = new Response () {
                StatusCode = result.StatusCode,
                Duration = DateTime.Now - start,
                Metadata = ev.Metadata,
                Body = result.Content.ReadAsStringAsync ().Result
            };
            if (BlockOnResponse)
                responses.Add (res);
            else
                responses.TryAdd (res);
        }
    }
}
