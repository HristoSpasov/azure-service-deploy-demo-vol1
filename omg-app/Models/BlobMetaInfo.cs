namespace omg_app.Models
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    public class BlobMetaInfo
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string DownloadUrl { get; set; }
    }
}
