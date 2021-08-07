using Event_Creator.models;
using Event_Creator.Other;
using Event_Creator.Other.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {

        private readonly ApplicationContext _appContext;

        public CategoryController(ApplicationContext application)
        {
            this._appContext = application;
        }

        [Authorize(Roles ="Admin")]
        [ServiceFilter(typeof(CsrfActionFilter))]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            if (await _appContext.categories.Where(x => x.CategoryId == category.ParentId).FirstOrDefaultAsync() == null && category.ParentId !=0)
            {
                return BadRequest(Errors.InvalidParentCategory);
            }
            await _appContext.categories.AddAsync(category);
            await _appContext.SaveChangesAsync();
            return Ok("دسته بندی افزوده شد");
        }


        [HttpGet]
        [Route("[action]/{parentId}")]
        public async Task<IActionResult> GetCategoriesByParent(int parentId)
        {
            if(parentId != 0 && await _appContext.categories.Where(x => x.CategoryId==parentId).SingleOrDefaultAsync() == null)
            {
                return BadRequest(Errors.InvalidCategoryName);
            }

            List<Category> categories = await _appContext.categories.Where(x => x.ParentId == parentId).ToListAsync();
            return Ok(categories);
        }

    }
}
