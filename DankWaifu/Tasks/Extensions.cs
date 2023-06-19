using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace DankWaifu.Tasks
{
    public static class Extensions
    {
        /// <summary>
        /// Throws a timeout exception if the task takes longer then timeout to compute
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int timeout)
        {
            using (var cToken = new CancellationTokenSource())
            {
                var delay = Task.Delay(timeout, cToken.Token);
                var completedTask = await Task.WhenAny(task, delay)
                    .ConfigureAwait(false);
                if (completedTask != task)
                {
                    await delay.ConfigureAwait(false);
                    throw new TimeoutException("The operation has timed out.");
                }

                cToken.Cancel();
                return await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Waits for the task to complete, unwrapping any exceptions.
        /// </summary>
        /// <param name="task">The task. May not be <c>null</c>.</param>
        public static void WaitAndUnwrapException(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Waits for the task to complete, unwrapping any exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the task.</typeparam>
        /// <param name="task">The task. May not be <c>null</c>.</param>
        /// <returns>The result of the task.</returns>
        public static TResult WaitAndUnwrapException<TResult>(this Task<TResult> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            return task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Throws a timeout exception if the task takes longer then timeout to compute
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var cToken = new CancellationTokenSource())
            {
                var delay = Task.Delay(timeout, cToken.Token);
                var completedTask = await Task.WhenAny(task, delay)
                    .ConfigureAwait(false);
                if (completedTask != task)
                {
                    await delay.ConfigureAwait(false);
                    throw new TimeoutException("The operation has timed out.");
                }

                cToken.Cancel();
                return await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Throws a timeout exception if the task takes longer then timeout to compute
        /// </summary>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task TimeoutAfter(this Task task, int timeout)
        {
            using (var cToken = new CancellationTokenSource())
            {
                var delay = Task.Delay(timeout, cToken.Token);
                var completedTask = await Task.WhenAny(task, delay)
                    .ConfigureAwait(false);
                if (completedTask != task)
                {
                    await delay.ConfigureAwait(false);
                    throw new TimeoutException("The operation has timed out.");
                }

                cToken.Cancel();
                await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Throws a timeout exception if the task takes longer then timeout to compute
        /// </summary>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task TimeoutAfter(
            this Task task,
            TimeSpan timeout)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (timeout.Ticks <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(timeout),
                    $"{nameof(timeout)} must be >= 0"
                );
            }

            using (var cToken = new CancellationTokenSource())
            {
                var delay = Task.Delay(timeout, cToken.Token);
                var completedTask = await Task.WhenAny(task, delay)
                    .ConfigureAwait(false);
                if (completedTask != task)
                {
                    await delay.ConfigureAwait(false);
                    throw new TimeoutException("The operation has timed out.");
                }

                cToken.Cancel();
                await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Fire and forget an async Task
        /// </summary>
        /// <param name="task"></param>
        public static async void BeginTask(this Task task)
        {
            await task.ConfigureAwait(false);
        }

        // Helper method when implementing an APM wrapper around a Task based async method which returns a result.
        // In the BeginMethod method, you would call use ToApm to wrap a call to MethodAsync:
        //     return MethodAsync(params).ToApm(callback, state);
        // In the EndMethod, you would use ToApmEnd<TResult> to ensure the correct exception handling
        // This will handle throwing exceptions in the correct place and ensure the IAsyncResult contains the provided
        // state object
        public static Task<TResult> ToApm<TResult>(this Task<TResult> task, AsyncCallback callback, object state = null)
        {
            // When using APM, the returned IAsyncResult must have the passed in state object stored in AsyncState. This
            // is so the callback can regain state. If the incoming task already holds the state object, there's no need
            // to create a TaskCompletionSource to ensure the returned (IAsyncResult)Task has the right state object.
            // This is a performance optimization for this special case.
            if (task.AsyncState == state)
            {
                if (callback != null)
                {
                    task.ContinueWith((antecedent, obj) =>
                    {
                        var callbackObj = obj as AsyncCallback;
                        callbackObj?.Invoke(antecedent);
                    }, callback, CancellationToken.None, TaskContinuationOptions.HideScheduler, TaskScheduler.Default);
                }
                return task;
            }

            // Need to create a TaskCompletionSource so that the returned Task object has the correct AsyncState value.
            var tcs = new TaskCompletionSource<TResult>(state);
            var continuationState = Tuple.Create(tcs, callback);
            task.ContinueWith((antecedent, obj) =>
            {
                var tuple = (Tuple<TaskCompletionSource<TResult>, AsyncCallback>)obj;
                var tcsObj = tuple.Item1;
                var callbackObj = tuple.Item2;
                if (antecedent.IsFaulted)
                {
                    if (antecedent.Exception != null)
                    {
                        tcsObj.TrySetException(
                            antecedent.Exception.InnerException ?? antecedent.Exception);
                    }
                    else
                    {
                        tcsObj.TrySetException(new InvalidOperationException("Task faulted"));
                    }
                }
                else if (antecedent.IsCanceled)
                {
                    tcsObj.TrySetCanceled();
                }
                else
                {
                    tcsObj.TrySetResult(antecedent.Result);
                }

                callbackObj?.Invoke(tcsObj.Task);
            }, continuationState, CancellationToken.None, TaskContinuationOptions.HideScheduler, TaskScheduler.Default);
            return tcs.Task;
        }

        // Helper method when implementing an APM wrapper around a Task based async method which returns a result.
        // In the BeginMethod method, you would call use ToApm to wrap a call to MethodAsync:
        //     return MethodAsync(params).ToApm(callback, state);
        // In the EndMethod, you would use ToApmEnd to ensure the correct exception handling
        // This will handle throwing exceptions in the correct place and ensure the IAsyncResult contains the provided
        // state object
        public static Task ToApm(this Task task, AsyncCallback callback, object state = null)
        {
            // When using APM, the returned IAsyncResult must have the passed in state object stored in AsyncState. This
            // is so the callback can regain state. If the incoming task already holds the state object, there's no need
            // to create a TaskCompletionSource to ensure the returned (IAsyncResult)Task has the right state object.
            // This is a performance optimization for this special case.
            if (task.AsyncState == state)
            {
                if (callback != null)
                {
                    task.ContinueWith((antecedent, obj) =>
                    {
                        var callbackObj = obj as AsyncCallback;
                        callbackObj?.Invoke(antecedent);
                    }, callback, CancellationToken.None, TaskContinuationOptions.HideScheduler, TaskScheduler.Default);
                }
                return task;
            }

            // Need to create a TaskCompletionSource so that the returned Task object has the correct AsyncState value.
            // As we intend to create a task with no Result value, we don't care what result type the TCS holds as we
            // won't be using it. As Task<TResult> derives from Task, the returned Task is compatible.
            var tcs = new TaskCompletionSource<byte>(state);
            var continuationState = Tuple.Create(tcs, callback);
            task.ContinueWith((antecedent, obj) =>
                {
                    var tuple = (Tuple<TaskCompletionSource<byte>, AsyncCallback>)obj;
                    var tcsObj = tuple.Item1;
                    var callbackObj = tuple.Item2;
                    if (antecedent.IsFaulted)
                    {
                        if (antecedent.Exception != null)
                        {
                            tcsObj.TrySetException(
                                antecedent.Exception.InnerException ?? antecedent.Exception);
                        }
                        else
                        {
                            tcsObj.TrySetException(new InvalidOperationException("Task faulted"));
                        }
                    }
                    else if (antecedent.IsCanceled)
                    {
                        tcsObj.TrySetCanceled();
                    }
                    else
                    {
                        tcsObj.TrySetResult(0);
                    }

                    callbackObj?.Invoke(tcsObj.Task);
                },
                continuationState,
                CancellationToken.None,
                TaskContinuationOptions.HideScheduler,
                TaskScheduler.Default);
            return tcs.Task;
        }

        /// <summary>
        /// Helper method to implement the End method of an APM method pair which is wrapping a Task
        /// based async method when the Task returns a result.By using task.GetAwaiter.GetResult(),
        /// the exception handling conventions are the same as when await'ing a task, i.e. this throws
        /// the first exception and doesn't wrap it in an AggregateException. It also throws the
        /// right exception if the task was cancelled.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="iar"></param>
        /// <returns></returns>
        public static TResult ToApmEnd<TResult>(this IAsyncResult iar)
        {
            try
            {
                var task = (Task<TResult>)iar;
                return task.GetAwaiter().GetResult();
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                // ReSharper disable once HeuristicUnreachableCode
                throw;
            }
        }

        /// <summary>
        /// Helper method to implement the End method of an APM method pair which is wrapping a Task
        /// based async method when the Task does not return result.
        /// </summary>
        /// <param name="iar"></param>
        public static void ToApmEnd(this IAsyncResult iar)
        {
            try
            {
                var task = (Task)iar;
                task.Wait();
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                // ReSharper disable once HeuristicUnreachableCode
                throw;
            }
        }
    }
}