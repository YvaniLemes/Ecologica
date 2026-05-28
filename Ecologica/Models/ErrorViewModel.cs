namespace Ecologica.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }   // aceita NULL

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
