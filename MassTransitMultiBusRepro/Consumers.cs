using MassTransit;

namespace MassTransitMultiBusRepro;

public record Message1(int Id = 1);

public class Consumer1 : IConsumer<Message1>
{
    public Task Consume(ConsumeContext<Message1> context)
    {
        Console.WriteLine($"Hello from message {context.Message.Id}");
        return Task.CompletedTask;
    }
}

public record Message2(int Id = 2);

public class Consumer2 : IConsumer<Message2>
{
    public Task Consume(ConsumeContext<Message2> context)
    {
        Console.WriteLine($"Hello from message {context.Message.Id}");
        return Task.CompletedTask;
    }
}
