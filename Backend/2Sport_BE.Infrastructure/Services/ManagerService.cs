﻿using _2Sport_BE.Repository.Interfaces;
using _2Sport_BE.Infrastructure.DTOs;
using _2Sport_BE.Repository.Models;
using AutoMapper;
namespace _2Sport_BE.Infrastructure.Services
{
    public interface IManagerService
    {
        //CRUD
        Task<ResponseDTO<ManagerVM>> CreateManagerAsync(ManagerCM managerCM);
        Task<ResponseDTO<ManagerVM>> UpdateManagerAsync(int managerId, ManagerUM managerUM);
        Task<ResponseDTO<int>> DeleteManagerAsync(int managerId);
        Task<ResponseDTO<List<ManagerVM>>> GetAllManagerAsync();
        Task<ResponseDTO<ManagerVM>> GetManagerByBranchIdAsync(int branchId);
        Task<ResponseDTO<ManagerVM>> GetManagerDetailByIdAsync(int managerId);
    }
    public class ManagerService : IManagerService
    {
        private readonly IUnitOfWork  _unitOfWork;
        private readonly IMapper _mapper;
        public ManagerService(IUnitOfWork unitOfWork,
                                IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<ManagerVM>> CreateManagerAsync(ManagerCM managerCM)
        {
            var response = new ResponseDTO<ManagerVM>();
            try
            {
                var manager = await _unitOfWork.ManagerRepository
                    .GetObjectAsync(m => m.UserId == managerCM.UserId);
                if(manager == null)
                {
                    manager = new Manager()
                    {
                        UserId = managerCM.UserId,
                        BranchId = managerCM.BranchId,
                        StartDate = managerCM.StartDate,
                        EndDate = managerCM.EndDate ?? null,
                    };
                    await _unitOfWork.ManagerRepository.InsertAsync(manager);
                    //Return
                    var result = _mapper.Map<ManagerVM>(manager);
                    response.IsSuccess = true;
                    response.Message = "Inserted successfully";
                    response.Data = result;

                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Error inserted";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO<int>> DeleteManagerAsync(int managerId)
        {
            var response = new ResponseDTO<int>();
            try
            {
                var toDeleted = await _unitOfWork.ManagerRepository
                                        .GetObjectAsync(m => m.Id == managerId);
                if (toDeleted == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Error Deleted";
                    response.Data = 0;

                }
                else
                {
                    await _unitOfWork.ManagerRepository.DeleteAsync(toDeleted);

                    response.IsSuccess = true;
                    response.Message = "Deleted Successfully";
                    response.Data = 1;

                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Data = 0;

            }
            return response;
        }

        public async Task<ResponseDTO<List<ManagerVM>>> GetAllManagerAsync()
        {
            var response = new ResponseDTO<List<ManagerVM>>();
            try
            {
                var query = await _unitOfWork.ManagerRepository.GetAllAsync();
                if(query.Count() > 0)
                {
                    var result = query.Select(m => new ManagerVM()
                    {
                        BranchId = m.BranchId,
                        EndDate = m.EndDate,
                        Id = m.Id,
                        StartDate = m.StartDate,
                        UserId  = m.UserId
                    }).ToList();

                    response.IsSuccess = true;
                    response.Message = "Query Successfully";
                    response.Data = result;

                }
                else
                {

                    response.IsSuccess = false;
                    response.Message = "Query failed";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;

            }
            return response;
        }

        public async Task<ResponseDTO<ManagerVM>> GetManagerByBranchIdAsync(int branchId)
        {
            var response = new ResponseDTO<ManagerVM>();
            try
            {
                var query = await _unitOfWork.ManagerRepository.GetObjectAsync(m => m.BranchId == branchId, new string[] { "User" });
                if (query != null)
                {
                    var managerVM = _mapper.Map<ManagerVM>(query);
                    var userVM = _mapper.Map<UserVM>(query.User);
                    managerVM.UserVM = userVM;

                    response.IsSuccess = true;
                    response.Message = "Query Successfully";
                    response.Data = managerVM;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Query failed";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;

            }
            return response;
        }

        public async Task<ResponseDTO<ManagerVM>> GetManagerDetailByIdAsync(int managerId)
        {
            var response = new ResponseDTO<ManagerVM>();
            try
            {
                var query = await _unitOfWork.ManagerRepository.GetObjectAsync(m => m.Id == managerId, new string[] { "User" });
                if (query != null)
                {
                    var managerVM = _mapper.Map<ManagerVM>(query);
                    var userVM = _mapper.Map<UserVM>(query.User);
                    managerVM.UserVM = userVM;

                    response.IsSuccess = true;
                    response.Message = "Query Successfully";
                    response.Data = managerVM;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Query failed";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;

            }
            return response;
        }

        public async Task<ResponseDTO<ManagerVM>> UpdateManagerAsync(int managerId, ManagerUM managerUM)
        {
            var response = new ResponseDTO<ManagerVM>();
            try
            {
                var manager = await _unitOfWork.ManagerRepository
                    .GetObjectAsync(m => m.Id == managerId);
                if (manager != null)
                {
                    manager = _mapper.Map<Manager>(managerUM);
                    await _unitOfWork.ManagerRepository.UpdateAsync(manager);

                    //Return
                    var result = _mapper.Map<ManagerVM>(manager);
                    response.IsSuccess = true;
                    response.Message = "Updated successfully";
                    response.Data = result;

                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Error updated";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}