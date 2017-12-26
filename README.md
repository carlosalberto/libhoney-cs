# libhoney [![Build Status](https://travis-ci.org/carlosalberto/libhoney-cs.svg?branch=master)](https://travis-ci.org/carlosalberto/libhoney-cs)

.Net library for sending events to [Honeycomb](https://honeycomb.io).

## Example

```cs
using Honeycomb;

var libHoney = new LibHoney ("YOUR_WRITE_KEY", "honeycomb-cs-example");
libHhoney.SendNow (new Dictionary<string, object> () {
    ["message"] = "Test Honeycomb event",
    ["count"] = 7
});
```

