namespace IFCConverto.Models
{
    // Model class for tranferring data between Settings page and settings service
    public class AppSettings
    {
        public string ServerURL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string BucketName { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}
