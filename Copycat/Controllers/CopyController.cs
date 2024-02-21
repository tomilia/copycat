using System.Net;
using Copycat.Model;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Copycat.Controllers;

[ApiController]
[Route("[controller]")]
public class CopyController : ControllerBase
{
    private readonly FileService _fileUploadService;
    private readonly ILogger<CopyController> _logger;
    private readonly string _filePath = "/FileUpload";
    private IConnectionMultiplexer _conn;
    private readonly IDatabase _redis;

    public CopyController(ILogger<CopyController> logger, IConnectionMultiplexer redis, FileService fileUpload)
    {
        _logger = logger;
        _redis = redis.GetDatabase();
        _conn = redis;
        _fileUploadService = fileUpload;

    }

    [HttpPost]
    [Route("AddWord")]
    public ActionResult SetNewWord(string word)
    {
        var expirationTime = DateTime.UtcNow.AddDays(1);
        var currentTimestamp = DateTime.Now.ToString();

        _redis.StringSet(currentTimestamp, word);
        // Set the expiration time on the key
        _redis.KeyExpireAsync(currentTimestamp, expirationTime).GetAwaiter().GetResult();

        return Ok();
    }
    [HttpGet]
    [Route("GetAllWordsOrderByTime")]

    public async Task<string[]> GetAllWordsOrderByTime()
    {
        EndPoint endPoint = _conn.GetEndPoints().First();
        RedisKey[] keys = _conn.GetServer(endPoint)
            .Keys(pattern: "*")
            .OrderBy(e => DateTime.Parse(e.ToString()))
            .ToArray();

        return (await _redis.StringGetAsync(keys))
            .Select(e => e.ToString())
            .ToArray();
    }
    [HttpGet]
    [Route("GetVPN")]
    public IActionResult GetVPN(string filePath)
    {
        // Validate the filePath parameter if necessary

        // Get the file name from the filePath
        string fileName = "E:\\static\\"+filePath;

        // Set the content type based on the file extension
        string contentType = "application/x-openvpn-profile";

        // Download the file
        return PhysicalFile(fileName, contentType, filePath);
    }
    [HttpPost("Upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        string targetDirectory = _filePath;

        string fileName = await _fileUploadService.Upload(file, targetDirectory);

        if (fileName == null)
        {
            // Handle file upload failure
            return BadRequest("Invalid file");
        }

        // Handle successful file upload
        return Ok("File uploaded successfully");
    }
    [HttpGet("File/{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        // Generate the file path based on the provided file name
        string filePath = Path.Combine(_filePath, fileName);

        // Check if the file exists
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        // Read the file into a byte array
        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

        // Set the content type and file name for the response
        string contentType = "application/octet-stream";
        string fileDownloadName = fileName;

        // Return the file as a byte array in the response
        return File(fileBytes, contentType, fileDownloadName);
    }
    [HttpGet("GetFiles")]
    public IActionResult GetFiles()
    {
        List<FileModel> file = new();

        // Get a list of file names in the specified directory
        
        FileModel[] fileNames = Directory.GetFiles(_filePath)
            .Select(f =>
            {
                string ext = Path.GetExtension(f);
                return new FileModel
                {
                    Name = Path.GetFileName(f),
                    Preview = ext == ".jpeg" || ext == ".png" || ext == ".jpg" ? _fileUploadService.ImageToByteArray(_fileUploadService.GetPreviewThumbnail(f, 200, 200)) : null
                };
            }
            )
            .ToArray();
        
        return Ok(fileNames);
    }
    [HttpPost("File/Delete/{filename}")]
    public IActionResult Delete(string filename)
    {
        try
        {
            string filePath = Path.Combine(_filePath, filename);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            System.IO.File.Delete(filePath);

            return Ok("File deleted successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}

