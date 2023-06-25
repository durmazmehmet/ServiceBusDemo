using Microsoft.Azure.ServiceBus;
using SBShared.Models;
using System.Text;
using System.Text.Json;

class Program
{
    const string connectionString = "Endpoint=sb://sb-bo-az-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=dyfzhU1RpZ+6TI2nce9ZsA5Ggytfq3m9b+ASbJbEA/s=";
    const string queueName = "sbq-bo-az-partpurchase";
    static IQueueClient queueClient;
    static async Task Main(string[] args)
    {
        queueClient = new QueueClient(connectionString, queueName);

        var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
        {
            MaxConcurrentCalls = 1,
            AutoComplete = false
        };

        queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

        Console.ReadLine();

        await queueClient.CloseAsync();
    }

    private static async Task ProcessMessagesAsync(Message message, CancellationToken token)
    {
        var jsonString = Encoding.UTF8.GetString(message.Body);
        PersonModel person = JsonSerializer.Deserialize<PersonModel>(jsonString) ?? new PersonModel { FirstName = "Jane", LastName = "Doe" };
        Console.WriteLine($"Person received: {person.FirstName} {person.LastName}");
        await queueClient.CompleteAsync(message.SystemProperties.LockToken);
    }

    private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs args)
    {
        Console.WriteLine($"Message handler exception: { args.Exception}");
        return Task.CompletedTask;
    }
}