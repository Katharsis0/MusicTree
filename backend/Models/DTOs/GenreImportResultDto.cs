// Models/DTOs/GenreImportResultDto.cs
namespace MusicTree.Models.DTOs
{
    public class GenreImportResultDto
    {
        public int TotalRecords { get; set; }
        public int ImportedRecords { get; set; }
        public int ErrorRecords { get; set; }
        public string ImportedFileName { get; set; } = string.Empty;
        public string ErrorFileName { get; set; } = string.Empty;
        public List<GenreImportErrorDto> Errors { get; set; } = new();
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public class GenreImportErrorDto
    {
        public GenreImportDto? OriginalRecord { get; set; }
        public string ErrorDescription { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
    }
}