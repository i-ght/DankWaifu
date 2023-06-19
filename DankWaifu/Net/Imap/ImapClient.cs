using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DankWaifu.Net.Imap
{
    public static class ImapClient
    {
        private static readonly Regex MailboxIdRegex;
        private static readonly Regex MailboxMessageUidSeqSubRegex;
        private static readonly Regex ExistsRegex;

        static ImapClient()
        {
            MailboxIdRegex = new Regex("\\* list \\(.*?\\) .*?\\s(.*?)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            );
            MailboxMessageUidSeqSubRegex = new Regex(
                "\\* (\\d+) FETCH.*?UID (\\d+).*?Subject: (.*?)\r\n",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline
            );
            ExistsRegex = new Regex("\\* (\\d+) EXISTS",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            );
        }

        public static async Task<bool> TryVerifyCredentials(
            WebProxy proxy,
            string host,
            ushort port,
            string loginId,
            string password,
            bool ssl = true)
        {
            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));

            if (host == null)
                throw new ArgumentNullException(nameof(host));

            if (loginId == null)
                throw new ArgumentNullException(nameof(loginId));

            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrWhiteSpace(loginId))
                throw new ArgumentException($"{nameof(loginId)} must not be whitespace.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException($"{nameof(password)} must not be whitespace.");

            loginId = loginId.ToLower();

            using (var client = new TcpClientWrapper
            {
                SendTimeout = TimeSpan.FromSeconds(120),
                ReceiveTimeout = TimeSpan.FromSeconds(120),
                ConnectTimeout = TimeSpan.FromSeconds(120)
            })
            {
                try
                {
                    var tagState = new ImapTagState();
                    await InternalLogin(
                        client,
                        proxy,
                        host,
                        port,
                        loginId,
                        password,
                        tagState.GetNextTag(),
                        ssl
                    ).ConfigureAwait(false);
                    return true;
                }
                catch (InvalidImapCredentialsException)
                {
                    return false;
                }
            }
        }

        private static async Task InternalLogin(
            TcpClientWrapper client,
            WebProxy proxy,
            string host,
            ushort port,
            string loginId,
            string password,
            string tag,
            bool ssl)
        {
            await client.ConnectWithProxyAsync(
                proxy,
                host,
                port
            ).ConfigureAwait(false);

            if (ssl)
            {
                await client.InitSslStreamAsync(host)
                    .ConfigureAwait(false);
            }

            await Login(
                tag,
                loginId,
                password,
                client
            ).ConfigureAwait(false);
        }

        public static async Task<ReadOnlyCollection<string>> RetrieveMessagesWithPartialSubject(
            WebProxy proxy,
            string host,
            ushort port,
            string loginId,
            string password,
            string subject,
            TimeSpan timeout,
            bool ssl = true)
        {
            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));

            if (host == null)
                throw new ArgumentNullException(nameof(host));

            if (loginId == null)
                throw new ArgumentNullException(nameof(loginId));

            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrWhiteSpace(loginId))
                throw new ArgumentException($"{nameof(loginId)} must not be whitespace.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException($"{nameof(password)} must not be whitespace.");

            loginId = loginId.ToLower();

            using (var client = new TcpClientWrapper
            {
                SendTimeout = TimeSpan.FromSeconds(120),
                ReceiveTimeout = TimeSpan.FromSeconds(120),
                ConnectTimeout = TimeSpan.FromSeconds(120)
            })
            {
                var tagState = new ImapTagState();
                await InternalLogin(
                    client,
                    proxy,
                    host,
                    port,
                    loginId,
                    password,
                    tagState.GetNextTag(),
                    ssl
                ).ConfigureAwait(false);

                var mailboxes = await RetrieveMailboxes(
                    tagState.GetNextTag(),
                    client
                ).ConfigureAwait(false);

                var mailboxesToSearch = new List<string>();
                foreach (var mailbox in mailboxes)
                {
                    var lower = mailbox.ToLower();
                    if (lower != "inbox" &&
                        lower != "bulk mail" &&
                        lower != "spam")
                    {
                        continue;
                    }

                    mailboxesToSearch.Add(mailbox);
                }

                var ret = await RetrieveMessagesThatContainSubject(
                    mailboxesToSearch,
                    subject,
                    tagState,
                    client,
                    timeout
                ).ConfigureAwait(false);

                return ret;
            }
        }

        private static async Task<ReadOnlyCollection<string>> RetrieveMessagesThatContainSubject(
            IReadOnlyCollection<string> mailboxes,
            string subject,
            ImapTagState tagState,
            TcpClientWrapper client,
            TimeSpan timeout)
        {
            var ret = new List<string>();
            using (var cancellationTokenSrc = new CancellationTokenSource(timeout))
            {
                while (!cancellationTokenSrc.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(
                            5000,
                            cancellationTokenSrc.Token
                        ).ConfigureAwait(false);

                        foreach (var mailbox in mailboxes)
                        {
                            var selectResponse = await SelectMailbox(
                                tagState.GetNextTag(),
                                mailbox,
                                client
                            ).ConfigureAwait(false);
                            if (selectResponse.MessageCount == 0)
                                continue;

                            var uids = await RetrieveUidsThatContainSubject(
                                tagState.GetNextTag(),
                                subject,
                                selectResponse.MessageCount,
                                client
                            ).ConfigureAwait(false);

                            if (uids.Count == 0)
                                continue;

                            foreach (var uid in uids)
                            {
                                var messageBody = await RetrieveMessageBodyWithUid(
                                    tagState.GetNextTag(),
                                    uid,
                                    client
                                ).ConfigureAwait(false);

                                messageBody = ImapHelpers.DecodeWords(messageBody);

                                messageBody = messageBody
                                    .Replace("=\r\n", string.Empty)
                                    .Replace("=\n", string.Empty)
                                    .Replace("\r\n", string.Empty)
                                    .Replace("\n", string.Empty)
                                    .Replace("=3D", "=")
                                    .Replace("=2C", ",");
                                ret.Add(messageBody);
                            }

                            return ret.AsReadOnly();
                        }

                        await Task.Delay(
                            5000,
                            cancellationTokenSrc.Token
                        ).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }

            throw new TimeoutException("Timed out waiting for email message to arrive.");
        }

        private static async Task<ReadOnlyCollection<int>> RetrieveUidsThatContainSubject(
            string tag,
            string subjectToFind,
            int mailboxMessageCount,
            TcpClientWrapper client)
        {
            int begSeqNum;
            int endSeqNum;
            if (mailboxMessageCount < 10)
            {
                begSeqNum = mailboxMessageCount - mailboxMessageCount + 1;
                endSeqNum = mailboxMessageCount;
            }
            else
            {
                begSeqNum = mailboxMessageCount - 10;
                endSeqNum = mailboxMessageCount;
            }

            var fetchUidsAndSubjectsPacket = ImapPackets.CreateFetchUidsAndSubjectsPacket(
                tag,
                begSeqNum,
                endSeqNum
            );
            await client.SendDataAsync(fetchUidsAndSubjectsPacket)
                .ConfigureAwait(false);

            var retrieveUidsWithSubjectResponse = await RetrieveResponse(
                client,
                tag
            ).ConfigureAwait(false);

            subjectToFind = subjectToFind.ToLower();

            var ret = new List<int>();
            foreach (Match match in MailboxMessageUidSeqSubRegex.Matches(
                retrieveUidsWithSubjectResponse))
            {
                var subject = match.Groups[3].Value;
                if (string.IsNullOrWhiteSpace(subject))
                    continue;

                subject = ImapHelpers.DecodeWords(subject);
                subject = subject.ToLower();
                if (!subject.Contains(subjectToFind))
                    continue;

                var uidStr = match.Groups[2].Value;
                if (!int.TryParse(uidStr, out var uid))
                    continue;

                ret.Add(uid);
            }

            return ret
                .AsReadOnly();
        }

        //private static async Task<ReadOnlyCollection<string>> RetrieveMessagesThatContainSubjectWithSearch(
        //    IReadOnlyCollection<string> mailboxes,
        //    string subject,
        //    ImapTagState tagState,
        //    TcpClientWrapper client,
        //    TimeSpan timeout)
        //{
        //    var ret = new List<string>();
        //    using (var cancellationTokenSrc = new CancellationTokenSource(timeout))
        //    {
        //        try
        //        {
        //            while (!cancellationTokenSrc.IsCancellationRequested)
        //            {
        //                await Task.Delay(
        //                    5000,
        //                    cancellationTokenSrc.Token
        //                ).ConfigureAwait(false);

        //                foreach (var mailbox in mailboxes)
        //                {
        //                    await SelectMailbox(
        //                        tagState.GetNextTag(),
        //                        mailbox,
        //                        client
        //                    ).ConfigureAwait(false);

        //                    var messageUids = await SearchForMessageBySubject(
        //                        tagState.GetNextTag(),
        //                        subject,
        //                        client
        //                    ).ConfigureAwait(false);

        //                    if (messageUids.Count == 0)
        //                        continue;

        //                    //var allMessageKeyInfos = await RetrieveMessagesKeyInfo(
        //                    //    tagState,
        //                    //    client
        //                    //).ConfigureAwait(false);
        //                    //if (allMessageKeyInfos.Count != messageUids.Count)
        //                    //{
        //                    //    throw new InvalidOperationException(
        //                    //        $"Found email messages that contained subject {subject} with uids {string.Join(", ", messageUids)}, but failed to find their sequence ids."
        //                    //    );
        //                    //}

        //                    //var msgSeqNumsThatContainSubj = new List<int>();
        //                    //foreach (var msgKeyInfo in allMessageKeyInfos)
        //                    //{
        //                    //    if (messageUids.Any(msgUid => msgKeyInfo.Uid == msgUid))
        //                    //    {
        //                    //        msgSeqNumsThatContainSubj.Add(msgKeyInfo.Sequence);
        //                    //    }
        //                    //}

        //                    //if (msgSeqNumsThatContainSubj.Count == 0)
        //                    //    continue;

        //                    foreach (var msgUid in messageUids)
        //                    {
        //                        var tag = tagState.GetNextTag();
        //                        var messageBody = await RetrieveMessageBodyWithUid(
        //                            tag,
        //                            msgUid,
        //                            client
        //                        ).ConfigureAwait(false);

        //                        ret.Add(messageBody);
        //                    }
        //                }

        //                await Task.Delay(
        //                    5000,
        //                    cancellationTokenSrc.Token
        //                ).ConfigureAwait(false);
        //            }
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            throw new TimeoutException("Timed out waiting for email message to arrive.");
        //        }
        //    }

        //    return ret
        //        .AsReadOnly();
        //}

        private static async Task<IReadOnlyCollection<string>> RetrieveMailboxes(
            string tag,
            TcpClientWrapper client)
        {
            var retrieveMailboxesPacket = ImapPackets.CreateRetrieveMailboxesPacket(tag);
            await client.SendDataAsync(retrieveMailboxesPacket)
                .ConfigureAwait(false);

            var retrieveMailboxesResponse = await RetrieveResponse(client, tag)
                .ConfigureAwait(false);

            var ret = new List<string>();
            var split = retrieveMailboxesResponse.Split('\r', '\n');
            foreach (var line in split)
            {
                if (!line.StartsWith("*"))
                    continue;

                var match = MailboxIdRegex.Match(line);
                if (!match.Success)
                    continue;

                var mailboxId = match.Groups[1].Value;
                if (mailboxId.Contains("\""))
                    mailboxId = mailboxId.Replace("\"", string.Empty);
                ret.Add(mailboxId);
            }

            if (ret.Count == 0)
            {
                throw new InvalidOperationException(
                    "Failed to parse mailbox ids from imap response."
                );
            }

            return ret
                .AsReadOnly();
        }

        private static async Task Login(
            string tag,
            string loginId,
            string password,
            TcpClientWrapper client)
        {
            var loginPacket = ImapPackets.CreateLoginPacket(tag, loginId, password);
            await client.SendDataAsync(loginPacket)
                .ConfigureAwait(false);

            var loginResponse = await RetrieveResponse(client, tag)
                .ConfigureAwait(false);

            var loginResponseToLower = loginResponse.ToLower();
            if (loginResponseToLower.Contains("authenticationfailed"))
            {
                if (loginResponseToLower.Contains("invalid credentials"))
                {
                    throw new InvalidImapCredentialsException(
                        $"Imap server returned invalid credentials for account {loginId}."
                    );
                }
                throw new InvalidImapCredentialsException(
                    $"Imap server returned authentication failed for account {loginId}."
                );
            }

            if (!loginResponseToLower.Contains("ok login completed") &&
                !loginResponseToLower.Contains("ok authenticate completed"))
            {
                throw new InvalidImapCredentialsException(
                    "Imap server did not return ok login completed."
                );
            }
        }

        private static async Task<SelectMailboxResponse> SelectMailbox(
            string tag,
            string mailboxId,
            TcpClientWrapper client)
        {
            var selectMailboxPacket = ImapPackets.CreateSelectMailboxPacket(
                tag,
                mailboxId
            );
            await client.SendDataAsync(selectMailboxPacket)
                .ConfigureAwait(false);

            var selectResponse = await RetrieveResponse(client, tag)
                .ConfigureAwait(false);

            var selectResponseToLower = selectResponse.ToLower();
            if (!selectResponseToLower.Contains("select completed"))
            {
                throw new InvalidOperationException(
                    "Imap server did not return select completed after trying to select a mailbox."
                );
            }

            var msgCntStr = string.Empty;
            var match = ExistsRegex.Match(selectResponseToLower);
            if (match.Success)
                msgCntStr = match.Groups[1].Value;

            if (string.IsNullOrWhiteSpace(msgCntStr)
                || !int.TryParse(msgCntStr, out var cnt))
            {
                throw new InvalidOperationException(
                    "Failed to parse exists count from select response"
                );
            }

            var ret = new SelectMailboxResponse(cnt);
            return ret;
        }

        //private static async Task<List<int>> SearchForMessageBySubject(
        //    string tag,
        //    string subject,
        //    TcpClientWrapper client)
        //{
        //    var searchPacket = ImapPackets.CreateSearchBySubjectPacket(tag, subject);
        //    await client.SendDataAsync(searchPacket)
        //        .ConfigureAwait(false);

        //    var searchResponse = await RetrieveResponse(client, tag)
        //        .ConfigureAwait(false);

        //    var split = searchResponse.Split('\r', '\n');
        //    var messageUids = new List<int>();
        //    foreach (var line in split)
        //    {
        //        if (!line.StartsWith("* SEARCH"))
        //            continue;

        //        var uidsStr = line.Replace("* SEARCH", string.Empty);
        //        if (string.IsNullOrWhiteSpace(uidsStr))
        //            continue;

        //        var spaceSplit = uidsStr.Split(' ');
        //        foreach (var messageUid in spaceSplit)
        //        {
        //            if (string.IsNullOrWhiteSpace(messageUid))
        //                continue;

        //            if (!int.TryParse(messageUid, out var i))
        //                continue;

        //            messageUids.Add(i);
        //        }
        //    }

        //    return messageUids;
        //}

        private static async Task<string> RetrieveMessageBodyWithUid(
            string tag,
            int messageUid,
            TcpClientWrapper client)
        {
            var retrieveMessageBodyPacket = ImapPackets.CreateFetchMessageBodyWithUidPacket(
                tag,
                messageUid
            );

            await client.SendDataAsync(retrieveMessageBodyPacket)
                .ConfigureAwait(false);

            var retrieveMessageBodyResponse = await RetrieveResponse(
                client,
                tag
            ).ConfigureAwait(false);

            return retrieveMessageBodyResponse;
        }

        private static async Task<string> RetrieveResponse(
            TcpClientWrapper client,
            string tag)
        {
            const byte newLine = 10;
            const byte carriageReturn = 13;

            var sb = new StringBuilder();
            var lst = new List<string>();
            while (true)
            {
                var buffer = new byte[1];
                var cnt = await client.ReceiveDataAsync(buffer, 0, buffer.Length)
                    .ConfigureAwait(false);
                if (cnt <= 0)
                    throw new InvalidOperationException("TcpClient received <=0 from server.");

                var @byte = buffer[0];

                switch (@byte)
                {
                    case carriageReturn:
                        break;

                    case newLine:
                        var line = sb.ToString();
                        lst.Add(line);
                        sb.Clear();
                        if (line.StartsWith(tag))
                            return string.Join("\r\n", lst);
                        break;

                    default:
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, cnt));
                        break;
                }
            }
        }
    }
}