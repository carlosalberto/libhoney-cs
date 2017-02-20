using System;
using System.Collections.Generic;
using Xunit;

using LibHoney;

[assembly: CollectionBehavior (CollectionBehavior.CollectionPerAssembly)]

namespace LibHoney.Tests
{
    public class HoneyTest : IDisposable
    {
        public void Dispose ()
        {
            Honey.Close ();
        }

        [Fact]
        public void Defaults ()
        {
            Assert.Equal (false, Honey.IsInitialized);
            Assert.Equal (null, Honey.WriteKey);
            Assert.Equal (null, Honey.DataSet);
            Assert.Equal (null, Honey.ApiHost);
            Assert.Equal (0, Honey.SampleRate);
            Assert.Equal (false, Honey.BlockOnSend);
            Assert.Equal (false, Honey.BlockOnResponse);
            //Assert.Equal (true, Honey.Responses != null);
            //Assert.Equal (0, Honey.Responses.Count);
        }

        [Fact]
        public void InitNull ()
        {
            bool excThrown = false;
            try { Honey.Init (null, "abc"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.Init ("abc", null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void InitNull2 ()
        {
            bool excThrown = false;
            try { Honey.Init (null, "abc", "def"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.Init ("abc", null, "def"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.Init ("abc", "def", null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void InitNull3 ()
        {
            bool excThrown = false;
            try { Honey.Init (null, "abc", "def", 1); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.Init ("abc", null, "def", 1); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.Init ("abc", "def", null, 1); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void InitBadRange ()
        {
            // sampleRate
            bool excThrown = false;
            try { Honey.Init ("abc", "def", "ghi", 0); } catch (ArgumentOutOfRangeException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.Init ("abc", "def", "ghi", -1); } catch (ArgumentOutOfRangeException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void Init ()
        {
            Honey.Init ("key1", "HelloHoney");

            Assert.Equal (true, Honey.IsInitialized);
            Assert.Equal ("key1", Honey.WriteKey);
            Assert.Equal ("HelloHoney", Honey.DataSet);
            Assert.Equal (Honey.DefaultApiHost, Honey.ApiHost);
            Assert.Equal (Honey.DefaultSampleRate, Honey.SampleRate);
            Assert.Equal (false, Honey.BlockOnSend);
            Assert.Equal (false, Honey.BlockOnResponse);
        }

        [Fact]
        public void Init2 ()
        {
            Honey.Init ("key1", "HelloHoney", "http://myhost");

            Assert.Equal (true, Honey.IsInitialized);
            Assert.Equal ("key1", Honey.WriteKey);
            Assert.Equal ("HelloHoney", Honey.DataSet);
            Assert.Equal ("http://myhost", Honey.ApiHost);
            Assert.Equal (Honey.DefaultSampleRate, Honey.SampleRate);
            Assert.Equal (false, Honey.BlockOnSend);
            Assert.Equal (false, Honey.BlockOnResponse);
        }

        [Fact]
        public void Init3 ()
        {
            Honey.Init ("key1", "HelloHoney", "http://myhost", 3);

            Assert.Equal (true, Honey.IsInitialized);
            Assert.Equal ("key1", Honey.WriteKey);
            Assert.Equal ("HelloHoney", Honey.DataSet);
            Assert.Equal ("http://myhost", Honey.ApiHost);
            Assert.Equal (3, Honey.SampleRate);
            Assert.Equal (false, Honey.BlockOnSend);
            Assert.Equal (false, Honey.BlockOnResponse);
        }

        [Fact]
        public void Init4 ()
        {
            Honey.Init ("key1", "HelloHoney", "http://myhost", 3, true, true);

            Assert.Equal (true, Honey.IsInitialized);
            Assert.Equal ("key1", Honey.WriteKey);
            Assert.Equal ("HelloHoney", Honey.DataSet);
            Assert.Equal ("http://myhost", Honey.ApiHost);
            Assert.Equal (3, Honey.SampleRate);
            Assert.Equal (true, Honey.BlockOnSend);
            Assert.Equal (true, Honey.BlockOnResponse);
        }

        [Fact]
        public void AddNull ()
        {
            bool excThrown = false;
            try { Honey.Add (null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void AddFieldNull ()
        {
            bool excThrown = false;
            try { Honey.AddField (null, "abc"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.AddField ("abc", null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.False (excThrown);
        }

        [Fact]
        public void AddDynamicFieldNull ()
        {
            bool excThrown = false;
            try { Honey.AddDynamicField (null, () => "abc"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.AddDynamicField ("abc", null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void CloseMultiple ()
        {
            // Without initialization.
            Honey.Close ();

            Honey.Init ("key1", "HelloHoney");
            Honey.Close ();

            // Again
            Honey.Close ();
        }

        [Fact]
        public void SendNowNull ()
        {
            bool excThrown = false;
            try { Honey.SendNow (null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendNowUninitalized ()
        {
            bool excThrown = false;
            try { Honey.SendNow (new Dictionary<string, object> ()); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendEmpty ()
        {
            Honey.Init ("key1", "HelloHoney");

            bool excThrown = false;
            try { Honey.SendNow (new Dictionary<string, object> ()); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }
    }
}
