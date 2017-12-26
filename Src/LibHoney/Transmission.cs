using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Honeycomb
{
    class Transmission : IDisposable
    {
        BlockingCollection<Event> pending;
        BlockingCollection<Response> responses;
        CountdownEvent countdownEv;
        HttpClient client;
        bool disposed;

        public const int DefaultTimeoutInSeconds = 10;
        public const int DefaultMaxPendingEvents = 1000;
        public const int DefaultMaxPendingResponses = 2000;

        internal const string HoneyTeamKey = "X-Hny-Team";
        internal const string HoneySamplerate = "X-Hny-Samplerate";
        internal const string HoneyEventTime = "X-Hny-Event-Time";
        internal const string HoneyUserAgent = "libhoney-net/" + LibHoney.Version;

        const string HoneyEventsUrl = "/1/events/";

        public Transmission (int maxConcurrentBatches, bool blockOnSend, bool blockOnResponse)
            : this (maxConcurrentBatches, blockOnSend, blockOnResponse,
                    DefaultTimeoutInSeconds, DefaultMaxPendingEvents, DefaultMaxPendingResponses)
        {
        }

        public Transmission (int maxConcurrentBatches, bool blockOnSend, bool blockOnResponse,
                             int timeout, int maxPendingEvents, int maxPendingResponses)
        {
            MaxConcurrentBatches = maxConcurrentBatches;
            BlockOnSend = blockOnSend;
            BlockOnResponse = blockOnResponse;

            pending = new BlockingCollection<Event> (maxPendingEvents);
            responses = new BlockingCollection<Response> (maxPendingResponses);

            client = new HttpClient ();
            client.DefaultRequestHeaders.UserAgent.ParseAdd (HoneyUserAgent);
            client.Timeout = TimeSpan.FromSeconds (timeout);

            InitBackgroundSendThreads ();
        }

        // Use old nice Thread objects, as we do not want to touch
        // the threadpool, and Task objects make use of it.
        // Also catch all the exceptions, as not handling them would
        // crash the *entire* application to crash.
        void InitBackgroundSendThreads ()
        {
            countdownEv = new CountdownEvent (MaxConcurrentBatches);

            for (int i = 0; i < MaxConcurrentBatches; i++) {
                var t = new Thread (() => {
                    try {
                        // Wait indefinitely for incoming items,
                        // till adding is marked as complete.
                        Event ev;
                        while (pending.TryTake (out ev, -1))
                            DoSend (ev);

                    } catch (Exception exc) {
                        // Unexpected error - report it and let the thread be done.
                        var res = new Response () {
                            ErrorMessage = "Fatal error: " + exc.Message,
                        };
                        EnqueueResponse (res);
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

        public int MaxConcurrentBatches {
            get;
            private set;
        }

        public BlockingCollection<Response> Responses {
            get { return responses; }
        }

        public BlockingCollection<Event> PendingEvents {
            get { return pending; }
        }

        public void Dispose ()
        {
            if (disposed)
                return;

            disposed = true;

            // Let our threads know we are done adding items.
            pending.CompleteAdding ();

            // Wait for our threads to be done and be signaled.
            countdownEv.Wait ();

            // Signal to the responses queue that nothing more is coming.
            responses.CompleteAdding ();

            pending.Dispose ();
            client.Dispose ();
        }

        public void EnqueueResponse (Response res)
        {
            if (BlockOnResponse)
                responses.Add (res);
            else
                responses.TryAdd (res);
        }

        public void Send (Event ev)
        {
            ev = ev.Clone (); // Prevent further changes
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
                EnqueueResponse (res);
            }
        }

        void DoSend (Event ev)
        {
            var uri = new Uri (ev.ApiHost + HoneyEventsUrl + ev.DataSet);

            using (MemoryStream ms = new MemoryStream()) {
                using (GZipStream gzip = new GZipStream(ms,
							CompressionMode.Compress, true))
                {
                    var evBytes = Encoding.UTF8.GetBytes(ev.ToJSON());
                    gzip.Write(evBytes, 0, evBytes.Length);
                }

                ms.Position = 0;

                var req = new HttpRequestMessage () {
                    RequestUri = uri,
                    Method = HttpMethod.Post,
                    Content = new StreamContent(ms)
                };
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                req.Content.Headers.ContentEncoding.Add ("gzip");
                req.Headers.Add (HoneyTeamKey, ev.WriteKey);
                req.Headers.Add (HoneySamplerate, ev.SampleRate.ToString ());
                req.Headers.Add (HoneyEventTime, ev.CreatedAtISO);

                HttpResponseMessage result = null;
                DateTime start = DateTime.Now;
                string errorMessage = null;

                try {
                    // Get the Result right away to hint the scheduler to run the task inline.
                    result = client.SendAsync (req).Result;
                } catch (AggregateException exc) {
                    // Ignore network errors, but report them as responses.
                    if (!IsNetworkError (exc.InnerException, out errorMessage))
                        throw;
                }

                Response res;
                if (result != null) {
                    res = new Response () {
                        StatusCode = result.StatusCode,
                        Body = result.Content.ReadAsStringAsync ().Result
                    };
                } else {
                    res = new Response () {
                        ErrorMessage = "Error while sending the event: " + errorMessage,
                    };
                }

                res.Duration = DateTime.Now - start;
                res.Metadata = ev.Metadata;
                EnqueueResponse (res);
            }
        }

        static bool IsNetworkError (Exception exc, out string errorMessage)
        {
            errorMessage = null;

            // .Net and newer versions of Mono wrap a WebException
            // with a HttpRequestException.
            if (exc is HttpRequestException)
                exc = exc.InnerException;

            // 1. Connection refused or network error
            if (exc is WebException) {
                errorMessage = "Network error";
                return true;
            }

            // 2. Time out.
            if (exc is TaskCanceledException) {
                errorMessage = "Operation timed out";
                return true;
            }

            // 3. Error while processing the network stream
            if (exc is IOException) {
                errorMessage = "Failed to read the response data";
                return true;
            }

            return false;
        }
    }
}
