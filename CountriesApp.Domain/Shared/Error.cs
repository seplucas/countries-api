namespace CountriesApp.Domain.Shared;
public record Error(string Code, string Message)
{
    public static Error NotFound(string message) => new("NotFound", message);
    public static Error Validation(string message) => new("ValidationError", message);
    public static Error Conflict(string message) => new("Conflict", message);
    public static Error Unexpected(string message) => new("Unexpected", message);
}