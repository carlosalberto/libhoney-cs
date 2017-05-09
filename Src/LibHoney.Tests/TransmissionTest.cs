using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace LibHoney.Tests
{
    public class TransmissionFixture : IDisposable
    {
        public TransmissionFixture ()
        {
            DataSet = "test-honey";
            Server = new HttpTestServer ("http://127.0.0.1:8000/1/events/" + DataSet + "/");
        }

        public void Dispose ()
        {
            Server.Dispose ();
        }

        public HttpTestServer Server {
            get;
            set;
        }

        public String DataSet {
            get; set;
        }
    }

    public class TransmissionTest : IDisposable, IClassFixture<TransmissionFixture>
    {
        // NOTE: In the tests we call Dispose on the Transmission
        // object, as a way to wait for it to finish processing all
        // the events.
        TransmissionFixture fixture;

        public TransmissionTest (TransmissionFixture fixture)
        {
            this.fixture = fixture;

            Server = fixture.Server;
            Server.Clear ();

            SampleEvent = CreateSampleEvent ();
        }

        Event CreateSampleEvent ()
        {
            return new Event () {
                WriteKey = "key1",
                DataSet = fixture.DataSet,
                ApiHost = "http://127.0.0.1:8000",
                Metadata = new object ()
            };
        }

        HttpTestServer Server {
            get;
            set;
        }

        Event SampleEvent {
            get;
            set;
        }

        Transmission CurrentTransmission {
            get;
            set;
        }

        public void Dispose ()
        {
            Server.Stop ();

            if (CurrentTransmission != null)
                CurrentTransmission.Dispose ();
        }

        [Fact]
        public void Ctor ()
        {
            var t = CurrentTransmission = new Transmission (3, false, true);
            Assert.Equal (3, t.MaxConcurrentBatches);
            Assert.Equal (false, t.BlockOnSend);
            Assert.Equal (true, t.BlockOnResponse);
            Assert.NotNull (t.Responses);
            Assert.Equal (0, t.Responses.Count);
            Assert.Equal (Transmission.DefaultMaxPendingResponses, t.Responses.BoundedCapacity);
            Assert.NotNull (t.PendingEvents);
            Assert.Equal (0, t.PendingEvents.Count);
            Assert.Equal (Transmission.DefaultMaxPendingEvents, t.PendingEvents.BoundedCapacity);
        }

        [Fact]
        public void Simple ()
        {
            var t = CurrentTransmission = new Transmission (1, false, false, 1, 1, 1);

            Task.Run (() => Server.ServeOne ((int) HttpStatusCode.Created, "HoneyHoney"));
            Server.WaitReady ();

            t.Send (SampleEvent);
            t.Dispose ();

            Response res;
            t.Responses.TryTake (out res);
            Assert.NotNull (res);
            Assert.Equal (HttpStatusCode.Created, res.StatusCode);
            Assert.True (res.Duration > TimeSpan.FromSeconds (0));
            Assert.Equal (res.Body, "HoneyHoney");
            Assert.Equal (res.Metadata, SampleEvent.Metadata);
            Assert.Equal (res.ErrorMessage, null);
        }

        [Fact]
        public void SendEventOverflow ()
        {
            var t = CurrentTransmission = new Transmission (1, false, false, 1, 1, 1);

            // Wait a little so:
            // 1. The first event gets for processing
            // 2. The second event waits in the queue
            // 3. The third event gets discarded.
            Task.Run (() => {
                Server.ServeOne (200, "{}", TimeSpan.FromMilliseconds (200));
                Server.ServeOne (); // keep that event in peace (no further warning)
            });
            Server.WaitReady ();

            for (int i = 0; i < 3; i++)
                t.Send (SampleEvent);

            Response res;
            t.Responses.TryTake (out res);
            Assert.NotNull (res);
            Assert.Equal ("Event dropped; queue overflow", res.ErrorMessage);
            Assert.Equal (res.Metadata, SampleEvent.Metadata);
        }

        [Fact]
        public void DisposeResponsesFull ()
        {
            var t =  new Transmission (1, false, false, 1, 1, 1);
            t.Responses.Add (new Response ());

            // Shouldn't be causing an error nor stay waiting,
            // even if the responses queue is full.
            t.Dispose ();
        }

        [Fact]
        public void Timeout ()
        {
            var t = CurrentTransmission = new Transmission (1, false, false, 1 /* timeout in seconds */, 1, 1);

            Task.Run (() => Server.ServeOne (200, "{}", TimeSpan.FromMilliseconds (1000 + 200)));
            Server.WaitReady ();

            t.Send (SampleEvent);
            t.Dispose ();

            Response res;
            t.Responses.TryTake (out res, TimeSpan.FromMilliseconds (100));
            Assert.NotNull (res);
            Assert.Equal ("Error when sending the event: A task was canceled.", res.ErrorMessage);
            Assert.Equal (res.Metadata, SampleEvent.Metadata);
        }

        [Fact]
        public void Payload ()
        {
            var t = CurrentTransmission = new Transmission (1, false, false, 1, 10, 10);
            SampleEvent.AddField ("field0", 0);
            SampleEvent.AddField ("field1", "without value");
            SampleEvent.AddField ("field2", null);

            Task.Run (() => Server.ServeOne ());
            Server.WaitReady ();

            t.Send (SampleEvent);
            t.Dispose ();

            string payload = Server.PayloadItems [0];
            Assert.Equal ("{\"field0\":0,\"field1\":\"without value\",\"field2\":null}", payload);

            var headers = Server.Headers [0];
            Assert.Equal ("key1", headers.Get ("X-Hny-Team"));
            Assert.Equal ("0", headers.Get ("X-Hny-Samplerate"));
        }

        [Fact]
        public void MultipleBatches ()
        {
            int iters = 30;
            var t = CurrentTransmission = new Transmission (2, false, false, 1, iters, iters);

            Task.Run (() => Server.Serve (iters));
            Server.WaitReady ();

            for (int i = 0; i < iters; i++)
                t.Send (CreateSampleEvent ());

            t.Dispose ();

            Assert.Equal (30, Server.PayloadItems.Count);
            Assert.Equal (iters, t.Responses.Count);
            Assert.All (t.Responses, (obj) => Assert.Equal (200, (int)obj.StatusCode));
        }
    }
}
