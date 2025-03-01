using StrategyGame.Enums;

namespace StrategyGame.Result
{
    public class TaskResult<T>
    {
        public TaskOutcome TaskOutcome { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public bool Succeeded => TaskOutcome == TaskOutcome.Success;
        public bool Failed => TaskOutcome == TaskOutcome.Fail;

        public TaskResult(TaskOutcome status, string message, T? data)
        {
            TaskOutcome = status;
            Message = message;
            Data = data;
        }
    }
}