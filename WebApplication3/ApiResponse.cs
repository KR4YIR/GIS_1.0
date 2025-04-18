namespace WebApplication3
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Value { get; set; }
    
        public ApiResponse(bool success, string message, T value)
        {
            Success = success;
            Message = message;
            Value = value;
        }
    }   
}
