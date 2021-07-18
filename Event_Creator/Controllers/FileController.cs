using BrunoZell.ModelBinding;
using Event_Creator.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileController : ControllerBase

    {

        
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UploadImage(List<IFormFile> files)
        {
            if (files == null) return BadRequest();
            foreach (var file in files)
            {
                var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images", file.FileName));
                var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);
                stream.Close();
            }
            return Ok();
        }

    }
}
