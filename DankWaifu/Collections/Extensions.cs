using DankWaifu.Net;
using DankWaifu.Sys;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DankWaifu.Collections
{
    public static class Extensions
    {
        /// <summary>
        /// Gets the next item from the queue. If requeue, adds it back.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        /// <param name="requeue"></param>
        /// <returns></returns>
        public static T GetNext<T>(this Queue<T> queue, bool requeue = true)
        {
            if (queue == null)
                throw new ArgumentNullException(nameof(queue));

            if (queue.Count == 0)
                return default(T);

            var ret = queue.Dequeue();
            if (requeue)
                queue.Enqueue(ret);

            return ret;
        }

        /// <summary>
        /// Gets the next item from the queue. If requeue, adds it back.
        /// </summary>
        /// <typeparam name="T">Type of the items int he queue.</typeparam>
        /// <param name="concurrentQueue">ConcurrentQueue instance.</param>
        /// <param name="requeue">Requeue the item after its dequeued.</param>
        /// <returns>The first item of the queue if count is > 0.</returns>
        public static T GetNext<T>(this ConcurrentQueue<T> concurrentQueue, bool requeue = true)
        {
            if (concurrentQueue == null)
                throw new ArgumentNullException(nameof(concurrentQueue));

            if (concurrentQueue.Count == 0)
                return default(T);

            concurrentQueue.TryDequeue(out var ret);
            if (requeue)
                concurrentQueue.Enqueue(ret);

            return ret;
        }

        /// <summary>
        /// Clears the ConcurrentQueue
        /// </summary>
        /// <param name="concurrentQueue">ConcurrentQueue instance</param>
        /// <typeparam name="T"></typeparam>
        public static void Clear<T>(this ConcurrentQueue<T> concurrentQueue)
        {
            if (concurrentQueue == null)
                throw new ArgumentNullException(nameof(concurrentQueue));

            while (concurrentQueue.Count > 0)
            {
                concurrentQueue.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Selects an item at random from the list
        /// </summary>
        /// <typeparam name="T">Type of the collection</typeparam>
        /// <param name="collection">ICollection to select randomly from</param>
        /// <returns>A randomly selected item from the ICollection</returns>
        public static T RandomSelection<T>(this ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (collection.Count == 0)
                return default(T);

            var randomlySelectedIndex = RandomHelpers.RandomInt(collection.Count);
            return collection.ElementAt(randomlySelectedIndex);
        }

        /// <summary>
        /// Selects an item at random from the list
        /// </summary>
        /// <typeparam name="T">Type of the collection</typeparam>
        /// <param name="array">Array to select randomly from</param>
        /// <returns>A randomly selected item from the ICollection</returns>
        public static T RandomSelection<T>(this T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(
                    nameof(array)
                );
            }

            if (array.Length == 0)
                return default(T);

            var randomlySelectedIndex = RandomHelpers.RandomInt(array.Length);
            return array[randomlySelectedIndex];
        }

        /// <summary>
        /// Loads the contents of the specified file into the ConcurrentQueue
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="fileName"></param>
        /// <param name="clear"></param>
        public static void LoadFromFile(this ConcurrentQueue<string> queue, string fileName, bool clear = true)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"The file {fileName} does not exist.");

            if (clear)
                queue.Clear();

            using (var stream = new StreamReader(fileName))
            {
                while (!stream.EndOfStream)
                    queue.Enqueue(stream.ReadLine());
            }
        }

        /// <summary>
        /// Loads the contents of the specified file into the ConcurrentQueue
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="fileName"></param>
        /// <param name="clear"></param>
        public static async Task LoadFromFileAsync(this ConcurrentQueue<string> queue, string fileName, bool clear = true)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"The file {fileName} does not exist.");

            if (clear)
                queue.Clear();

            using (var stream = new StreamReader(fileName))
            {
                while (!stream.EndOfStream)
                    queue.Enqueue(await stream.ReadLineAsync()
                        .ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Loads specified file's contents into the Queue
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="fileName"></param>
        /// <param name="clear"></param>
        public static void LoadFromFile(this Queue<string> queue, string fileName, bool clear = true)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"The file {fileName} does not exist.");

            if (clear)
                queue.Clear();

            using (var sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                    queue.Enqueue(sr.ReadLine());
            }
        }

        /// <summary>
        /// Loads specified file's contents into the Queue
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="fileName"></param>
        /// <param name="clear"></param>
        public static async Task LoadFromFileAsync(this Queue<string> queue, string fileName, bool clear = true)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"The file {fileName} does not exist.");

            if (clear)
                queue.Clear();

            using (var sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                    queue.Enqueue(await sr.ReadLineAsync()
                        .ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Loads specified file's contents into the List
        /// </summary>
        /// <param name="list"></param>
        /// <param name="fileName"></param>
        /// <param name="clear"></param>
        public static void LoadFromFile(this ICollection<string> list, string fileName, bool clear = true)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"The file {fileName} does not exist.");

            if (clear)
                list.Clear();

            using (var sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                    list.Add(sr.ReadLine());
            }
        }

        /// <summary>
        /// Loads specified file's contents into the List
        /// </summary>
        /// <param name="list"></param>
        /// <param name="fileName"></param>
        /// <param name="clear"></param>
        public static async Task LoadFromFileAsync(this ICollection<string> list, string fileName, bool clear = true)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"The file {fileName} does not exist.");

            if (clear)
                list.Clear();

            using (var sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                    list.Add(await sr.ReadLineAsync()
                        .ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Loads specified file's contents into the List
        /// </summary>
        /// <param name="list"></param>
        /// <param name="fileName"></param>
        /// <param name="clear"></param>
        public static async Task LoadFromFileAsync(this IList<string> list, string fileName, bool clear = true)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"The file {fileName} does not exist.");

            if (clear)
                list.Clear();

            using (var sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                    list.Add(await sr.ReadLineAsync()
                        .ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Shuffles the contents of the ICollection
        /// </summary>
        /// <param name="collection">ICollection to shuffle</param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Shuffle<T>(this ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (collection.Count == 0)
                return;

            var list = new List<T>(collection);
            collection.Clear();

            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = RandomHelpers.RandomInt(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            foreach (var item in list)
                collection.Add(item);
        }

        /// <summary>
        /// Shuffles the contents of the ConcurrentQueue.
        /// </summary>
        /// <param name="concurrentQueue"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Shuffle<T>(this ConcurrentQueue<T> concurrentQueue)
        {
            if (concurrentQueue == null)
                throw new ArgumentNullException(nameof(concurrentQueue));

            if (concurrentQueue.Count == 0)
                return;

            var list = concurrentQueue.ToList();
            concurrentQueue.Clear();

            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = RandomHelpers.RandomInt(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            foreach (var item in list)
                concurrentQueue.Enqueue(item);
        }

        /// <summary>
        /// Shuffles the contents of the Queue.
        /// </summary>
        /// <param name="queue"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Shuffle<T>(this Queue<T> queue)
        {
            if (queue == null)
                throw new ArgumentNullException(nameof(queue));

            if (queue.Count == 0)
                return;

            var list = queue.ToList();
            queue.Clear();

            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = RandomHelpers.RandomInt(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            foreach (var item in list)
                queue.Enqueue(item);
        }

        public static string ToUrlEncodedQueryString(this ICollection<KeyValuePair<string, string>> kvpCollection, bool spaceIsPlus = true)
        {
            var lst = new List<string>();
            foreach (var kvp in kvpCollection)
            {
                var key = kvp.Key ?? string.Empty;
                var val = kvp.Value ?? string.Empty;
                lst.Add($"{HttpHelpers.UrlEncode(key, spaceIsPlus)}={HttpHelpers.UrlEncode(val, spaceIsPlus)}");
            }

            return string.Join("&", lst);
        }

        public static string ToUrlQueryString(this ICollection<KeyValuePair<string, string>> kvpCollection)
        {
            var lst = new List<string>();
            foreach (var kvp in kvpCollection)
            {
                var key = kvp.Key ?? string.Empty;
                var val = kvp.Value ?? string.Empty;
                lst.Add($"{key}={val}");
            }

            return string.Join("&", lst);
        }
    }
}