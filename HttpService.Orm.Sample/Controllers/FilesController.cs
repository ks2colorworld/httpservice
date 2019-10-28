using HttpService.Orm.Sample.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Orm.Sample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController:ControllerBase
    {
        public FilesController(DefaultDatabaseContext db)
        {
            this.db = db;
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var item = db.Attachments
                .Where(x => x.Attachment_key == id)
                .Select(x => new
                {
                    Name = x.File_name,
                    Size = x.File_size,
                    Type = x.File_format,
                })
                .FirstOrDefault();

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        private readonly DefaultDatabaseContext db;
    }
}
