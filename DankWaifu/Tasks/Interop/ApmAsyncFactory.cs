﻿using System;
using System.Threading.Tasks;

namespace DankWaifu.Tasks.Interop
{
    /// <summary>
    /// Creation methods for tasks wrapping the Asynchronous Programming Model (APM), and APM wrapper methods around tasks.
    /// </summary>
    public static class ApmAsyncFactory
    {
        /// <summary>
        /// Wraps a <see cref="Task"/> into the Begin method of an APM pattern.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        /// <param name="callback">The callback method passed into the Begin method of the APM pattern.</param>
        /// <param name="state">The state passed into the Begin method of the APM pattern.</param>
        /// <returns>The asynchronous operation, to be returned by the Begin method of the APM pattern.</returns>
        public static IAsyncResult ToBegin(Task task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<byte>(state);
            CompleteAsync(task, callback, tcs);
            return tcs.Task;
        }

        // `async void` is on purpose, to raise `callback` exceptions directly on the thread pool.
        private static async void CompleteAsync(Task task, AsyncCallback callback, TaskCompletionSource<byte> tcs)
        {
            try
            {
                await task.ConfigureAwait(false);
                tcs.TrySetResult(0);
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
            finally
            {
                callback?.Invoke(tcs.Task);
            }
        }

        /// <summary>
        /// Wraps a <see cref="Task"/> into the End method of an APM pattern.
        /// </summary>
        /// <param name="asyncResult">The asynchronous operation returned by the matching Begin method of this APM pattern.</param>
        /// <returns>The result of the asynchronous operation, to be returned by the End method of the APM pattern.</returns>
        public static void ToEnd(IAsyncResult asyncResult)
        {
            ((Task)asyncResult).WaitAndUnwrapException();
        }

        /// <summary>
        /// Wraps a <see cref="Task{TResult}"/> into the Begin method of an APM pattern.
        /// </summary>
        /// <param name="task">The task to wrap. May not be <c>null</c>.</param>
        /// <param name="callback">The callback method passed into the Begin method of the APM pattern.</param>
        /// <param name="state">The state passed into the Begin method of the APM pattern.</param>
        /// <returns>The asynchronous operation, to be returned by the Begin method of the APM pattern.</returns>
        public static IAsyncResult ToBegin<TResult>(Task<TResult> task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<TResult>(state);
            CompleteAsync(task, callback, tcs);
            return tcs.Task;
        }

        // `async void` is on purpose, to raise `callback` exceptions directly on the thread pool.
        private static async void CompleteAsync<TResult>(Task<TResult> task, AsyncCallback callback, TaskCompletionSource<TResult> tcs)
        {
            try
            {
                tcs.TrySetResult(await task.ConfigureAwait(false));
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
            finally
            {
                callback?.Invoke(tcs.Task);
            }
        }

        /// <summary>
        /// Wraps a <see cref="Task{TResult}"/> into the End method of an APM pattern.
        /// </summary>
        /// <param name="asyncResult">The asynchronous operation returned by the matching Begin method of this APM pattern.</param>
        /// <returns>The result of the asynchronous operation, to be returned by the End method of the APM pattern.</returns>
        public static TResult ToEnd<TResult>(IAsyncResult asyncResult)
        {
            return ((Task<TResult>)asyncResult).WaitAndUnwrapException();
        }
    }
}