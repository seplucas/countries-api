namespace CountriesApp.Domain.Shared;

public abstract record Result
{
    public bool IsSuccess { get; init; }
    public Error? Error { get; init; }
    
    public static implicit operator bool(Result result) => result.IsSuccess;

    public static Result Success() => new SuccessResult();
    public static Result Failure(Error error) => new FailureResult(error);
}

public sealed record SuccessResult : Result
{
    public SuccessResult()
    {
        IsSuccess = true;
        Error = null;
    }
}

public sealed record FailureResult : Result
{
    public FailureResult(Error error)
    {
        IsSuccess = false;
        Error = error;
    }
}

public abstract record Result<T> : Result
{
    public T? Value { get; init; }

    public static Result<T> Success(T value) => new SuccessResult<T>(value);
    public static Result<T> Failure(Error error) => new FailureResult<T>(error);

    public static implicit operator T?(Result<T> result) => result.IsSuccess ? result.Value : default;
}

public sealed record SuccessResult<T> : Result<T>
{
    public SuccessResult(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = null;
    }
}

public sealed record FailureResult<T> : Result<T>
{
    public FailureResult(Error error)
    {
        IsSuccess = false;
        Error = error;
        Value = default;
    }
}

