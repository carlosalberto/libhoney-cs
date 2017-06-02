using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Honeycomb.Tests
{
    public class HttpTestServer : IDisposable
    {
        HttpListener listener;
        List<string> payloadItems;
        List<NameValueCollection> headers;
        ManualResetEvent manualEvent;

        public HttpTestServer (string url)
        {
            payloadItems = new List<string> ();
            headers = new List<NameValueCollection> ();
            manualEvent = new ManualResetEvent (false);

            listener = new HttpListener ();
            listener.Prefixes.Add (url);
        }

        public List<string> PayloadItems {
            get { return payloadItems; }
        }

        public List<NameValueCollection> Headers {
            get { return headers; }
        }

        public void Clear ()
        {
            payloadItems.Clear ();
            headers.Clear ();
        }

        public void Serve (int times)
        {
            for (int i = 0; i < times; i++)
                ServeOne ();
        }

        public void ServeOne ()
        {
            ServeOne (200, "{}", TimeSpan.Zero);
        }

        public void ServeOne (int statusCode)
        {
            ServeOne (statusCode, "{}", TimeSpan.Zero);
        }

        public void ServeOne (int statusCode, string body)
        {
            ServeOne (statusCode, body, TimeSpan.Zero);
        }

        public void ServeOne (int statusCode, string body, TimeSpan timeout)
        {
            if (!listener.IsListening)
                listener.Start ();

            // Can start receiving requests
            manualEvent.Set ();
            
            var context = listener.GetContext ();
            var request = context.Request;
            var response = context.Response;

            if (timeout > TimeSpan.FromSeconds (0))
                Thread.Sleep (timeout);

            // Save the payload
            var reader = new StreamReader (request.InputStream);
            payloadItems.Add (reader.ReadToEnd ());
            headers.Add (request.Headers);

            // Send back the data specified in the params
            response.StatusCode = statusCode;

            var buff = Encoding.UTF8.GetBytes (body);
            response.ContentLength64 = buff.Length;
            var stream = response.OutputStream;
            stream.Write (buff, 0, buff.Length);
            stream.Close ();
        }

        public void WaitReady ()
        {
            manualEvent.WaitOne ();
        }

        public void Stop ()
        {
            manualEvent.Reset ();

            if (listener.IsListening)
                listener.Stop ();
        }

        public void Dispose ()
        {
            if (listener.IsListening)
                listener.Stop ();
        }
    }
}
