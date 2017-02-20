# libhoney

.Net library for sending events to [Honeycomb](https://honeycomb.io).

## Example

```cs
using LibHoney;

Honey.Init ("YOUR_WRITE_KEY", "honeycomb-cs-example");
Honey.SendNow (new Dictionary<string, object> () {
    ["message"] = "Test Honeycomb event",
    ["count"] = 7
});
```

