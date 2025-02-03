namespace IoT.Common
{
    public class Result<T>
    {
        public bool IsSucceded { get; }

        public T? Data { get; }

        public string? ErrorMessage { get; }

        private Result(bool isSucceded, T? data, string? errorMessage)
        {
            IsSucceded = isSucceded;
            Data = data;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T data) => new(true, data, null);

        public static Result<T> Failure(string? errorMessage = default) => new(false, default, errorMessage);
    }
}
