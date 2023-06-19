using System.Threading;

namespace DankWaifu.Net.Imap
{
    internal class ImapTagState
    {
        private int _index;
        private readonly string _prefix;

        public ImapTagState()
        {
            _index = 0;
            _prefix = "A";
        }

        public string GetNextTag()
        {
            Interlocked.Increment(ref _index);
            var str = $"{_prefix}{_index:0000}";
            return str;
        }
    }
}