namespace Wmi.Api.Models;

public class Result<T>
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? ErrorDetails { get; set; }
    public T? Value { get; set; }

    public static Result<T> Ok(T value) => new Result<T> { Success = true, Value = value };

    public static Result<T> Fail(string error) => new Result<T> { Success = false, Error = error };

    public static Result<T> Fail(string error, string? errorDetails) => new Result<T>
        { Success = false, Error = error, ErrorDetails = errorDetails };

}