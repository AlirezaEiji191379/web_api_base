using BrunoZell.ModelBinding;
using Event_Creator.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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

        [Authorize]
        [HttpPut]
        [Route("[action]")]
        public async Task<IActionResult> UploadProfile(IFormFile profile)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            long userId = Convert.ToInt64(uid);
            string name = userId.ToString() + Path.GetExtension(profile.FileName).ToLower();
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images\users", name));
            var fileStream = new FileStream(path, FileMode.Create);
            await profile.CopyToAsync(fileStream);
            fileStream.Close();
            return Ok("عکس پروفایل اضافه شد");
        }

        public enum Image
        {
            book,
            user
        }

    }
}
