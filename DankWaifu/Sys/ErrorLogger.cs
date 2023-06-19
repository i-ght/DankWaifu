using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DankWaifu.Sys
{
    public static class ErrorLogger
    {
        private static readonly SemaphoreSlim WritelockAsync;

        static ErrorLogger()
        {
            WritelockAsync = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Logs the exception to file
        /// </summary>
        /// <param name="objException"></param>
        /// <param name="writeConsole"></param>
        /// <returns></returns>
        public static void Write(Exception objException, bool writeConsole = true)
        {
            if (objException == null)
                return;

            WritelockAsync.Wait();
            try
            {
                if (writeConsole)
                {
                    var stderr = new StringBuilder();
                    stderr.AppendLine("\"EXCEPTION\":");
                    stderr.AppendLine("{");
                    stderr.AppendLine($"    \"Type\": \"{objException.GetType().FullName}\",");
                    stderr.AppendLine($"    \"Message\": \"{objException.Message.Trim()}\"");
                    stderr.Append("}");

                    Console.Error.WriteLine(stderr.ToString());
                }

                using (var sw = new StreamWriter("Exceptions.txt", true))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Source: {objException.Source}");
                    sb.AppendLine($"Exception type: {objException.GetType().FullName}");
                    sb.AppendLine($"Method: {objException.TargetSite.Name}");
                    sb.AppendLine($"Date: {DateTime.Now.ToLongDateString()}");
                    sb.AppendLine($"Time: {DateTime.Now.ToLongTimeString()}");
                    sb.AppendLine($"Error: {objException.Message.Trim()}");
                    sb.AppendLine($"Stack trace: {objException.StackTrace}");
                    sb.AppendLine("[*]========================================================================");

                    sw.WriteLine(sb.ToString());
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
            finally
            {
                WritelockAsync.Release();
            }
        }

        /// <summary>
        /// Logs the exception to file
        /// </summary>
        /// <param name="objException"></param>
        /// <param name="writeConsole"></param>
        /// <returns></returns>
        public static async Task WriteAsync(Exception objException, bool writeConsole = true)
        {
            if (objException == null)
                return;

            await WritelockAsync.WaitAsync().ConfigureAwait(false);

            try
            {
                if (writeConsole)
                {
                    var stderr = new StringBuilder();
                    stderr.AppendLine("\"EXCEPTION\":");
                    stderr.AppendLine("{");
                    stderr.AppendLine($"    \"Type\": \"{objException.GetType().FullName}\",");
                    stderr.AppendLine($"    \"Message\": \"{objException.Message.Trim()}\"");
                    stderr.Append("}");

                    await Console.Error.WriteLineAsync(stderr.ToString())
                        .ConfigureAwait(false);
                }

                using (var sw = new StreamWriter("Exceptions.txt", true))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Source: {objException.Source}");
                    sb.AppendLine($"Exception type: {objException.GetType().FullName}");
                    sb.AppendLine($"Method: {objException.TargetSite.Name}");
                    sb.AppendLine($"Date: {DateTime.Now.ToLongDateString()}");
                    sb.AppendLine($"Time: {DateTime.Now.ToLongTimeString()}");
                    sb.AppendLine($"Error: {objException.Message.Trim()}");
                    sb.AppendLine($"Stack trace: {objException.StackTrace}");
                    sb.AppendLine("[*]========================================================================");

                    await sw.WriteLineAsync(sb.ToString())
                        .ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync(e.ToString())
                    .ConfigureAwait(false);
            }
            finally
            {
                WritelockAsync.Release();
            }
        }
    }
}