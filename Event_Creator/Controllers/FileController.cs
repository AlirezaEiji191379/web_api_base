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

        
        [HttpGet]
        [Route("[action]/{imageId}")]
        public IActionResult DownloadImage(string imageId,Image kind)
        {
            string path = null;
            if(kind==Image.book) path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images\books"));
            else path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images\users"));
            string[] files = Directory.GetFiles(path);
            foreach(var file in files)
            {
                if (file.Contains(imageId))
                {
                    var image = System.IO.File.OpenRead(file);
                    string content_Type = "image/" + Path.GetExtension(file).Substring(1);
                    return File(image , content_Type);
                }
            }
            return NotFound();

        }

        public enum Image
        {
            book,
            user
        }

    }
}
