# Oy
Oy is a simple, extensible message bus for .NET

Nuget Package: [LogicAndTrick.Oy](https://www.nuget.org/packages/LogicAndTrick.Oy)
```
Install-Package LogicAndTrick.Oy
```

## What's it do?
Oy is a simple abstraction over a messaging system. Messaging is pretty simple, but using an interface should help you abstract away any possible issues that might arise from rolling one yourself. Oy is extensible, so you can use other communication methods if you like. A Redis sample is provided in the repo.

## How do I use it?
First, subscribe to a channel and provide a callback:

```csharp
Oy.Subscribe<String>("Channel 1", async (x, t) => Console.WriteLine(x.Number + " - " + x.Message));
```

Then, publish to that channel:

```csharp
Oy.Publish("Channel 1", "This is a message");
````

Pretty easy, right?

## Custom implementations

To use your own messaging protocol, implement `IMessageBus`:

```csharp
public class CustomMessageBus : IMessageBus
{
    // ... See Redis example for full sample code
}
```

And then register your implementation on application startup:

```csharp
Oy.Use(new CustomMessageBus());
```

If you want to use multiple message protocols, you can create a composite bus, or you can simply instantiate and use the base message bus class without using the static `Oy` singleton class.

## Why did you call it "Oy"?
Because boring names are boring.

## License
MIT
