using System.IO;
using System.Threading.Tasks;

namespace DankWaifu.Collections
{
    public static class CollectionsHelpers
    {
        /// <summary>
        /// Counts all the lines present in a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static long CountLines(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"The file {fileName} does not exist.");

            long cnt = 0;
            using (var sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                {
                    sr.ReadLine();
                    cnt++;
                }
            }

            return cnt;
        }

        /// <summary>
        /// Counts all the lines present in a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<long> CountLinesAsync(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"The file {fileName} does not exist.");

            long cnt = 0;
            using (var sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                {
                    await sr.ReadLineAsync()
                        .ConfigureAwait(false);
                    cnt++;
                }
            }

            return cnt;
        }
    }
}