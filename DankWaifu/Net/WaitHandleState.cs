using System.Threading;

namespace DankWaifu.Net
{
    internal class WaitHandlesState
    {
        public WaitHandlesState(WaitHandle asyncWaitHandle)
        {
            AsyncWaitHandle = asyncWaitHandle;
        }

        public RegisteredWaitHandle RegisteredWaitHandle { get; set; }
        public HttpReq HttpReq { get; set; }
        public WaitHandle AsyncWaitHandle { get; }
    }
}