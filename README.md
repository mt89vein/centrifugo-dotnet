# Centrifuge client for dotnet

This client can connect to [Centrifugo](https://github.com/centrifugal/centrifugo)
using websockets.

Work in progress now.

Look at sample project for details.

## Playground

Clone, start docker-compose in root folder.

on first window open home page,
on the other hit /new?text=my-text

## Usage api surface

```csharp

var ws = new WebsocketClient(new Uri("ws://localhost:8000/connection/websocket?format=protobuf"));

// create client
var centrifugo = new CentrifugoClient(ws);

// setup event handlers
centrifugo.OnConnect(e => logger.LogInformation(e.Client.ToString()));
centrifugo.OnError(e => logger.LogInformation(e.ToString()));
// OnRefresh, OnDisconnect not implemented yet.

// create new subscription
var subscription = centrifugo.CreateNewSubscription("test-channel");

// setup for event handlers
// both sync and async supported.
subscription.OnSubscribe(e => logger.LogInformation("subscribed"));
subscription.OnUnsubscribe(e => logger.LogInformation("unsubscribed"));
subscription.OnPublish(e => logger.LogInformation(e.ToString()));
// also OnJoin, OnLeave etc events

// generate JWT token with correct secret key.
// It must be identical with 'token_hmac_secret_key' in centrifugo configuration.
var token = await tokenProvider
        .GenerateTokenAsync("some_client_identifier", "Hi, my name is Jack");

// set jwt-token
centrifugo.SetToken(token);

// open ws connection and connection handshake to centrifugo.
await centrifugo.ConnectAsync();

// send subscribe message with configured above subscription.
await centrifugo.SubscribeAsync(subscription);

// prepare message to publish
var msg = new TestMessage
{
    Msg = text ?? "Hello",
    Id = Guid.NewGuid()
};

// convert it to json, and then get bytes
var json = JsonConvert.SerializeObject(msg);
var payload = Encoding.UTF8.GetBytes(json);

// publish to channel
await centrifugo.PublishAsync(subscription.Channel, payload);

// hint: under the hood used Google protocol buffers, but 
// you can pass any payload.

```

## Contribute

Feel free for creation issues, or PR :)

## License

Copyright © 2020 Shamil Sultanov

The MIT licence.