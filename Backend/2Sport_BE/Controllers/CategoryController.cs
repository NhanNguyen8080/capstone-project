﻿using _2Sport_BE.Infrastructure.Services;
using _2Sport_BE.Repository.Interfaces;
using _2Sport_BE.Repository.Models;
using _2Sport_BE.Service.Services;
using _2Sport_BE.Services;
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
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CategoryController(ICategoryService categoryService, IUnitOfWork unitOfWork,
									IProductService productService,
                                    IWarehouseService warehouseService,
                                    IImageService imageService,
                                    IMapper mapper, 
                                    ISportService sportService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _warehouseService = warehouseService;
            _imageService = imageService;
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
                var warehouses = (await _warehouseService.GetAvailableWarehouse());
                var products = new List<Product>();
                foreach (var item in warehouses)
                {
                    products.Add(await _productService.GetProductByProductCode(item.ProductCode));
                }
                foreach (var item in query.ToList())
				{
                    item.Sport = await _sportService.GetSportById(item.SportId);
                    item.Quantity = 0;
                    foreach (var product in products)
                    {
                        if (product.CategoryId == item.Id && product.Status == true)
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
                if (newCategoryCM.CategoryImage != null)
                {
                    var uploadResult = await _imageService.UploadImageToCloudinaryAsync(newCategoryCM.CategoryImage);
                    if (uploadResult != null && uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        newCategory.CategoryImgPath = uploadResult.SecureUrl.AbsoluteUri;
                    }
                    else
                    {
                        return BadRequest("Something wrong!");
                    }
                }
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
        public async Task<IActionResult> AddCategories([FromForm] CategoryCM[] newCategoryCMs)
        {
            try
            {
                // Log the incoming request body for debugging
                var requestBody = Request.Form;
                Console.WriteLine("Request Body: " + requestBody);

                foreach (var newCategoryCM in newCategoryCMs)
                {
                    var newCategory = _mapper.Map<Category>(newCategoryCM);
                    if (newCategoryCM.CategoryImage != null)
                    {
                        var uploadResult = await _imageService.UploadImageToCloudinaryAsync(newCategoryCM.CategoryImage);
                        if (uploadResult != null && uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            newCategory.CategoryImgPath = uploadResult.SecureUrl.AbsoluteUri;
                        }
                        else
                        {
                            return BadRequest("Something wrong!");
                        }
                    }
                    await _categoryService.AddCategory(newCategory);

                }
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
                if (categoryUM.CategoryImage != null)
                {
                    var uploadResult = await _imageService.UploadImageToCloudinaryAsync(categoryUM.CategoryImage);
                    if (uploadResult != null && uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        updatedCategory.CategoryImgPath = uploadResult.SecureUrl.AbsoluteUri;
                    }
                    else
                    {
                        return BadRequest("Something wrong!");
                    }
                }
                await _categoryService.UpdateCategory(updatedCategory);
                return Ok(updatedCategory);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut]
        [Route("edit-status/{categoryId}")]
        public async Task<IActionResult> EditStatus(int categoryId)
        {
            try
            {
                var updatedCategory = await _categoryService.GetCategoryById(categoryId);
                updatedCategory.Status = !updatedCategory.Status;
                await _categoryService.UpdateCategory(updatedCategory);
                return Ok($"Status of {updatedCategory.CategoryName} edited to {updatedCategory.Status}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete]
        [Route("delete-category/{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                await _categoryService.DeleteCategoryById(categoryId);
                return Ok("Deleted category successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
