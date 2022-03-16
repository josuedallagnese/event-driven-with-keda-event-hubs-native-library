using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using KedaWorker.Requests;
using MediatR;
using Shared;

namespace KedaWorker
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly EventHubsConfiguration _hubConfiguration;
        private readonly EventProcessorClient _eventProcessorClient;

        public Worker(
            ILogger<Worker> logger,
            IServiceScopeFactory scopeFactory,
            EventHubsConfiguration hubConfiguration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _hubConfiguration = hubConfiguration;

            var storageClient = new BlobContainerClient(
                _hubConfiguration.BlobStorageConnectionString,
                _hubConfiguration.BlobContainerName);

            _eventProcessorClient = new EventProcessorClient(
                storageClient,
                _hubConfiguration.ConsumerGroup,
                _hubConfiguration.ConnectionString);

            _eventProcessorClient.ProcessEventAsync += ProcessEventAsync;
            _eventProcessorClient.ProcessErrorAsync += ProcessErrorAsync;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start worker execution. Reading hub messages for earliest position.");

            _eventProcessorClient.StartProcessing();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping worker execution ...");

            _eventProcessorClient.StopProcessing();

            _eventProcessorClient.ProcessEventAsync -= ProcessEventAsync;
            _eventProcessorClient.ProcessErrorAsync -= ProcessErrorAsync;

            _logger.LogInformation("Worker execution finished ...");

            return Task.CompletedTask;
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            var partition = args.PartitionId;

            _logger.LogError($"[Partition - {partition}] - Exception: {args.Exception}");

            return Task.CompletedTask;
        }

        private async Task ProcessEventAsync(ProcessEventArgs args)
        {
            if (!args.HasEvent)
            {
                _logger.LogInformation("Heartbeat ...");

                return;
            }

            if (args.Data == null)
            {
                _logger.LogInformation("Has no events ...");

                return;
            }

            if (string.IsNullOrWhiteSpace(args.Data.CorrelationId))
            {
                _logger.LogInformation("Invalid message in hub, just ignore and not save. Update checkpoint...");

                await args.UpdateCheckpointAsync();

                return;
            }

            using var scope = _scopeFactory.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new ProcessMessageRequest(args));
        }
    }
}
