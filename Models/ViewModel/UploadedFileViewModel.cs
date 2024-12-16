namespace VisualHead.Models.ViewModel
{
    public class UploadedFileViewModel : UploadedFile
    {
        public byte[] FileData { get; set; }
        public List<UploadedFile> SystemFiles { get; set; }
    }
}
