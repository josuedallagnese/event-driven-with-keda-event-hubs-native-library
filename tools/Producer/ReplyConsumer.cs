using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Shared;
using Shared.Messages;

namespace Producer
{
    public class ReplyConsumer
    {
        private readonly EventProcessorClient _consumer;

        public ReplyConsumer(EventHubsConfiguration configuration)
        {
            var storageClient = new BlobContainerClient(
                configuration.BlobStorageConnectionString,
                configuration.ReplyBlobContainerName);

            _consumer = new EventProcessorClient(
                storageClient,
                configuration.ConsumerGroup,
                configuration.ReplyConnectionString);
        }

        public void Start()
        {
            _consumer.ProcessEventAsync += ProcessEventAsync;
            _consumer.ProcessErrorAsync += ProcessErrorAsync;

            _consumer.StartProcessing();
        }

        public void Stop()
        {
            _consumer.StopProcessing();

            _consumer.ProcessEventAsync -= ProcessEventAsync;
            _consumer.ProcessErrorAsync -= ProcessErrorAsync;
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            var partition = args.PartitionId;

            Console.WriteLine($"[Partition - {partition}] - Exception: {args.Exception}");

            return Task.CompletedTask;
        }

        private async Task ProcessEventAsync(ProcessEventArgs args)
        {
            if (!args.HasEvent)
            {
                Console.Write("Heartbeat ...");

                return;
            }

            if (args.Data == null)
            {
                Console.Write("Has no events ...");

                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(args.Data.CorrelationId))
                {
                    await args.UpdateCheckpointAsync();

                    return;
                }

                var replyMessage = args.Data.EventBody.ToObjectFromJson<ReplyMessage>();

                MemoryStore.Save(replyMessage);

                await args.UpdateCheckpointAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                throw;
            }
        }
    }
}
