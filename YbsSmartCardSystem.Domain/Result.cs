namespace YbsSmartCardSystem.Domain
{
    public class Result<T>
    {
         public bool IsSuccess { get; set; }
        public bool IsError { get { return !IsSuccess; } }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
