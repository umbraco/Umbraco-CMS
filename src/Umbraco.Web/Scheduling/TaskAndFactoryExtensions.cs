using System;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.Scheduling
{
    internal static class TaskAndFactoryExtensions
    {
        #region Task Extensions

        // TODO: Not used, is this used in Deploy or something?
        static void SetCompletionSource<TResult>(TaskCompletionSource<TResult> completionSource, Task task)
        {
            if (task.IsFaulted)
                completionSource.SetException(task.Exception.InnerException);
            else
                completionSource.SetResult(default(TResult));
        }

        // TODO: Not used, is this used in Deploy or something?
        static void SetCompletionSource<TResult>(TaskCompletionSource<TResult> completionSource, Task<TResult> task)
        {
            if (task.IsFaulted)
                completionSource.SetException(task.Exception.InnerException);
            else
                completionSource.SetResult(task.Result);
        }

        // TODO: Not used, is this used in Deploy or something?
        public static Task ContinueWithTask(this Task task, Func<Task, Task> continuation)
        {
            var completionSource = new TaskCompletionSource<object>();
            task.ContinueWith(atask => continuation(atask).ContinueWith(
                    atask2 => SetCompletionSource(completionSource, atask2),
                    // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
                    TaskScheduler.Default),
                // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
                TaskScheduler.Default);
            return completionSource.Task;
        }

        // TODO: Not used, is this used in Deploy or something?
        public static Task ContinueWithTask(this Task task, Func<Task, Task> continuation, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<object>();
            task.ContinueWith(atask => continuation(atask).ContinueWith(
                    atask2 => SetCompletionSource(completionSource, atask2),
                    token,
                    TaskContinuationOptions.None,
                    // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
                    TaskScheduler.Default),
                token,
                TaskContinuationOptions.None,
                // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
                TaskScheduler.Default);
            return completionSource.Task;
        }

        #endregion

        #region TaskFactory Extensions

        public static Task Completed(this TaskFactory factory)
        {
            var taskSource = new TaskCompletionSource<object>();
            taskSource.SetResult(null);
            return taskSource.Task;
        }

        public static Task Sync(this TaskFactory factory, Action action)
        {
            var taskSource = new TaskCompletionSource<object>();
            action();
            taskSource.SetResult(null);
            return taskSource.Task;
        }

        #endregion
    }
}
