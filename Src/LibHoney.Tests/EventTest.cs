using System;
using System.Collections.Generic;
using Xunit;

namespace LibHoney.Tests
{
    public class EventTest : IClassFixture<LibHoneyFixture>, IDisposable
    {
        LibHoneyFixture fixture;

        public EventTest (LibHoneyFixture fixture)
        {
            this.fixture = fixture;
        }

        public void Dispose ()
        {
            fixture.LibHoney.Reset ();
        }

        Honey GetLibHoney ()
        {
            return fixture.LibHoney;
        }

        [Fact]
        public void CtorNull ()
        {
            bool excThrown = false;
            try { new Event ((Honey) null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void CtorNull2 ()
        {
            var honey = GetLibHoney ();

            bool excThrown = false;
            try { new Event (null, new Dictionary<string, object> ()); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { new Event (honey, null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void CtorNull3 ()
        {
            var honey = GetLibHoney ();

            bool excThrown = false;
            try { new Event (null, new Dictionary<string, object> (), new Dictionary<string, Func<object>> ()); }
            catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { new Event (honey, null, new Dictionary<string, Func<object>> ()); }
            catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);

            excThrown = false;
            try { new Event (honey, new Dictionary<string, object> (), null); }
            catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void Ctor ()
        {
            var honey = GetLibHoney ();

            var ev = new Event (honey);
            Assert.Equal (honey.WriteKey, ev.WriteKey);
            Assert.Equal (honey.DataSet, ev.DataSet);
            Assert.Equal (honey.ApiHost, ev.ApiHost);
            Assert.Equal (honey.SampleRate, ev.SampleRate);
        }

        [Fact]
        public void AddNull ()
        {
            var ev = new Event (GetLibHoney ());
            var excThrown = false;
            try { ev.Add (null); } catch (ArgumentNullException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void AddFieldNull ()
        {
            var ev = new Event (GetLibHoney ());
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
            var ev = new Event (GetLibHoney ());

            ev.Metadata = 4;
            Assert.Equal (4, ev.Metadata);

            ev.Metadata = null;
            Assert.Equal (null, ev.Metadata);
        }

        [Fact]
        public void SendDisposed ()
        {
            var honey = GetLibHoney ();
            var ev = new Event (honey);
            honey.Dispose ();

            bool excThrown = false;
            try { ev.Send (); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendEmpty ()
        {
            bool excThrown = false;
            var ev = new Event (GetLibHoney ());
            try { ev.Send (); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendPreSampledDisposed ()
        {
            var honey = GetLibHoney ();
            var ev = new Event (honey);
            honey.Dispose ();

            bool excThrown = false;
            try { ev.SendPreSampled (); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void SendPreSampledEmpty ()
        {
            bool excThrown = false;
            var ev = new Event (GetLibHoney ());
            try { ev.SendPreSampled (); } catch (SendException) { excThrown = true; }
            Assert.True (excThrown);
        }

        [Fact]
        public void ToJSONBasic ()
        {
            var honey = GetLibHoney ();

            var ev = new Event (honey);
            Assert.Equal ("{}", ev.ToJSON ());

            ev.AddField ("counter", 13);
            ev.AddField ("id", "666");
            ev.AddField ("meta", null);
            ev.AddField ("r", 3.1416);
            Assert.Equal ("{\"counter\":13,\"id\":\"666\",\"meta\":null,\"r\":3.1416}", ev.ToJSON ());

            ev = new Event (
                honey,
                new Dictionary<string, object> () {
                ["counter"] = 13,
                ["values"] = new [] { 1, 7, 11 }
            }, new Dictionary<string, Func<object>> () {
                ["counter2"] = () => 14,
                ["values2"] = () => new [] { 2, 8, 12 }
            });
            Assert.Equal ("{\"counter\":13,\"values\":\"[1,7,11]\",\"counter2\":14,\"values2\":\"[2,8,12]\"}", ev.ToJSON ());
        }

        [Fact]
        public void ToJSONObjects ()
        {
            var honey = GetLibHoney ();
            Event ev = null;

            // generic object
            ev = new Event (honey);
            ev.AddField ("obj", new object ());
            Assert.Equal ("{\"obj\":\"{}\"}", ev.ToJSON ());

            // simple object
            ev = new Event (honey);
            ev.AddField ("obj2", new Hacker () { Name = "Anders" });
            Assert.Equal ("{\"obj2\":\"{\\\"Name\\\":\\\"Anders\\\"}\"}", ev.ToJSON ());

            // array
            ev = new Event (honey);
            ev.AddField ("arr", new [] { 1, 7, 11 });
            Assert.Equal ("{\"arr\":\"[1,7,11]\"}", ev.ToJSON ());

            // list
            ev = new Event (honey);
            ev.AddField ("list", new List<int> (new [] { 1, 7, 11 }));
            Assert.Equal ("{\"list\":\"[1,7,11]\"}", ev.ToJSON ());

            // Dictionary
            ev = new Event (honey);
            ev.AddField ("dict", new Dictionary<string, int> () {
                ["count"] = 13,
                ["max"] = 666
            });
            Assert.Equal ("{\"dict\":\"{\\\"count\\\":13,\\\"max\\\":666}\"}", ev.ToJSON ());

            // DateTime
            ev = new Event (honey);
            ev.AddField ("datetime", new DateTime (2000, 1, 1, 23, 59, 59));
            Assert.Equal ("{\"datetime\":\"2000-01-01T23:59:59\"}", ev.ToJSON ());

            // TimeSpan
            ev = new Event (honey);
            ev.AddField ("timespan", new TimeSpan (7, 11, 13, 14, 877));
            Assert.Equal ("{\"timespan\":\"7.11:13:14.8770000\"}", ev.ToJSON ());

            // Decimal
            ev = new Event (honey);
            ev.AddField ("decimal", new Decimal (3.1416));
            Assert.Equal ("{\"decimal\":3.1416}", ev.ToJSON ());

            // Enum
            ev = new Event (honey);
            ev.AddField ("enum", HackerType.Backend);
            Assert.Equal ("{\"enum\":0}", ev.ToJSON ());
        }

        class Hacker
        {
            public string Name { get; set; }
            internal int Id { get; set; }
        }

        enum HackerType
        {
            Backend,
            Frontend
        }

        [Fact]
        public void InternalState ()
        {
            var honey = GetLibHoney ();

            var ev = new Event (honey);
            Assert.Equal (true, ev.Fields.IsEmpty);
            Assert.Equal (0, ev.Fields.Fields.Count);
            Assert.Equal (0, ev.Fields.DynamicFields.Count);

            ev = new Event (
                honey,
                new Dictionary<string, object> () {
                    ["counter"] = 13
                },
                new Dictionary<string, Func<object>> () {
                    ["dynamic_value"] = () => 17
                }
            );
            Assert.Equal (false, ev.Fields.IsEmpty);
            Assert.Equal (2, ev.Fields.Fields.Count);
            Assert.Equal (1, ev.Fields.DynamicFields.Count);

            // Event with global fields included
            honey.AddField ("global_counter", 14);
            honey.AddDynamicField ("global_dynamic_value", () => 18);

            ev = new Event (honey);
            Assert.Equal (false, ev.Fields.IsEmpty);
            Assert.Equal (2, ev.Fields.Fields.Count);
            Assert.Equal (1, ev.Fields.DynamicFields.Count);

            ev.AddField ("extra_counter", 33);
            Assert.Equal (false, ev.Fields.IsEmpty);
            Assert.Equal (3, ev.Fields.Fields.Count);
            Assert.Equal (1, ev.Fields.DynamicFields.Count);
        }
    }
}
