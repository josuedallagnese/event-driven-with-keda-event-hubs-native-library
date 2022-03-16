using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using KedaWorker.Requests;
using MediatR;
using Shared;
using Shared.Messages;

namespace KedaWorker.Handlers
{
    public class ProcessMessageHandler : AsyncRequestHandler<ProcessMessageRequest>
    {
        private readonly ILogger _logger;
        private readonly EventHubProducerClient _replyProducer;

        public ProcessMessageHandler(
            ILogger<ProcessMessageHandler> logger,
            EventHubsConfiguration hubConfiguration)
        {
            _replyProducer = new EventHubProducerClient(hubConfiguration.ReplyConnectionString);
            _logger = logger;
        }

        protected override async Task Handle(ProcessMessageRequest request, CancellationToken cancellationToken)
        {
            var correlationId = request.Args.Data.CorrelationId;

            Exception processedWithError = null;

            try
            {
                var message = request.Args.Data.EventBody.ToObjectFromJson<Message>();

                if (message.SimulateError)
                    throw new Exception($"This message is an error simulation.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                processedWithError = ex;
            }
            finally
            {
                _logger.LogInformation($"[Partition - {request.Args.Partition.PartitionId}] - Message {request.Args.Data.CorrelationId}.");

                await ReplyAsync(correlationId, new ReplyMessage()
                {
                    Detail = processedWithError?.Message,
                    Error = processedWithError != null,
                    MessageId = correlationId
                });

                await request.Args.UpdateCheckpointAsync();
            }
        }

        private async Task ReplyAsync(string correlationId, ReplyMessage reply)
        {
            var eventBody = new BinaryData(reply);

            var eventData = new EventData(eventBody)
            {
                CorrelationId = correlationId
            };

            await _replyProducer.SendAsync(new EventData[]
            {
                eventData
            });
        }
    }
}
