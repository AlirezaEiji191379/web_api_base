﻿using Event_Creator.models;
using Event_Creator.Other;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
    public class BookController : ControllerBase
    {
        private readonly ApplicationContext _appContext;

        public BookController(ApplicationContext applicationContext)
        {
            _appContext = applicationContext;
        }


        [Authorize(Roles ="User")]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddBook([FromForm]string bookJson,List<IFormFile> bookImages)
        {

            long volSum = 0;
            if(bookImages ==null || bookImages.Count==0) return BadRequest("حداکثر 4 تصویر و حداقل یک تصویر باید آپلود شود");
            if (bookImages.Count > 4) return BadRequest("حداکثر 4 تصویر و حداقل یک تصویر باید آپلود شود");
            foreach(var file in bookImages)
            {
                if(Path.GetExtension(file.FileName)!=".jpg" && Path.GetExtension(file.FileName) != ".png" && Path.GetExtension(file.FileName) != ".jpeg")
                {
                    return BadRequest(Errors.InvalidFileFormat);
                }
                volSum = volSum + file.Length;
            }
            if (volSum > 10485760) return BadRequest("حجم فایل های ارسالی بیش از 10 مگابایت مجاز نیست");

            try
            {
                Book book = JsonConvert.DeserializeObject<Book>(bookJson);
                if (book.BookName == null || book.BookName=="") return BadRequest("نام کتاب را وارد کنید");
                if (book.PublisherName == null || book.PublisherName == "") return BadRequest("نام ناشر کتاب را وارد کنید");
                if (await _appContext.categories.Where(x => x.CategoryId == book.CategoryId).SingleOrDefaultAsync() == null)
                {
                    return BadRequest("این دسته بندی موجود نمیباشد");
                }
                if(await _appContext.categories.Where(x => x.ParentId == book.CategoryId).FirstOrDefaultAsync() != null)
                {
                    return BadRequest("لطفا دسته بندی درست را انتخاب کنید");
                }
                var now = DateTime.Now;
                var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
                book.addedDate = unixTimeSeconds;
                var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
                var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(stream);
                var tokenS = jsonToken as JwtSecurityToken;
                var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
                //book.user = await _appContext.Users.Where(x => x.UserId == Convert.ToInt64(uid)).SingleAsync();
                book.UserId = Convert.ToInt64(uid);
                book.imageCount = bookImages.Count;
                if(book.exchanges!=null && book.exchanges.Count > 0)
                {
                    foreach (var b in book.exchanges)
                    {
                        if (b.BookName == null || b.BookName == "") return BadRequest("نام کتاب تبادلی را وارد کنید");
                        b.bookToExchange = book;
                    }
                    await _appContext.exchanges.AddRangeAsync(book.exchanges);
                }
                await _appContext.books.AddAsync(book);
                await _appContext.SaveChangesAsync();
                int i = 1;
                foreach (var file in bookImages)
                {
                    string name = book.BookId.ToString() + "_" + i.ToString() + Path.GetExtension(file.FileName).ToLower(); 
                    var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images", name));
                    var fileStream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(fileStream);
                    fileStream.Close();
                    i++;
                }
             
                return Ok();
            }
            catch
            {
               return BadRequest("");
            }
        }
       
        [Authorize(Roles="User")]
        [HttpPut]
        [Route("[action]")]
        public async Task<IActionResult> UpdateBookName([FromBody]UpdateBookNameRequest update)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            Book book = await _appContext.books.Include(x => x.user).Where(x => x.user.UserId == Convert.ToInt64(uid) &&
             x.BookId == update.BookId).SingleOrDefaultAsync();

            if(book == null)
            {
                return NotFound("چنین کتابی موجود نیست");
            }
            book.BookName = update.BookName;
            _appContext.books.Update(book);
            await _appContext.SaveChangesAsync();
            return Ok("نام کتاب تغییر کرد!");
        }

        [Authorize(Roles = "User")]
        [HttpPut]
        [Route("[action]")]
        public async Task<IActionResult> UpdateBookPrice([FromBody] UpdateBookPriceRequest update)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            Book book = await _appContext.books.Include(x => x.user).Where(x => x.user.UserId == Convert.ToInt64(uid) &&
             x.BookId == update.BookId).SingleOrDefaultAsync();

            if (book == null)
            {
                return NotFound("چنین کتابی موجود نیست");
            }
            book.Price = update.Price;
            _appContext.books.Update(book);
            await _appContext.SaveChangesAsync();
            return Ok("قیمت کتاب تغییر کرد");
        }
        

        [Authorize(Roles ="User")]
        [HttpPut]
        [Route("[action]")]
        public async Task<IActionResult> UpdateBookCategory([FromBody]UpdateBookCategoryRequest update)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            Book book = await _appContext.books.Include(x => x.user).Where(x => x.user.UserId == Convert.ToInt64(uid) &&
             x.BookId == update.BookId).SingleOrDefaultAsync();

            if (book == null)
            {
                return NotFound("چنین کتابی موجود نیست");
            }

            if (await _appContext.categories.Where(x => x.CategoryId == update.CategoryId).SingleOrDefaultAsync() == null)
            {
                return BadRequest("این دسته بندی موجود نمیباشد");
            }
            if (await _appContext.categories.Where(x => x.ParentId == update.CategoryId).FirstOrDefaultAsync() != null)
            {
                return BadRequest("لطفا دسته بندی درست را انتخاب کنید");
            }

            book.CategoryId = update.CategoryId;
            _appContext.books.Update(book);
            await _appContext.SaveChangesAsync();
            return Ok("دسته بندی کتاب عوض شد");
        }

        [Authorize(Roles = "User")]
        [HttpPut]
        [Route("[action]")]
        public async Task<IActionResult> UpdateBookExchangeName([FromBody] UpdateExchangeBookNameRequest update)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            Book book = await _appContext.books.Include(x => x.user).Where(x => x.user.UserId == Convert.ToInt64(uid) &&
             x.BookId == update.BookId).SingleOrDefaultAsync();

            if (book == null)
            {
                return NotFound("چنین کتابی موجود نیست");
            }

            Exchange exchangeBook = await _appContext.exchanges.Where(x => x.ExchangeId==update.ExchangeId &&
            x.bookToExchangeId==update.BookId).SingleOrDefaultAsync();

            if(exchangeBook == null)
            {
                return BadRequest("چنین کتاب تبادلی ای وجود ندارد");
            }
            exchangeBook.BookName = update.BookName;
            _appContext.exchanges.Update(exchangeBook);
            await _appContext.SaveChangesAsync();
            return Ok("کتاب تبادلی به روز رسانی شد");
        }

        [Authorize(Roles = "User")]
        [HttpPut]
        [Route("[action]")]
        public async Task<IActionResult> AddExchangeBook([FromBody]AddExchangeBookRequest update)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            Book book = await _appContext.books.Include(x => x.user).Where(x => x.user.UserId == Convert.ToInt64(uid) &&
             x.BookId == update.BookId).SingleOrDefaultAsync();

            if (book == null)
            {
                return NotFound("چنین کتابی موجود نیست");
            }

            update.exchange.bookToExchangeId = update.BookId;
            await _appContext.exchanges.AddAsync(update.exchange);
            await _appContext.SaveChangesAsync();
            return Ok("افزوده شد");
        }
        
        
        [Authorize(Roles = "User")]
        [HttpDelete]
        [Route("[action]/{exchangeId}")]
        public async Task<IActionResult> DeleteExchangeBook(long exchangeId)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;

            Exchange exchange = await _appContext.exchanges.Where(x => x.ExchangeId == exchangeId).SingleOrDefaultAsync();
            if (exchange == null)
            {
                return BadRequest("چنین کتاب تبادلی ای وجود ندارد");
            }
            long bookId = exchange.bookToExchangeId;
            Book book = await _appContext.books.Where(x => x.BookId == bookId && x.UserId == Convert.ToInt64(uid)).SingleOrDefaultAsync();
            if(book == null)
            {
                return BadRequest("چنین کتاب تبادلی ای وجود ندارد");
            }
            _appContext.exchanges.Remove(exchange);
            await _appContext.SaveChangesAsync();
            return Ok("کتاب تبادلی مورد نظر حذف شد");
        }

        




    }
}