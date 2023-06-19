using System.Text;

namespace DankWaifu.Net.Imap
{
    public static class ImapPackets
    {
        public static byte[] CreateLoginPacket(
            string tag,
            string loginId,
            string password)
        {
            var loginPacket = $"{tag} LOGIN \"{loginId}\" \"{password}\"\r\n";
            var loginPacketBytes = Encoding.UTF8.GetBytes(loginPacket);
            return loginPacketBytes;
        }

        public static byte[] CreateRetrieveMailboxesPacket(string tag)
        {
            var retrieveMailboxesPacket = $"{tag} LIST \"\" \"*\"\r\n";
            var retrieveMailboxesPacketBytes = Encoding.UTF8.GetBytes(retrieveMailboxesPacket);
            return retrieveMailboxesPacketBytes;
        }

        public static byte[] CreateSelectMailboxPacket(string tag, string mailboxId)
        {
            var selectMailboxPacket = $"{tag} SELECT \"{mailboxId}\"\r\n";
            var selectMailboxPacketBytes = Encoding.UTF8.GetBytes(selectMailboxPacket);
            return selectMailboxPacketBytes;
        }

        public static byte[] CreateFetchMessageUidsPacket(string tag)
        {
            var fetchMessageUidsPacket = $"{tag} UID FETCH 1:* (UID)\r\n";
            var fetchMessageUidsPacketBytes = Encoding.UTF8.GetBytes(fetchMessageUidsPacket);
            return fetchMessageUidsPacketBytes;
        }

        public static byte[] CreateSearchBySubjectPacket(
            string tag,
            string keyword)
        {
            var searchBySubjPacket = $"{tag} UID SEARCH SUBJECT \"{keyword}\"\r\n";
            var searchBySubjPacketBytes = Encoding.UTF8.GetBytes(searchBySubjPacket);
            return searchBySubjPacketBytes;
        }

        public static byte[] CreateFetchMessageBodyWithUidPacket(string tag, int messageUid)
        {
            var fetchMessageBodyPacket = $"{tag} UID FETCH {messageUid} body[text]\r\n";
            var fetchMessageBodyPacketBytes = Encoding.UTF8.GetBytes(fetchMessageBodyPacket);
            return fetchMessageBodyPacketBytes;
        }

        public static byte[] CreateFetchUidsAndSubjectsPacket(
            string tag,
            int beginningSeqNum,
            int endingSeqNum)
        {
            var fetchUidsAndSubjectsPacket = $"{tag} FETCH {beginningSeqNum}:{endingSeqNum} (UID FLAGS BODY[HEADER.FIELDS (SUBJECT DATE FROM)])\r\n";
            var fetchUidsAndSubjectBytes = Encoding.UTF8.GetBytes(fetchUidsAndSubjectsPacket);
            return fetchUidsAndSubjectBytes;
        }
    }
}