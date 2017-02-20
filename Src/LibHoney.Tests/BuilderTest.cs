using System;
using System.Collections.Generic;
using Xunit;

using LibHoney;

namespace LibHoney.Tests
{
    public class BuilderTest : IDisposable
    {
        public void Dispose ()
        {
            Honey.Close ();
        }

        [Fact]
        public void Defaults ()
        {
            var b = new Builder ();
            Assert.Equal (null, b.WriteKey);
            Assert.Equal (null, b.DataSet);
            Assert.Equal (0, b.SampleRate);
        }

        [Fact]
        public void CtorNull ()
        {
            bool excThrown = false;
            try { new Builder (null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.Equal (true, excThrown);
        }

        [Fact]
        public void CtorNull2 ()
        {
            bool excThrown = false;
            try { new Builder (new Dictionary<string, object> (), null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.Equal (true, excThrown);

            excThrown = false;
            try { new Builder (null, new Dictionary<string, Func<object>> ()); } catch (ArgumentNullException) { excThrown = true; }
            Assert.Equal (true, excThrown);
        }

        [Fact]
        public void Ctor ()
        {
            Honey.Init ("key1", "HelloHoney", "http://", 5);

            var b = new Builder ();
            Assert.Equal ("key1", b.WriteKey);
            Assert.Equal ("HelloHoney", b.DataSet);
            Assert.Equal (5, b.SampleRate);

            Honey.Init ("key2", "HelloComb", "http://", 15);

            Assert.Equal ("key1", b.WriteKey);
            Assert.Equal ("HelloHoney", b.DataSet);
            Assert.Equal (5, b.SampleRate);
        }

        [Fact]
        public void AddNull ()
        {
            bool excThrown = false;
            var b = new Builder ();
            try { b.Add (null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.Equal (true, excThrown);
        }

        [Fact]
        public void AddFieldNull ()
        {
            bool excThrown = false;
            var b = new Builder ();
            try { b.AddField (null, "abc"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.Equal (true, excThrown);

            excThrown = false;
            try { b.AddField ("abc", null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.Equal (false, excThrown);
        }

        [Fact]
        public void AddDynamicFieldNull ()
        {
            bool excThrown = false;
            var b = new Builder ();
            try { b.AddDynamicField (null, () => "abc"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.Equal (true, excThrown);

            excThrown = false;
            try { b.AddDynamicField ("abc", null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.Equal (true, excThrown);
        }

        [Fact]
        public void Clone ()
        {
            Honey.Init ("key1", "HelloHoney", "http://", 5);

            var b = new Builder ();
            Assert.Equal ("key1", b.WriteKey);
            Assert.Equal ("HelloHoney", b.DataSet);
            Assert.Equal (5, b.SampleRate);

            Honey.Init ("key2", "HelloComb", "http://", 15);

            var clone = b.Clone ();
            Assert.Equal (true, clone != null);
            Assert.Equal ("key1", clone.WriteKey);
            Assert.Equal ("HelloHoney", clone.DataSet);
            Assert.Equal (5, clone.SampleRate);
        }

        [Fact]
        public void NewEvent ()
        {
            Honey.Init ("key1", "HelloHoney", "http://", 5);

            var b = new Builder ();
            var ev = b.NewEvent ();
            Assert.Equal (true, ev != null);
            Assert.Equal ("key1", ev.WriteKey);
            Assert.Equal ("HelloHoney", ev.DataSet);
            Assert.Equal ("http://", ev.ApiHost);
            Assert.Equal (5, ev.SampleRate);

            Honey.Init ("key2", "HelloComb", "http://localhost", 15);

            Assert.Equal ("key1", ev.WriteKey);
            Assert.Equal ("HelloHoney", ev.DataSet);
            Assert.Equal ("http://", ev.ApiHost);
            Assert.Equal (5, ev.SampleRate);
        }

        [Fact]
        public void SendNowNull ()
        {
            var b = new Builder ();
            bool excThrown = false;
            try { b.SendNow (null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendNowUninitialized ()
        {
            var b = new Builder ();
            bool excThrown = false;
            try { b.SendNow (); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { b.SendNow (new Dictionary<string, object> ()); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendNowEmpty ()
        {
            Honey.Init ("key1", "HelloHoney");

            var b = new Builder ();
            bool excThrown = false;
            try { b.SendNow (new Dictionary<string, object> ()); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }
    }
}
