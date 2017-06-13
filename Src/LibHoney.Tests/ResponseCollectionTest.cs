using System;
using System.Collections.Concurrent;
using Xunit;

namespace Honeycomb.Tests
{
    public class ResponseCollectionTest
    {
        [Fact]
        public void Wrap ()
        {
            var responses = new BlockingCollection<Response> ();
            responses.Add (new Response ());
            responses.Add (null);

            var coll = new ResponseCollection (responses);
            Assert.Equal (coll.Responses, responses);
            Assert.Equal (coll.Count, 2);
            Assert.False (coll.IsAddingCompleted);
            Assert.False (coll.IsCompleted);

            responses.CompleteAdding ();
            Assert.True (coll.IsAddingCompleted);
            Assert.False (coll.IsCompleted);

            responses.Take ();
            responses.Take ();
            Assert.True (coll.IsAddingCompleted);
            Assert.True (coll.IsCompleted);
        }

        [Fact]
        public void Consume ()
        {
            var responses = new BlockingCollection<Response> ();
            responses.Add (null);
            responses.Add (null);

            var coll = new ResponseCollection (responses);

            Response response1, response2;
            response1 = coll.Take ();
            coll.TryTake (out response2);

            Assert.Null (response1);
            Assert.Null (response2);
            Assert.False (coll.IsAddingCompleted);
            Assert.False (coll.IsCompleted);
            Assert.Equal (coll.Count, 0);
        }

        [Fact]
        public void Enumerate ()
        {
            var responses = new BlockingCollection<Response> ();
            for (int i = 0; i < 5; i++)
                responses.Add (null);

            var coll = new ResponseCollection (responses);

            int count = 0;
            foreach (var response in coll) {
                count++;
                Assert.Null (response);
            }

            Assert.Equal (count, 5);
            Assert.Equal (coll.Count, 5);
        }
    }
}
