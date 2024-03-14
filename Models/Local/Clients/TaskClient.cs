using System.Threading.Tasks;

namespace Utify.Models.Local.Clients
{
    public class TaskCompletedEventArgs : EventArgs
    {
        public object Result { get; set; }
    }

    public class TaskFailedEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public object Result { get; set; }
    }

    public class TaskClient
    {
        public event EventHandler<TaskCompletedEventArgs> TaskCompleted;
        public event EventHandler<TaskFailedEventArgs> TaskFailed;

        public async Task RunTask(Func<Task<object>> taskFunction)
        {
            try
            {
                var result = await taskFunction();
                OnTaskCompleted(new TaskCompletedEventArgs { Result = result });
            }
            catch (Exception ex)
            {
                OnTaskFailed(new TaskFailedEventArgs { Exception = ex });
            }
        }

        protected virtual void OnTaskCompleted(TaskCompletedEventArgs e)
        {
            TaskCompleted?.Invoke(this, e);
        }

        protected virtual void OnTaskFailed(TaskFailedEventArgs e)
        {
            TaskFailed?.Invoke(this, e);
        }
    }
}
