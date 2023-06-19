namespace DankWaifu.Net.Imap
{
    internal class SelectMailboxResponse
    {
        public SelectMailboxResponse(int messageCount)
        {
            MessageCount = messageCount;
        }

        public int MessageCount { get; }
    }
}