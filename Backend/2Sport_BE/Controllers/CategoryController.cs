﻿using _2Sport_BE.Infrastructure.Services;
using _2Sport_BE.Repository.Interfaces;
using _2Sport_BE.Repository.Models;
using _2Sport_BE.Service.Services;
using _2Sport_BE.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _2Sport_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ISportService _sportService;
        private readonly IWarehouseService _warehouseService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CategoryController(ICategoryService categoryService, IUnitOfWork unitOfWork,
									IProductService productService,
                                    IWarehouseService warehouseService,
                                    IMapper mapper, 
                                    ISportService sportService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _warehouseService = warehouseService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _sportService = sportService;
        }

        [HttpGet]
        [Route("list-categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var query = await _categoryService.GetAllCategories();
                var warehouses = (await _warehouseService.GetWarehouse(_ => _.TotalQuantity > 0)).Include(_ => _.Product).ToList();
                foreach (var item in warehouses)
                {
                    item.Product = await _productService.GetProductById((int)item.ProductId);
                }
                foreach (var item in query.ToList())
				{
                    item.Sport = await _sportService.GetSportById(item.SportId);
                    item.Quantity = 0;
                    foreach (var productInWarehouse in warehouses)
                    {
                        if (productInWarehouse.Product.CategoryId == item.Id)
                        {
                            item.Quantity += 1;
                        }
                    }
                }
				var categories = _mapper.Map<List<CategoryVM>>(query.ToList());
                return Ok(new { total = categories.Count, data = categories });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }


        [HttpGet]
        [Route("get-category-by-id/{categoryId}")]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            try
            {
                var category = await _categoryService.GetCategoryById(categoryId);
                var result = _mapper.Map<CategoryVM>(category);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        [Route("add-category")]
        public async Task<IActionResult> AddCategory(CategoryCM newCategoryCM)
        {
            try
            {
                var newCategory = _mapper.Map<Category>(newCategoryCM);
                await _categoryService.AddCategory(newCategory);
                return Ok("Add new category successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("add-categories")]
        public async Task<IActionResult> AddCategories(List<CategoryCM> newCategoryCMs)
        {
            try
            {
                var newCategories = _mapper.Map<List<Category>>(newCategoryCMs);
                await _categoryService.AddCategories(newCategories);
                return Ok("Add new categories successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut]
        [Route("update-category/{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, CategoryUM categoryUM)
        {
            try
            {
                var updatedCategory = await _categoryService.GetCategoryById(categoryId);
                updatedCategory.CategoryName = categoryUM.CategoryName;
                updatedCategory.SportId = categoryUM.SportId;
                updatedCategory.Sport = await _sportService.GetSportById(categoryUM.SportId);
                updatedCategory.Description = categoryUM.Description;
                await _categoryService.UpdateCategory(updatedCategory);
                return Ok(updatedCategory);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete]
        [Route("active-deactive-category/{categoryId}")]
        public async Task<IActionResult> ActiveDeactiveCategory(int categoryId)
        {
            try
            {
                var deletedCategory = await _categoryService.GetCategoryById(categoryId);
                if (deletedCategory.Status == true)
                {
                    deletedCategory.Status = false;
                    await _categoryService.UpdateCategory(deletedCategory);
                    return Ok("Deactive successfully");
                }
                else
                {
                    deletedCategory.Status = true;
                    await _categoryService.UpdateCategory(deletedCategory);
                    return Ok("Active successfully");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
