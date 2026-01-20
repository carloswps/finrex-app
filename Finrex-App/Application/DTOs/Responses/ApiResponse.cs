namespace Finrex_App.Application.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; private set; }
    public T Data { get; private set; }
    public string Message { get; private set; }
    public List<string> Errors { get; private set; }

    private ApiResponse(T data, string message = null)
    {
        Success = true;
        Data = data;
        Message = message;
        Errors = new List<string>();
    }

    private ApiResponse(List<string> errors, string message)
    {
        Success = false;
        Errors = errors;
        Message = message;
        Data = default;
    }

    public static ApiResponse<T> CreateSuccess(T data, string message = "Operação realizada com sucesso.")
    {
        return new ApiResponse<T>(data, message);
    }

    public static ApiResponse<T> CreateFailure(string errorMessage, List<string> erros = null)
    {
        var errorList = erros ?? new List<string> { errorMessage };
        return new ApiResponse<T>(errorList, errorMessage);
    }
}