# BeeInstant .NET SDK Sample

### The library currently targets `netstandard.2.0`

Before usings this sample please make sure you've updated the `beeInstant.config.json` with the creds from beeinstant.com.

```
{
    "publicKey": "yourPublicKey",
    "secretKey": "yourSecretKey",
    "endPoint": "https://{endpoint}.beeinstant.com",
    "flushInSeconds": "5", // flush existing metrics to the server every 5 seconds
    "flushStartDelayInSeconds": "10", // due time before automatically sending first metric to the server
    "isManualFlush": false // if true, you will need to push metrics manually, otherwise metrics will be pushed automatically
}
```

### CLI

- Move to the sample directory and do
```
dotnet restore
dotnet run
```

It will now start pushing metrics to your BeeInstant instance.
