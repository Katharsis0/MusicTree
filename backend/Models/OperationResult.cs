namespace MusicTree.Models
{
    public class OperationResult
    {
        public bool Success { get; }
        public string Message { get; }
        public string? Id { get; }

        private OperationResult(bool success, string message, string? id)
        {
            Success = success;
            Message = message;
            Id = id;
        }

        public static OperationResult CreateSuccess(string? id = null)
        {
            return new OperationResult(true, string.Empty, id);
        }

        public static OperationResult CreateFailure(string message)
        {
            return new OperationResult(false, message, null);
        }
    }
}