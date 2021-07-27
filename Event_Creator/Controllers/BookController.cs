using Event_Creator.models;
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
        private readonly int limit = 3;
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
                if (book.Publication == 0) { return BadRequest("لظفا سال کتاب را وارد کنید"); }
                if (book.Writer == "" || book.Writer == null) return BadRequest("لطفا نام نویسنده را وارد کنید");
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
                book.Exchangable = false;
                if(book.exchanges!=null && book.exchanges.Count > 0)
                {
                    foreach (var b in book.exchanges)
                    {
                        if (b.BookName == null || b.BookName == "") return BadRequest("نام کتاب تبادلی را وارد کنید");
                        b.bookToExchange = book;
                    }
                    book.Exchangable = true;
                    await _appContext.exchanges.AddRangeAsync(book.exchanges);
                }
                await _appContext.books.AddAsync(book);
                await _appContext.SaveChangesAsync();
                int i = 1;
                foreach (var file in bookImages)
                {
                    string name = book.BookId.ToString() + "_" + i.ToString() + Path.GetExtension(file.FileName).ToLower(); 
                    var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images\books", name));
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

        [Authorize]
        [HttpDelete]
        [Route("[action]/{bookId}")]
        public async Task<IActionResult> DeleteBook(long bookId)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            long userId = Convert.ToInt64(uid);
            Book book = await _appContext.books.Where(x => x.BookId == bookId).SingleOrDefaultAsync();
            if (book == null)
            {
                return BadRequest("چنین کتابی وجود ندارد");
            }

            if(book.UserId != userId)
            {
                User user=await _appContext.Users.Where(x => x.UserId == userId).SingleAsync();
                if (user.role == Role.User)
                {
                    return StatusCode(403, "شما قفط می توانید کتاب های خود را حذف کنید");
                }
            }
            await _appContext.Entry(book).Collection(x => x.exchanges).LoadAsync();
            book.exchanges.Clear();
            _appContext.books.Remove(book);
            List<Bookmark> bookmarks = await _appContext.bookmarks.Include(x => x.book).Where(x => x.book.BookId==bookId).ToListAsync();
            _appContext.bookmarks.RemoveRange(bookmarks);
            await _appContext.SaveChangesAsync();
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Resources\webApi\images\books"));
            List<string> files = Directory.GetFiles(path).Where(x => x.Contains(bookId.ToString())).ToList();
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }
            return Ok("کتاب مورد نظر حذف شد");
        }


        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public IActionResult GetAllBooksByUserId()
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            long userId = Convert.ToInt64(uid);
            Book.jsonStatus = JsonStatus.DisableUserAndCategory;
            var allBooks = _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.UserId == userId).ToList();
            return Ok(allBooks);
        }

        [HttpGet]
        [Route("[action]/{bookId}")]
        public async Task<IActionResult> GetBookById(long bookId)
        {
            Book book = await _appContext.books.Where(x => x.BookId==bookId).SingleOrDefaultAsync();
            if (book == null)
            {
                return BadRequest("چنین کتابی وجود ندارد");
            }
           // Book.jsonStatus = JsonStatus.EnableUserAndCategory;
            book.views = book.views + 1;
            await _appContext.Entry(book).Collection(x => x.exchanges).LoadAsync();
            await _appContext.Entry(book).Reference(x => x.user).LoadAsync();
            await _appContext.Entry(book).Reference(x => x.Category).LoadAsync();
            _appContext.books.Update(book);
            await _appContext.SaveChangesAsync();
            return Ok(book);
        }

        [HttpGet]
        [Route("[action]/{categoryId}")]
        public async Task<IActionResult> GetAllBooksByCategory(long categoryId,Status status,Sort sort,double min_price=-1,double max_price=-1,int index=1)
        {

            int skip = (index - 1) * limit;
            List<Book> books = null;
            if(status == Status.all)
            {
                if (sort == Sort.price)
                {
                    if (min_price == -1 && max_price == -1) { 
                       if(categoryId!=0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                       else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                    }
                    else if (min_price != -1 && max_price == -1) {
                       if(categoryId!=0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && x.Price >= min_price).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync(); 
                       else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                    }
                    else if (max_price == -1 && max_price != -1) { 
                      if(categoryId!=0)  books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && max_price >= x.Price).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                    }
                    else {
                      if(categoryId!=0)  books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && x.Price >= min_price && max_price >= x.Price).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                      else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                    }

                }
                else
                {
                    if (min_price == -1 && max_price == -1) { 
                      if(categoryId!=0)  books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync(); 
                      else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                    }
                    else if (min_price != -1 && max_price == -1) {
                        if (categoryId != 0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && x.Price >= min_price).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                    }
                    else if (max_price == -1 && max_price != -1) {
                        if (categoryId != 0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && max_price >= x.Price).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                    }
                    else {
                        if (categoryId != 0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && x.Price >= min_price && max_price >= x.Price).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                    }

                }
            }
            else
            {
                if (sort == Sort.price)
                {
                    if (min_price == -1 && max_price == -1) {
                        if (categoryId != 0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && x.Exchangable == true).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                    }
                    else if (min_price != -1 && max_price == -1) {
                        if (categoryId != 0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && x.Price >= min_price && x.Exchangable == true).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                    }
                    else if (max_price == -1 && max_price != -1) {
                        if (categoryId != 0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && max_price >= x.Price && x.Exchangable == true).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                    }
                    else {
                        if (categoryId != 0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && x.Price >= min_price && max_price >= x.Price && x.Exchangable == true).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.Price).Skip(skip).Take(limit).ToListAsync();
                    }
                }
                else
                {
                    if (min_price == -1 && max_price == -1) {
                        if (categoryId != 0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && x.Exchangable == true).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                    }
                    else if (min_price != -1 && max_price == -1) {
                        if (categoryId != 0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && x.Price >= min_price && x.Exchangable == true).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                    }
                    else if (max_price == -1 && max_price != -1) {
                        if (categoryId != 0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && max_price >= x.Price && x.Exchangable == true).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                    }
                    else {
                        if (categoryId != 0) books = await _appContext.books.Include(x => x.Category).Include(x => x.user).Where(x => x.CategoryId == categoryId && x.Price >= min_price && max_price >= x.Price && x.Exchangable == true).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                        else books = await _appContext.books.Include(x => x.Category).Include(x => x.user).OrderByDescending(x => x.addedDate).Skip(skip).Take(limit).ToListAsync();
                    }
                }
            }
            return Ok(books);
        }

        [HttpPut]
        [Authorize]
        [Route("[action]/{bookId}")]
        public async Task<IActionResult> Bookmark(long bookId)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            long userId = Convert.ToInt64(uid);
            Book book = await _appContext.books.Where(x => x.BookId==bookId).SingleOrDefaultAsync();
            if(book == null)
            {
                return BadRequest("چنین کتابی موجود نیست");
            }
            Bookmark mark = await _appContext.bookmarks.Include(x => x.book).Where(x => x.userId == userId && x.book.BookId == bookId).SingleOrDefaultAsync();
            if (mark != null) return BadRequest("این کتاب قبلا ذخیره شده است");
            Bookmark bookmark = new Bookmark()
            {
                book=book,
                userId=userId
            };
            book.bookmarks = book.bookmarks + 1;
            _appContext.books.Update(book);
            await _appContext.bookmarks.AddAsync(bookmark);
            await _appContext.SaveChangesAsync();
            return Ok("کتاب به لیست افزوده شد");
        }  

        [HttpGet]
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> GetUserBookmarks()
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            long userId = Convert.ToInt64(uid);
            User user = await _appContext.Users.Where(x => x.UserId == userId).SingleAsync();
            await _appContext.Entry(user).Collection(x => x.bookmarks).LoadAsync();
            foreach(var mark in user.bookmarks)
            {
                await _appContext.Entry(mark).Reference(x => x.book).LoadAsync();
                await _appContext.Entry(mark.book).Reference(x => x.Category).LoadAsync();
                await _appContext.Entry(mark.book).Reference(x => x.user).LoadAsync();
            }
            return Ok(user.bookmarks);
        }

        [HttpDelete]
        [Authorize]
        [Route("[action]/{bookId}")]
        public async Task<IActionResult> DeleteBookmark(long bookId)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            long userId = Convert.ToInt64(uid);
            Bookmark bookmark = await _appContext.bookmarks.Include(x => x.book).Where(x => x.userId == userId && x.book.BookId == bookId).SingleOrDefaultAsync();
            if (bookmark == null) return BadRequest("چنین کتابی ذخیره نشده است");
            _appContext.bookmarks.Remove(bookmark);
            await _appContext.SaveChangesAsync();
            return Ok("کتاب مورد نظر حذف شد");
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> AddBuyer([FromBody] BookBuySellRequest request)
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            long userId = Convert.ToInt64(uid);
            Book book = await _appContext.books.Where(x => x.UserId == userId && x.BookId == request.BookId).SingleOrDefaultAsync();
            if(book == null)
            {
                return StatusCode(404,"چنین کتابی وجود ندارد");
            }
            if (book.sellStatus !=SellStatus.none)
            {
                return StatusCode(403,"این کتاب قبلا فروخته شده است");
            }
            if(request.username !="" && request.username != null)
            {
                User user = await _appContext.Users.Where(x => x.Username == request.username).SingleOrDefaultAsync();
                if (user == null)
                {
                    return StatusCode(404, "چنین کاربری وجود ندارد");
                }
                if(user.UserId == userId)
                {
                    return StatusCode(403,"شما نمیتوانید کتاب را به خودتان بفروشید");
                }
                book.buyerId = user.UserId;
                book.sellStatus = SellStatus.AuthenticatedBuyer;
            }
            else
            {
                book.sellStatus = SellStatus.unAuthenticatedBuyer;
            }
            _appContext.books.Update(book);
            await _appContext.SaveChangesAsync();
            return Ok("کتاب به لیست فروخته شده ها افزوده شد");
        }

        [HttpGet]
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> GetBoughtBooks()
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            long userId = Convert.ToInt64(uid);
            List<Book> bought = await _appContext.books.Where(x => x.buyerId == userId).ToListAsync();
            return Ok(bought);
        }

        [HttpGet]
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> GetSoldBooks()
        {
            var authorizationHeader = Request.Headers.Single(x => x.Key == "Authorization");
            var stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            long userId = Convert.ToInt64(uid);
            Book.jsonStatus = JsonStatus.DisableUserAndCategory;
            List<Book> sold = await _appContext.books.Where(x => x.UserId == userId && x.sellStatus != SellStatus.none).ToListAsync();
            return Ok(sold);
        }
        

        public enum Status
        {
            all,
            exchange
        }

        public enum Sort
        {
            time,
            price
        }
    }
}
