using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mongoback;
using MongoDB.Bson;

namespace mongofront.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CatchAll : ControllerBase
    {
        private readonly ILogger<CatchAll> _logger;

        public CatchAll(ILogger<CatchAll> logger)
        {
            _logger = logger;
        }

        [HttpGet("{*path}")]
        public IEnumerable<string> Get(string path)
        {
            return MyMongo.Instance.GetResources(path, HttpContext.Session.Id).Select(i => i.ToJson());
        }
    }

    public static class MyMongo
    {
        public static MyMongoService Instance = new MyMongoService("mongodb://localhost", isTest: false);
    }
}
