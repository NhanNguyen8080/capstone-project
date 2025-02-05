﻿using _2Sport_BE.Repository.Data;
using _2Sport_BE.Repository.Interfaces;
using _2Sport_BE.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2Sport_BE.Service.Services
{
	public interface ILikeService
	{
		Task LikeProduct(Like like);
		Task<int> CountLikesOfProduct(string productCode);
        Task<IQueryable<Like>> GetLikesOfProduct();
		Task DeleteLikes(IEnumerable<Like> likes);
        Task<IQueryable<Like>> GetLikesOfProduct(string productCode);
        Task<Like> GetLikeByProductCodeAndUserId(string productCode, int userId);
        Task DeleteLike(Like unlikeObject);
    }

    public class LikeService : ILikeService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly TwoSportCapstoneDbContext _context;

		public LikeService(IUnitOfWork unitOfWork, TwoSportCapstoneDbContext context)
		{
			_unitOfWork = unitOfWork;
			_context = context;
		}

		public async Task<IQueryable<Like>> GetLikesOfProduct()
		{
			var likes = (await _unitOfWork.LikeRepository.GetAsync(_ => !string.IsNullOrEmpty(_.ProductCode))).AsQueryable();
			return likes;
		}
        public async Task LikeProduct(Like like)
        {
            try
            {
                var liked = (await _unitOfWork.LikeRepository.GetAsync(_ => _.UserId == like.UserId
                                                                && _.ProductCode.ToLower()
                                                                .Equals(like.ProductCode.ToLower()))).FirstOrDefault();
                if (liked != null)
                {
                    await _unitOfWork.LikeRepository.DeleteAsync(liked);
                }
                else
                {
                    await _unitOfWork.LikeRepository.InsertAsync(like);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<int> CountLikesOfProduct(string productCode)
		{
			try
			{
				var likes = (await _unitOfWork.LikeRepository.GetAsync(_ => _.ProductCode.ToLower()
                                                                    .Equals(productCode.ToLower()))).ToList();
				var numOfLikes = likes.Count();
				return numOfLikes;
			} catch (Exception ex)
			{
				return 0;
			}
		}


        public async Task DeleteLikes(IEnumerable<Like> likes)
        {
			try
			{
				await _unitOfWork.LikeRepository.DeleteRangeAsync(likes);
			} catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
        }

        public async Task<IQueryable<Like>> GetLikesOfProduct(string productCode)
        {
            var likes = (await _unitOfWork.LikeRepository.GetAsync(_ => _.ProductCode.ToLower()
                                                                        .Equals(productCode.ToLower()))).AsQueryable();
            return likes;
        }

        public async Task<Like> GetLikeByProductCodeAndUserId(string productCode, int userId)
        {
            var likes = (await _unitOfWork.LikeRepository.GetAsync(_ => _.ProductCode.ToLower()
                                                                        .Equals(productCode.ToLower()) &&
                                                                        _.UserId == userId))
                                                                        .FirstOrDefault();
            return likes;
        }

        public async Task DeleteLike(Like unlikeObject)
        {
            await _unitOfWork.LikeRepository.DeleteAsync(unlikeObject);
        }
    }


}
