using System;
using Microsoft.Extensions.Configuration;

namespace Shared
{
    public class EventHubsConfiguration
    {
        public string ConnectionString { get; }
        public string ReplyConnectionString { get; }
        public string ConsumerGroup { get; }
        public string BlobStorageConnectionString { get; }
        public string BlobContainerName { get; }
        public string ReplyBlobContainerName { get; }

        public EventHubsConfiguration(IConfiguration configuration)
        {
            ConnectionString = configuration.GetValue<string>("EventHubs:ConnectionString");

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new ArgumentNullException($"Invalid EventHubs:ConnectionString configuration value");

            ConsumerGroup = configuration.GetValue<string>("EventHubs:ConsumerGroup");

            if (string.IsNullOrWhiteSpace(ConsumerGroup))
                throw new ArgumentNullException($"Invalid EventHubs:ConsumerGroup configuration value");

            ReplyConnectionString = configuration.GetValue<string>("EventHubs:ReplyConnectionString");

            BlobStorageConnectionString = configuration.GetValue<string>("EventHubs:BlobStorageConnectionString");

            if (string.IsNullOrWhiteSpace(BlobStorageConnectionString))
                throw new ArgumentNullException($"Invalid EventHubs:BlobStorageConnectionString configuration value");

            BlobContainerName = configuration.GetValue<string>("EventHubs:BlobContainerName");

            if (string.IsNullOrWhiteSpace(BlobContainerName))
                throw new ArgumentNullException($"Invalid EventHubs:BlobContainerName configuration value");

            ReplyBlobContainerName = configuration.GetValue<string>("EventHubs:ReplyBlobContainerName");

            if (string.IsNullOrWhiteSpace(ReplyBlobContainerName))
                throw new ArgumentNullException($"Invalid EventHubs:ReplyBlobContainerName configuration value");
        }
    }
}
