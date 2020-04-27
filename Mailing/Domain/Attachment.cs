using System;

namespace Mailing.Domain
{
    public class Attachment
    {
        private Attachment() {}

        public Attachment(byte[] fileContent, string fileName)
        {
            if (fileContent == null || fileContent.Length == 0) throw new ArgumentNullException("Field 'fileContent' can not be null or with length = 0");
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("Field 'to' can not be null or empty");

            FileContent = fileContent;
            FileName = fileName;
        }

        public byte[] FileContent { get; set; }
        public string FileName { get; set; }
    }
}