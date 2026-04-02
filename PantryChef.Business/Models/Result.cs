namespace PantryChef.Business.Models
{
    public record Error(string Message);
    public record Success(); 

    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Data { get; }
        public string ErrorMessage { get; }

        private Result(bool isSuccess, T data, string errorMessage)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T data) => new Result<T>(true, data, null);
        public static Result<T> Failure(string errorMessage) => new Result<T>(false, default, errorMessage);

        public static implicit operator Result<T>(T data) => Success(data);
        public static implicit operator Result<T>(Error error) => Failure(error.Message);
    }

    public class Result
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }

        private Result(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static Result Success() => new Result(true, null);
        public static Result Failure(string errorMessage) => new Result(false, errorMessage);

        public static implicit operator Result(Error error) => Failure(error.Message);
        
        public static implicit operator Result(Success _) => Success(); 
    }
}