namespace ZgjedhjetApi.Models.DTOs
{
    public class MigrationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RecordsMigrated { get; set; }
        public string? Error { get; set; }
    }
}