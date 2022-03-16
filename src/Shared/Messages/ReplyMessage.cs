namespace Shared.Messages
{
    public class ReplyMessage
    {
        public ReplyMessage()
        {
            Error = false;
        }

        public string MessageId { get; set; }
        public bool Error { get; set; }
        public string Detail { get; set; }
        public int Attempts { get; set; }
    }
}
