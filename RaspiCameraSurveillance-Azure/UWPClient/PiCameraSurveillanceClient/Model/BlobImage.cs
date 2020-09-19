using System;

namespace PiCameraSurveillanceClient.Model
{
    public class BlobImage
    {
        public DateTimeOffset? LastModified { get; }
        public Uri Uri { get; }

        public string FileName { get; set; }

        public BlobImage(DateTimeOffset? lastModified, Uri uri)
        {
            this.LastModified = lastModified;
            this.Uri = uri;
        }
    }
}