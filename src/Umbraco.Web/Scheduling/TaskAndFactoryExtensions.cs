using System;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.Scheduling
{
    internal static class TaskAndFactoryExtensions
    {
        #region Task Extensions

        static void SetCompletionSource<TResult>(TaskCompletionSource<TResult> completionSource, Task task)
        {
            if (task.IsFaulted)
                completionSource.SetException(task.Exception.InnerException);
            else
                completionSource.SetResult(default(TResult));
        }

        static void SetCompletionSource<TResult>(TaskCompletionSource<TResult> completionSource, Task<TResult> task)
        {
            if (task.IsFaulted)
                completionSource.SetException(task.Exception.InnerException);
            else
                completionSource.SetResult(task.Result);
        }

        public static Task ContinueWithTask(this Task task, Func<Task, Task> continuation)
        {
            var completionSource = new TaskCompletionSource<object>();
            task.ContinueWith(atask => continuation(atask).ContinueWith(atask2 => SetCompletionSource(completionSource, atask2)));
            return completionSource.Task;
        }

        public static Task ContinueWithTask(this Task task, Func<Task, Task> continuation, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<object>();
            task.ContinueWith(atask => continuation(atask).ContinueWith(atask2 => SetCompletionSource(completionSource, atask2), token), token);
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