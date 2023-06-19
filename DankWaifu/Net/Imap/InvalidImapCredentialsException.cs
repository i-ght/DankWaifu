using System;

namespace DankWaifu.Net.Imap
{
    public class InvalidImapCredentialsException : Exception
    {
        public InvalidImapCredentialsException()
        {
        }

        public InvalidImapCredentialsException(string message) : base(message)
        {
        }

        public InvalidImapCredentialsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}