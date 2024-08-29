using System.ComponentModel.DataAnnotations;

namespace Fantasy.Shared.ViewsModels
{
    public class FileModel
    {
        [Required(ErrorMessage = "This field is required")]
        [MinLength(5, ErrorMessage = "5 characters is required")]
        public string FileBase64 { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        [MinLength(5, ErrorMessage = "5 characters is required")]
        public string FileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        [MinLength(5, ErrorMessage = "5 characters is required")]
        public string FolderName { get; set; } = string.Empty;
    }
}