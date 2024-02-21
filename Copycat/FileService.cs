

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
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
            Image originalImage = Image.Load(filePath);
                originalImage.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(thumbnailWidth, thumbnailHeight),
                    Mode = ResizeMode.Max
                }));
                return originalImage;
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
        public string ImageToByteArray(Image image)
        {
            string base64String;
            using (var ms = new MemoryStream())
            {
                image.Save(ms, new JpegEncoder());
                byte[] imageBytes = ms.ToArray();
                base64String = Convert.ToBase64String(imageBytes);
                imageBytes = null;
            }
            return base64String;
        }
    }
}
