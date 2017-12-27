# libhoney

[![NuGet version (LibHoney)](https://img.shields.io/nuget/v/LibHoney.svg?style=flat-square)](https://www.nuget.org/packages/LibHoney/)
[![Build Status](https://travis-ci.org/carlosalberto/libhoney-cs.svg?branch=master)](https://travis-ci.org/carlosalberto/libhoney-cs)

.Net library for sending events to [Honeycomb](https://honeycomb.io).

## Installation:

Using Package Manager:

```
PM> Install-Package LibHoney -Version <current-version>
```

Using .NET CLI:

```
> dotnet add package LibHoney --version <current-version>
```

## Example

Honeycomb can calculate all sorts of statistics, so send the values you care about and let us crunch the averages, percentiles, lower/upper bounds, cardinality -- whatever you want -- for you.

```cs
using Honeycomb;

// Create LibHoney with the desired configuration.
var libHoney = new LibHoney ("YOUR_WRITE_KEY", "honeycomb-cs-example");

libHhoney.SendNow (new Dictionary<string, object> () {
    ["duration_ms"] = 153.12,
    ["method"] = "get",
    ["hostname"] = "appserver15",
    ["payload_length"] = 27,
});
```

See the [`examples` directory](examples/) for sample code demonstrating how to use events,
builders, fields, and dynamic fields.

## Contributions

Features, bug fixes and other changes to libhoney are gladly accepted. Please
open issues or a pull request with your change. Remember to add your name to the
CONTRIBUTORS file!

All contributions will be released under the Apache License 2.0.

