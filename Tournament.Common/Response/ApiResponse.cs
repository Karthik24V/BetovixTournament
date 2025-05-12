namespace Tournament.Common.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "OK";
        public T Data { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "OK")
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = message };
        }

        public static ApiResponse<T> FailureResponse(string message)
        {
            return new ApiResponse<T> { Success = false, Message = message };
        }
    }
}
