using BrunoZell.ModelBinding;
using Event_Creator.models;
using Event_Creator.Other.Filters;
using Event_Creator.Other.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationContext _appContext;
        private readonly IJwtService _jwtService;

        public FileController(ApplicationContext context, IJwtService jwtService)
        {
            _appContext = context;
            _jwtService = jwtService;
        }

        [HttpGet]
        [Route("[action]/{imageId}")]
        public IActionResult DownloadImage(string imageId, Image kind)
        {
            string path = null;
            if (kind == Image.book) path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images\books"));
            else path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images\users"));
            string[] files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (file.Contains(imageId))
                {
                    var image = System.IO.File.OpenRead(file);
                    string content_Type = "image/" + Path.GetExtension(file).Substring(1);
                    return File(image, content_Type);
                }
            }
            if (kind == Image.user) { 
                return File(System.IO.File.OpenRead(path + "\\user.png"), "image/png");
            }
            else if (kind == Image.book){ 
                return File(System.IO.File.OpenRead(path + "\\empty.jpg"), "image/jpg");
            }
            return NotFound();
            //string downloadPath = Directory.GetFiles(path).Where(x => x.Contains(imageId.ToString())).FirstOrDefault();
            ////Console.WriteLine(downlaodPath);

            ////if (downlaodPath == null) return NotFound("چنین فایلی موجود نیست");
            //var image = System.IO.File.OpenRead(downloadPath);
            //string content_Type = "image/" + Path.GetExtension(downloadPath).Substring(1);
            //return File(image, content_Type);
        }

        [Authorize]
        [HttpPut]
        [ServiceFilter(typeof(CsrfActionFilter))]
        [Route("[action]")]
        public async Task<IActionResult> UploadProfile(IFormFile profile)
        {
            long userId = _jwtService.getUserIdFromJwt(HttpContext);
            string name = userId.ToString() + Path.GetExtension(profile.FileName).ToLower();
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images\users", name));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            var fileStream = new FileStream(path, FileMode.Create);
            await profile.CopyToAsync(fileStream);
            fileStream.Close();
            return Ok("عکس پروفایل اضافه شد");
        }

        [Authorize]
        [HttpPut]
        [ServiceFilter(typeof(CsrfActionFilter))]
        [Route("[action]/{bookId}")]
        public async Task<IActionResult> AddBookImage(IFormFile image,long bookId)
        {
            long userId = _jwtService.getUserIdFromJwt(HttpContext);
            Book book = await _appContext.books.Where(x => x.BookId == bookId && x.UserId == userId).SingleOrDefaultAsync();
            if (book == null) return StatusCode(404,"چنین کتابی موجود نیست");
            if (book.imageCount == 4) return StatusCode(403,"تعداد تصاویر حداکثر 4 تا میباشد");
            int index = book.imageCount + 1;
            string name = book.BookId.ToString() + "_" + index.ToString() + Path.GetExtension(image.FileName).ToLower();
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images\books", name));
            var fileStream = new FileStream(path, FileMode.Create);
            await image.CopyToAsync(fileStream);
            fileStream.Close();
            book.imageCount = book.imageCount + 1;
            _appContext.books.Update(book);
            await _appContext.SaveChangesAsync();
            return Ok("تصویر جدید اضافه شد");
        }
        
        [Authorize]
        [HttpDelete]
        [ServiceFilter(typeof(CsrfActionFilter))]
        [Route("[action]/{bookId}/{imageId}")]
        public async Task<IActionResult> DeleteBookImage(long bookId, int imageId)
        {
            long userId = _jwtService.getUserIdFromJwt(HttpContext);
            Book book = await _appContext.books.Where(x => x.BookId == bookId && x.UserId == userId).SingleOrDefaultAsync();
            if (book == null) return StatusCode(404, "چنین کتابی موجود نیست");
            string name = book.BookId.ToString() + "_" + imageId.ToString();
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images\books"));
            List<string> files = Directory.GetFiles(path).Where(x => x.Contains(bookId.ToString())).ToList();
            string deletePath = files.Where(x => x.Contains(name)).SingleOrDefault();
            if(deletePath ==null) return StatusCode(404, "چنین فایلی وجود ندارد");
            files.Remove(deletePath);
            System.IO.File.Delete(deletePath);
            for (int i = 0; i < files.Count; i++)
            {
                 System.IO.File.Move(files[i],Path.Combine(path,bookId.ToString()+"_"+(i+1)+Path.GetExtension(files[i])));
            }
             book.imageCount--;
             _appContext.books.Update(book);
             await _appContext.SaveChangesAsync();
            return Ok("تصویر مورد نظر پاک شد");
        }


        [Authorize]
        [HttpDelete]
        [ServiceFilter(typeof(CsrfActionFilter))]
        [Route("[action]")]
        public IActionResult DeleteProfileImage()
        {
            long userId = _jwtService.getUserIdFromJwt(HttpContext);
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images\users"));
            string deletePath = Directory.GetFiles(path).Where(x => x.Contains(userId.ToString())).SingleOrDefault();
            if(deletePath == null) return StatusCode(404, "چنین عکسی وجود ندارد");
            System.IO.File.Delete(deletePath);
            return Ok("تصویر مورد نظر پاک شد");
        }
        


        public enum Image
        {
            book,
            user
        }

    }
}
