﻿using _2Sport_BE.DataContent;
using _2Sport_BE.Repository.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using _2Sport_BE.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using _2Sport_BE.Service.Services;
using _2Sport_BE.ViewModels;
using AutoMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using _2Sport_BE.Services;
using System.Security.Claims;
using _2Sport_BE.Service.Enums;

namespace _2Sport_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly IEmployeeDetailService _employeeDetailService;
        private readonly IUserService _userService;
        private readonly IImageService _imageService;
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;
        public BrandController(IBrandService brandService, IProductService productService, 
                               IWarehouseService warehouseService,
                               IImageService imageService,
                               IEmployeeDetailService employeeDetailService,
                               IUserService userService,
                               IEmployeeService employeeService,
                               IMapper mapper)
        {
            _brandService = brandService;
            _productService = productService;
            _warehouseService = warehouseService;
            _imageService = imageService;
            _mapper = mapper;
            _userService = userService;
            _employeeDetailService = employeeDetailService;
            _employeeService = employeeService;
        }
        [HttpGet]
        [Route("list-all")]
        public async Task<IActionResult> ListAllAsync()
        {
            try
            {
                var brands = await _brandService.ListAllAsync();
                var warehouses = (await _warehouseService.GetWarehouse(_ => _.TotalQuantity > 0)).Include(_ => _.Product).ToList();
                foreach (var item in warehouses)
                {
                    item.Product = await _productService.GetProductById((int)item.ProductId);
                }

                foreach (var item in brands.ToList())
                {
                    item.Quantity = 0;
                    foreach (var productInWarehouse in warehouses)
                    {
                        if (productInWarehouse.Product.BrandId == item.Id)
                        {
                            item.Quantity += 1;
                        }
                    }
                }
                var result = _mapper.Map<List<BrandVM>>(brands.ToList());
                return Ok(new { total = result.Count(), data = result });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }


        [HttpGet]
        [Route("get-brand-by-id/{brandId}")]
        public async Task<IActionResult> GetBrandById(int brandId)
        {
            try
            {
                var brand = await _brandService.GetBrandById(brandId);
                var result = _mapper.Map<BrandVM>(brand);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        [Route("add-brand")]
        public async Task<IActionResult> AddBrand(BrandCM brandCM)
        {
            var addedBrand = _mapper.Map<Brand>(brandCM);
            var addedBrandBranch = new BrandBranch();
            try
            {
                
                var userId = GetCurrentUserIdFromToken();

                if (userId == 0)
                {
                    return Unauthorized();
                }

                var employee = await _employeeService.GetEmployeeDetailsById(userId);
                var employeeDetail = await _employeeDetailService.GetEmployeeDetailByEmployeeId(userId);

                //Add brand under manager role
                if (employeeDetail != null && employee.Data.RoleId == 2)
                {
                    var existedBrand = (await _brandService.GetBrandsAsync(Name)).FirstOrDefault();
                    if (existedBrand != null)
                    {
                        addedBrandBranch.BrandId = existedBrand.Id;
                        addedBrandBranch.BranchId = employeeDetail.BranchId;
                        await _brandService.AssignAnOldBrandToBranchAsync(addedBrandBranch);
                        return Ok("Add Brand successfully!");
                    }

                    addedBrandBranch.BranchId = employeeDetail.BranchId;
                }

                if (brandCM.LogoImage != null)
                {
                    var uploadResult = await _imageService.UploadImageToCloudinaryAsync(brandCM.LogoImage);
                    if (uploadResult != null && uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        addedBrand.Logo = uploadResult.SecureUrl.AbsoluteUri;
                    }
                    else
                    {
                        return BadRequest("Something wrong!");
                    }
                }
                await _brandService.CreateANewBrandAsync(addedBrand, addedBrandBranch);
                return Ok("Add brand successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("update-brand/{brandId}")]
        public async Task<IActionResult> UpdateBrand(int brandId, BrandUM brandUM)
        {
            var updatedBrand = (await _brandService.GetBrandById(brandId)).FirstOrDefault();
            if (updatedBrand != null)
            {
                updatedBrand.BrandName = brandUM.BrandName;
                try
                {
                    if (brandUM.LogoImage != null)
                    {
                        var uploadResult = await _imageService.UploadImageToCloudinaryAsync(brandUM.LogoImage);
                        if (uploadResult != null && uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            updatedBrand.Logo = uploadResult.SecureUrl.AbsoluteUri;
                        }
                        else
                        {
                            return BadRequest("Something wrong!");
                        }
                    }
                    await _brandService.UpdateBrandAsync(updatedBrand);
                    return Ok("Update brand successfully!");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest($"Cannot find brand with id: {brandId}");

        }

        [HttpPost]
        [Route("active-deactive-brand/{brandId}")]
        public async Task<IActionResult> ActiveDeactiveBrand(int brandId)
        {
            var deletedBrand = await (await _brandService.GetBrandById(brandId)).FirstOrDefaultAsync();
            if (deletedBrand != null)
            {
                if (deletedBrand.Status == true)
                {
                    deletedBrand.Status = false;
                }
                else
                {
                    deletedBrand.Status = true;
                }
                return Ok($"Delete brand with id: {brandId}!");
            }
            return BadRequest($"Cannot find brand with id {brandId}!");
        }

        protected int GetCurrentUserIdFromToken()
        {
            int UserId = 0;
            try
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    if (identity != null)
                    {
                        IEnumerable<Claim> claims = identity.Claims;
                        string strUserId = identity.FindFirst("UserId").Value;
                        int.TryParse(strUserId, out UserId);

                    }
                }
                return UserId;
            }
            catch
            {
                return UserId;
            }
        }
    }
}
