namespace ImageUpWpf.Core
{
    public class TaskGroupCreatingResult
    {
        /// <summary>
        /// Full file name
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// If false, means success
        /// </summary>
        public bool HasError { get; set; }
        /// <summary>
        /// Error Message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// If success, TaskItem will be set
        /// </summary>
        public TaskGroup TaskGroup { get; set; }
    }
}