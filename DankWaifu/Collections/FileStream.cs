using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DankWaifu.Collections
{
    public delegate string ParseLine(string line);

    public class FileStream
    {
        private readonly Queue<string> _queue;
        private readonly SemaphoreSlim _lock;
        private StreamReader _stream;

        public event EventHandler<long> GotLineCount;

        public ParseLine ParseLineFunc;

        public FileStream()
        {
            _queue = new Queue<string>();
            _lock = new SemaphoreSlim(1, 1);
            Blacklists = new List<ISet<string>>();
        }

        public List<ISet<string>> Blacklists { get; }

        public bool EOF
        {
            get
            {
                if (_stream == null)
                    return true;

                _lock.Wait();
                try
                {
                    return _stream.EndOfStream && _queue.Count == 0;
                }
                finally
                {
                    _lock.Release();
                }
            }
        }

        public long Index { get; private set; }
        public long Count { get; private set; }
        public string PrevLine { get; private set; }

        public void Open(string pathToFile)
        {
            if (!File.Exists(pathToFile))
                throw new FileNotFoundException($"the file {pathToFile} was not found at {GetType().FullName}.Open");

            _lock.Wait();

            try
            {
                _stream = new StreamReader(pathToFile);

                Index = 0;
                Task.Run(() =>
                {
                    var lineCount = CollectionsHelpers.CountLines(pathToFile);
                    Count = lineCount;
                    GotLineCount?.Invoke(this, lineCount);
                });
            }
            finally
            {
                _lock.Release();
            }
        }

        public string GetNext()
        {
            if (_stream == null)
                throw new InvalidOperationException($"{nameof(_stream)} is null at {GetType().FullName}.GetNextAsync");

            _lock.Wait();

            try
            {
                if (_queue.Count > 0)
                {
                    Count--;
                    Index++;
                    return _queue.Dequeue();
                }

                while (_queue.Count < 10000)
                {
                    if (_stream.EndOfStream)
                        break;

                    string line;
                    try
                    {
                        line = _stream.ReadLine();
                        PrevLine = line;
                    }
                    catch (Exception
#if DEBUG
#pragma warning disable 168
                        e
#pragma warning restore 168
#endif
                    )
                    {
                        continue;
                    }

                    var blacklisted = false;
                    var @break = false;
                    foreach (var blacklist in Blacklists)
                    {
                        switch (blacklist)
                        {
                            case ConcurrentHashSet<string> c:
                                {
                                    var item = ParseLineFunc != null ? ParseLineFunc.Invoke(line) : line;

                                    if (c.Contains(item))
                                    {
                                        @break = true;
                                        blacklisted = true;
                                    }
                                    break;
                                }
                            default:
                                {
                                    var item = ParseLineFunc != null ? ParseLineFunc.Invoke(line) : line;

                                    if (blacklist.Contains(item))
                                    {
                                        blacklisted = true;
                                        @break = true;
                                    }
                                    break;
                                }
                        }

                        if (@break)
                            break;
                    }

                    if (blacklisted)
                        continue;

                    _queue.Enqueue(line);
                }

                if (_queue.Count == 0)
                    return string.Empty;

                Count--;
                Index++;
                return _queue.Dequeue();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<string> GetNextAsync()
        {
            if (_stream == null)
                throw new InvalidOperationException($"{nameof(_stream)} is null at {GetType().FullName}.GetNextAsync");

            await _lock.WaitAsync()
                .ConfigureAwait(false);

            try
            {
                if (_queue.Count > 0)
                {
                    Count--;
                    Index++;
                    return _queue.Dequeue();
                }

                while (_queue.Count < 10000)
                {
                    if (_stream.EndOfStream)
                        break;

                    string line;
                    try
                    {
                        line = await _stream.ReadLineAsync()
                            .ConfigureAwait(false);
                        PrevLine = line;
                    }
                    catch (Exception
#if DEBUG
#pragma warning disable 168
                        e
#pragma warning restore 168
#endif
                        )
                    {
                        continue;
                    }

                    var blacklisted = false;
                    var @break = false;
                    foreach (var blacklist in Blacklists)
                    {
                        switch (blacklist)
                        {
                            case ConcurrentHashSet<string> c:
                                {
                                    var item = ParseLineFunc != null ? ParseLineFunc.Invoke(line) : line;

                                    if (c.Contains(item))
                                    {
                                        @break = true;
                                        blacklisted = true;
                                    }
                                    break;
                                }
                            default:
                                {
                                    var item = ParseLineFunc != null ? ParseLineFunc.Invoke(line) : line;

                                    if (blacklist.Contains(item))
                                    {
                                        blacklisted = true;
                                        @break = true;
                                    }
                                    break;
                                }
                        }

                        if (@break)
                            break;
                    }

                    if (blacklisted)
                        continue;

                    _queue.Enqueue(line);
                }

                if (_queue.Count == 0)
                    return string.Empty;

                Count--;
                Index++;
                return _queue.Dequeue();
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Close()
        {
            _lock.Wait();

            try
            {
                _queue.Clear();
                _stream?.Dispose();
                _stream = null;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}