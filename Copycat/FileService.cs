using System.Drawing;
using System.Drawing.Imaging;

namespace Copycat
{
    public class FileService
    {
        private bool ThumbnailCallback()
        {
            return false;
        }
        public Image GetPreviewThumbnail(string filePath, int thumbnailWidth, int thumbnailHeight)
        {
            using (Image originalImage = Image.FromFile(filePath))
            {
                Image.GetThumbnailImageAbort callback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
                Image thumbnail = originalImage.GetThumbnailImage(thumbnailWidth, thumbnailHeight, callback, IntPtr.Zero);
                return thumbnail;
            }
        }
        public async Task<string> Upload(IFormFile file, string targetDirectory)
        {
            if (file == null || file.Length == 0)
            {
                return null; // Handle empty file
            }

            // Generate a unique file name
            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{fileName}_{Path.GetRandomFileName()}{fileExtension}";

            // Combine the target directory with the unique file name
            string filePath = Path.Combine(targetDirectory, uniqueFileName);

            // Save the file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return uniqueFileName;
        }
        public byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
    }
}
