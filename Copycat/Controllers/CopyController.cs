using System.Net;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Copycat.Controllers;

[ApiController]
[Route("[controller]")]
public class CopyController : ControllerBase
{

    private readonly ILogger<CopyController> _logger;

    private IConnectionMultiplexer _conn;
    private readonly IDatabase _redis;

    public CopyController(ILogger<CopyController> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis.GetDatabase();
        _conn = redis;
    }

    [HttpPost(Name = "AddWord")]
    public ActionResult SetNewWord(string word)
    {
        var expirationTime = DateTime.UtcNow.AddDays(1);
        var currentTimestamp = DateTime.Now.ToString();

        _redis.StringSet(currentTimestamp, word);
        // Set the expiration time on the key
        _redis.KeyExpireAsync(currentTimestamp, expirationTime).GetAwaiter().GetResult();

        return Ok();
    }
    [HttpGet(Name = "GetAllWordsOrderByTime")]
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
}

