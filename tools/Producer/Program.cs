using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using ConsoleTables;
using Microsoft.Extensions.Configuration;
using Producer;
using Shared;
using Shared.Messages;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
    .Build();

var hubConfiguration = new EventHubsConfiguration(configuration);

var producer = new EventHubProducerClient(hubConfiguration.ConnectionString);

var replyConsumer = new ReplyConsumer(hubConfiguration);

replyConsumer.Start();

while (true)
{
    Console.Clear();

    Console.WriteLine("[1] - Send a new message");
    Console.WriteLine("[2] - List messages and reply messages");
    Console.WriteLine("[3] - Resend failed messages");
    Console.WriteLine("[4] - Resend failed messages and force successful");
    Console.WriteLine();
    Console.WriteLine("[9] - Exit");
    Console.WriteLine();

    _ = int.TryParse(Console.ReadLine().ToLower(), out var task);

    if (task == 9)
        break;

    if (task < 0 || task > 4)
    {
        Console.WriteLine("Invalid Task! Press any key to continue");
        Console.ReadKey();
        continue;
    }

    Console.WriteLine();

    if (task == 1)
    {
        var newMessage = Message.Generate();

        var eventBody = new BinaryData(newMessage);

        var eventData = new EventData(eventBody)
        {
            CorrelationId = newMessage.Id
        };

        Console.WriteLine("Sending ....");
        Console.WriteLine();

        await producer.SendAsync(new EventData[]
        {
            eventData
        });

        MemoryStore.Save(newMessage);

        var table = new ConsoleTable("Id", "Customer", "Simulate Error");
        table.AddRow(newMessage.Id, newMessage.Customer, newMessage.SimulateError);
        table.Write();
    }

    if (task == 2)
    {
        var table = new ConsoleTable("Id", "Customer", "Simulate Error");

        foreach (var message in MemoryStore.GetMessages())
            table.AddRow(message.Id, message.Customer, message.SimulateError);

        table.Write();

        Console.WriteLine();

        var replyTable = new ConsoleTable("MessageId", "Error", "Attempts", "Detail");

        foreach (var reply in MemoryStore.GetReplies())
            replyTable.AddRow(reply.MessageId, reply.Error, reply.Attempts, reply.Detail);

        replyTable.Write();
    }

    if (task == 3 || task == 4)
    {
        var messages = MemoryStore.GetMessages();
        var repliesWithError = MemoryStore.GetReplies().Where(w => w.Error);

        var messagesToSend = new List<EventData>();

        foreach (var reply in repliesWithError)
        {
            var message = messages.Single(s => s.Id == reply.MessageId);

            if (task == 4)
                message.SimulateError = false;

            var eventBody = new BinaryData(message);
            var eventData = new EventData(eventBody)
            {
                CorrelationId = message.Id
            };

            messagesToSend.Add(eventData);

            MemoryStore.Save(message);
        }

        Console.WriteLine("Sending ....");
        Console.WriteLine();

        if (messagesToSend.Any())
            await producer.SendAsync(messagesToSend);
        else
            Console.WriteLine("No messages found");
    }

    Console.WriteLine();
    Console.WriteLine("Press any key to continue");
    Console.ReadKey();
}

replyConsumer.Stop();
