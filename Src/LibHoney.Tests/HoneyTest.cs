﻿using System;
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
            Honey.Close (true);
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
            Assert.Equal (true, Honey.Responses != null);
            Assert.Equal (0, Honey.Responses.Count);
        }

        [Fact]
        public void DefaultsAfterClose ()
        {
            Honey.Init ("key1", "HelloHoney", "http://myhost", 3, 6, true, true);
            Honey.Close ();

            Assert.Equal (false, Honey.IsInitialized);
            Assert.Equal (null, Honey.WriteKey);
            Assert.Equal (null, Honey.DataSet);
            Assert.Equal (null, Honey.ApiHost);
            Assert.Equal (0, Honey.SampleRate);
            Assert.Equal (false, Honey.BlockOnSend);
            Assert.Equal (false, Honey.BlockOnResponse);
            Assert.Equal (true, Honey.Responses != null);
            Assert.Equal (1, Honey.Responses.Count); // prev transmission, null signaling termination
            Assert.Equal (null, Honey.Responses.Take ());
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
            try { Honey.Init (null, "abc", "def", 1, 1); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.Init ("abc", null, "def", 1, 1); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.Init ("abc", "def", null, 1, 1); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void InitBadRange ()
        {
            // sampleRate
            bool excThrown = false;
            try { Honey.Init ("abc", "def", "ghi", 0, 1); } catch (ArgumentOutOfRangeException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.Init ("abc", "def", "ghi", -1, 1); } catch (ArgumentOutOfRangeException) { excThrown = true; }
            Assert.True (excThrown);

            // maxConcurrentBatches
            excThrown = false;
            try { Honey.Init ("abc", "def", "ghi", 1, 0); } catch (ArgumentOutOfRangeException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { Honey.Init ("abc", "def", "ghi", 1, -1); } catch (ArgumentOutOfRangeException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void InitBadUri ()
        {
            // Bad scheme
            bool excThrown = false;
            try { Honey.Init ("abc", "def", "ftp://myhost"); } catch (ArgumentException) { excThrown = true; }
            Assert.True (excThrown);

            // Incomplete
            excThrown = false;
            try { Honey.Init ("abc", "def", "http://"); } catch (ArgumentException) { excThrown = true; }
            Assert.True (excThrown);

            // Incomplete
            excThrown = false;
            try { Honey.Init ("abc", "def", "127.0.0.1"); } catch (ArgumentException) { excThrown = true; }
            Assert.True (excThrown);

            // Incorrect
            excThrown = false;
            try { Honey.Init ("abc", "def", "http://127.0.0.1:::"); } catch (ArgumentException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void InitWithoutClosing ()
        {
            bool excThrown = false;
            Honey.Init ("abc", "def");
            try { Honey.Init ("def", "abc"); } catch (InvalidOperationException) { excThrown = true; }
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
            Honey.Init ("key1", "HelloHoney", "http://myhost", 3, 6);

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
            Honey.Init ("key1", "HelloHoney", "http://myhost", 3, 6, true, true);

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

            // Again, a few times.
            Honey.Close ();
            Honey.Close (true);
            Honey.Close (false);
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

        [Fact]
        public void InternalStateDefault ()
        {
            Assert.Equal (true, Honey.Fields.IsEmpty);
            Assert.Equal (0, Honey.Fields.Fields.Count);
            Assert.Equal (0, Honey.Fields.DynamicFields.Count);
            Assert.Equal (true, Honey.Transmission == null);
            Assert.Equal (true, Honey.Responses != null);
            Assert.Equal (Honey.Responses.BoundedCapacity, 1);
        }

        [Fact]
        public void InternalStateAfterInit ()
        {
            Honey.Init ("key1", "HelloHoney");
            Honey.AddField ("counter", 13);
            Honey.AddField ("value", DateTime.Now);
            Honey.AddDynamicField ("dynamic_value", () => DateTime.Now);

            Assert.Equal (false, Honey.Fields.IsEmpty);
            Assert.Equal (2, Honey.Fields.Fields.Count);
            Assert.Equal (1, Honey.Fields.DynamicFields.Count);
            Assert.Equal (true, Honey.Transmission != null);
            Assert.Equal (true, Honey.Responses != null);
            Assert.Equal (Honey.Transmission.Responses, Honey.Responses);
            Assert.Equal (true, Honey.Responses.BoundedCapacity > 1);
        }

        [Fact]
        public void InternalStateAfterClose ()
        {
            Honey.Init ("key1", "HelloHoney");
            Honey.AddField ("counter", 13);
            Honey.AddField ("value", DateTime.Now);
            Honey.AddDynamicField ("dynamic_value", () => DateTime.Now);

            // Close, but keep the global fields/responses
            Honey.Close ();
            Assert.Equal (false, Honey.Fields.IsEmpty);
            Assert.Equal (2, Honey.Fields.Fields.Count);
            Assert.Equal (1, Honey.Fields.DynamicFields.Count);
            Assert.Equal (true, Honey.Transmission == null);
            Assert.Equal (true, Honey.Responses != null);
            Assert.Equal (true, Honey.Responses.BoundedCapacity > 1); // prev transmission

            // Close, discarding the global fields/responses, if any
            Honey.Close (true);
            Assert.Equal (true, Honey.Fields.IsEmpty);
            Assert.Equal (0, Honey.Fields.Fields.Count);
            Assert.Equal (0, Honey.Fields.DynamicFields.Count);
            Assert.Equal (true, Honey.Transmission == null);
            Assert.Equal (true, Honey.Responses != null);
            Assert.Equal (true, Honey.Responses.BoundedCapacity == 1); // default
        }
    }
}
