using Azure.Messaging.EventHubs.Processor;
using MediatR;

namespace KedaWorker.Requests
{
    public class ProcessMessageRequest : IRequest
    {
        public ProcessEventArgs Args { get; set; }

        public ProcessMessageRequest(ProcessEventArgs args)
        {
            Args = args;
        }
    }
}
