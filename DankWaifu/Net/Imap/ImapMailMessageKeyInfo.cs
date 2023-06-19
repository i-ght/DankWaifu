namespace DankWaifu.Net.Imap
{
    public struct ImapMailMessageKeyInfo
    {
        public ImapMailMessageKeyInfo(
            int sequence,
            int uid)
        {
            Sequence = sequence;
            Uid = uid;
        }

        public int Sequence { get; }
        public int Uid { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is ImapMailMessageKeyInfo instance))
                return false;

            return instance.Sequence == Sequence &&
                   instance.Uid == Uid;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + Sequence.GetHashCode();
                hash = hash * 23 + Uid.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(
            ImapMailMessageKeyInfo left,
            ImapMailMessageKeyInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            ImapMailMessageKeyInfo left,
            ImapMailMessageKeyInfo right)
        {
            return !(left == right);
        }
    }
}