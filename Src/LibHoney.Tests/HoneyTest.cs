using System;
using System.Collections.Generic;
using Xunit;

[assembly: CollectionBehavior (CollectionBehavior.CollectionPerAssembly)]

namespace Honeycomb.Tests
{
    public class HoneyTest : IDisposable
    {
        public LibHoney LibHoney {
            get;
            set;
        }

        public void Dispose ()
        {
            if (LibHoney != null)
                LibHoney.Dispose ();
        }

        [Fact]
        public void InitNull ()
        {
            bool excThrown = false;
            try { new LibHoney (null, "abc"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { new LibHoney ("abc", null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void InitNull2 ()
        {
            bool excThrown = false;
            try { new LibHoney (null, "abc", "def"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { new LibHoney ("abc", null, "def"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { new LibHoney ("abc", "def", null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void InitNull3 ()
        {
            bool excThrown = false;
            try { new LibHoney (null, "abc", "def", 1, 1); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { new LibHoney ("abc", null, "def", 1, 1); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { new LibHoney ("abc", "def", null, 1, 1); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void InitBadRange ()
        {
            // sampleRate
            bool excThrown = false;
            try { new LibHoney ("abc", "def", "ghi", 0, 1); } catch (ArgumentOutOfRangeException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { new LibHoney ("abc", "def", "ghi", -1, 1); } catch (ArgumentOutOfRangeException) { excThrown = true; }
            Assert.True (excThrown);

            // maxConcurrentBatches
            excThrown = false;
            try { new LibHoney ("abc", "def", 0); } catch (ArgumentOutOfRangeException) { excThrown = true; }

            excThrown = false;
            try { new LibHoney ("abc", "def", -1); } catch (ArgumentOutOfRangeException) { excThrown = true; }

            excThrown = false;
            try { new LibHoney ("abc", "def", "ghi", 1, 0); } catch (ArgumentOutOfRangeException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { new LibHoney ("abc", "def", "ghi", 1, -1); } catch (ArgumentOutOfRangeException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void InitBadUri ()
        {
            // Bad scheme
            bool excThrown = false;
            try { new LibHoney ("abc", "def", "ftp://myhost"); } catch (ArgumentException) { excThrown = true; }
            Assert.True (excThrown);

            // Incomplete
            excThrown = false;
            try { new LibHoney ("abc", "def", "http://"); } catch (ArgumentException) { excThrown = true; }
            Assert.True (excThrown);

            // Incomplete
            excThrown = false;
            try { new LibHoney ("abc", "def", "127.0.0.1"); } catch (ArgumentException) { excThrown = true; }
            Assert.True (excThrown);

            // Incorrect
            excThrown = false;
            try { new LibHoney ("abc", "def", "http://127.0.0.1:::"); } catch (ArgumentException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void Ctor ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney");

            Assert.Equal ("key1", libHoney.WriteKey);
            Assert.Equal ("HelloHoney", libHoney.DataSet);
            Assert.Equal (LibHoney.DefaultApiHost, libHoney.ApiHost);
            Assert.Equal (LibHoney.DefaultSampleRate, libHoney.SampleRate);
            Assert.Equal (false, libHoney.BlockOnSend);
            Assert.Equal (false, libHoney.BlockOnResponse);
        }

        [Fact]
        public void Ctor2 ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", "http://myhost");

            Assert.Equal ("key1", libHoney.WriteKey);
            Assert.Equal ("HelloHoney", libHoney.DataSet);
            Assert.Equal ("http://myhost", libHoney.ApiHost);
            Assert.Equal (LibHoney.DefaultSampleRate, libHoney.SampleRate);
            Assert.Equal (false, libHoney.BlockOnSend);
            Assert.Equal (false, libHoney.BlockOnResponse);
        }

        [Fact]
        public void Ctor3 ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", "http://myhost", 3, 6);

            Assert.Equal ("key1", libHoney.WriteKey);
            Assert.Equal ("HelloHoney", libHoney.DataSet);
            Assert.Equal ("http://myhost", libHoney.ApiHost);
            Assert.Equal (3, libHoney.SampleRate);
            Assert.Equal (false, libHoney.BlockOnSend);
            Assert.Equal (false, libHoney.BlockOnResponse);
        }

        [Fact]
        public void Ctor4 ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", "http://myhost", 3, 6, true, true);

            Assert.Equal ("key1", libHoney.WriteKey);
            Assert.Equal ("HelloHoney", libHoney.DataSet);
            Assert.Equal ("http://myhost", libHoney.ApiHost);
            Assert.Equal (3, libHoney.SampleRate);
            Assert.Equal (true, libHoney.BlockOnSend);
            Assert.Equal (true, libHoney.BlockOnResponse);
        }

        [Fact]
        public void Responses ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", 1);

            Assert.Equal (true, libHoney.Responses != null);
            Assert.Equal (false, libHoney.Responses.IsAddingCompleted);
            Assert.Equal (false, libHoney.Responses.IsCompleted);
        }

        [Fact]
        public void AddNull ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", 1);

            bool excThrown = false;
            try { libHoney.Add (null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void AddFieldNull ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", 1);

            bool excThrown = false;
            try { libHoney.AddField (null, "abc"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { libHoney.AddField ("abc", null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.False (excThrown);
        }

        [Fact]
        public void AddDynamicFieldNull ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", 1);

            bool excThrown = false;
            try { libHoney.AddDynamicField (null, () => "abc"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { libHoney.AddDynamicField ("abc", null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void AfterDispose ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", "http://myhost", 3, 6, true, true);
            libHoney.Dispose ();

            Assert.Equal (true, libHoney.Responses != null);
            Assert.Equal (0, libHoney.Responses.Count);
            Assert.Equal (true, libHoney.Responses.IsAddingCompleted);
            Assert.Equal (true, libHoney.Responses.IsCompleted);
        }

        [Fact]
        public void DisposeMultiple ()
        {
            var libHoney = new LibHoney ("key1", "HelloHoney", 1);
            libHoney.Dispose ();

            // Again, a few times.
            libHoney.Dispose ();
            libHoney.Dispose ();
        }

        [Fact]
        public void SendNowNull ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", 1);

            bool excThrown = false;
            try { libHoney.SendNow (null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { libHoney.SendNow (null, "value"); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendNowDisposed ()
        {
            var libHoney = new LibHoney ("key1", "HelloHoney", 1);
            libHoney.Dispose ();

            bool excThrown = false;
            try { libHoney.SendNow (new Dictionary<string, object> ()); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { libHoney.SendNow ("name", "value"); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendEmpty ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", 1);
            
            bool excThrown = false;
            try { libHoney.SendNow (new Dictionary<string, object> ()); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void InternalStateAfterInit ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", 3);
            libHoney.AddField ("counter", 13);
            libHoney.AddField ("value", DateTime.Now);
            libHoney.AddDynamicField ("dynamic_value", () => DateTime.Now);

            Assert.Equal (false, libHoney.Fields.IsEmpty);
            Assert.Equal (2, libHoney.Fields.Fields.Count);
            Assert.Equal (1, libHoney.Fields.DynamicFields.Count);
            Assert.Equal (true, libHoney.Transmission != null);
            Assert.Equal (3, libHoney.Transmission.MaxConcurrentBatches);
            Assert.Equal (true, libHoney.Responses != null);
            Assert.Equal (libHoney.Transmission.Responses, libHoney.Responses);

            Assert.Equal (true, libHoney.Transmission.Responses.BoundedCapacity > 1);
            Assert.Equal (false, libHoney.Transmission.Responses.IsAddingCompleted);
            Assert.Equal (false, libHoney.Transmission.Responses.IsCompleted);
        }

        [Fact]
        public void InternalStateAfterDispose ()
        {
            var libHoney = LibHoney = new LibHoney ("key1", "HelloHoney", 1);
            libHoney.Dispose ();

            Assert.Equal (true, libHoney.IsDisposed);
            Assert.Null (libHoney.Transmission);
            Assert.NotNull (libHoney.Responses);
        }
    }
}
