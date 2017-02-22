using System;
using System.Collections.Generic;
using Xunit;

using LibHoney;

namespace LibHoney.Tests
{
    public class EventTest : IDisposable
    {
        public void Dispose ()
        {
            Honey.Close ();
        }

        [Fact]
        public void Defaults ()
        {
            var ev = new Event ();
            Assert.Equal (null, ev.WriteKey);
            Assert.Equal (null, ev.DataSet);
            Assert.Equal (null, ev.ApiHost);
            Assert.Equal (0, ev.SampleRate);
            Assert.Equal (null, ev.Metadata);
        }

        [Fact]
        public void CtorNull ()
        {
            bool excThrown = false;
            try { new Event (null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void CtorNull2 ()
        {
            bool excThrown = false;
            try { new Event (null, new Dictionary<string, Func<object>> ()); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { new Event (new Dictionary<string, object> (), null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void Ctor ()
        {
            Honey.Init ("key1", "HelloHoney", "http://127.0.0.1", 5);

            var ev = new Event ();
            Assert.Equal ("key1", ev.WriteKey);
            Assert.Equal ("HelloHoney", ev.DataSet);
            Assert.Equal ("http://127.0.0.1", ev.ApiHost);
            Assert.Equal (5, ev.SampleRate);

            Honey.Close ();
            Honey.Init ("key2", "HelloComb", "http://127.0.0.1", 15);

            Assert.Equal ("key1", ev.WriteKey);
            Assert.Equal ("HelloHoney", ev.DataSet);
            Assert.Equal ("http://127.0.0.1", ev.ApiHost);
            Assert.Equal (5, ev.SampleRate);
        }

        [Fact]
        public void AddNull ()
        {
            var ev = new Event ();
            var excThrown = false;
            try { ev.Add (null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void AddFieldNull ()
        {
            var ev = new Event ();
            var excThrown = false;
            try { ev.AddField (null, "abc"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { ev.AddField ("abc", null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.False (excThrown);
        }

        [Fact]
        public void Metadata ()
        {
            var ev = new Event ();

            ev.Metadata = 4;
            Assert.Equal (4, ev.Metadata);

            ev.Metadata = null;
            Assert.Equal (null, ev.Metadata);
        }

        [Fact]
        public void SendUninitialized ()
        {
            bool excThrown = false;
            var ev = new Event ();
            try { ev.Send (); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendEmpty ()
        {
            Honey.Init ("key1", "HelloHoney");

            bool excThrown = false;
            var ev = new Event ();
            try { ev.Send (); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendPreSampledUninitialized ()
        {
            bool excThrown = false;
            var ev = new Event ();
            try { ev.SendPreSampled (); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendPreSampledEmpty ()
        {
            Honey.Init ("key1", "HelloHoney");

            bool excThrown = false;
            var ev = new Event ();
            try { ev.SendPreSampled (); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void ToJSON ()
        {
            var ev = new Event ();
            Assert.Equal ("{}", ev.ToJSON ());

            ev.AddField ("counter", 13);
            ev.AddField ("id", "666");
            ev.AddField ("meta", null);
            ev.AddField ("r", 3.1416);
            Assert.Equal ("{\"counter\":13,\"id\":\"666\",\"meta\":null,\"r\":3.1416}", ev.ToJSON ());
        }
    }
}
