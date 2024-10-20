﻿using _2Sport_BE.Repository.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace _2Sport_BE.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Blog> BlogRepository { get; }
        IGenericRepository<Brand> BrandRepository { get; }
        IGenericRepository<Branch> BranchRepository { get; }
        IGenericRepository<BrandBranch> BrandBranchRepository { get; }
        IGenericRepository<Cart> CartRepository { get; }
        IGenericRepository<CartItem> CartItemRepository { get; }
        IGenericRepository<Category> CategoryRepository { get; }
        IGenericRepository<ImagesVideo> ImagesVideoRepository { get; }
        IGenericRepository<ImportHistory> ImportHistoryRepository { get; }
        IGenericRepository<Like> LikeRepository { get; }
        IGenericRepository<Order> OrderRepository { get; }
        IGenericRepository<OrderDetail> OrderDetailRepository { get; }
        IGenericRepository<PaymentMethod> PaymentMethodRepository { get; }
        IGenericRepository<Product> ProductRepository { get; }
        IGenericRepository<Review> ReviewRepository { get; }
        IGenericRepository<Role> RoleRepository { get; }
        IGenericRepository<ShipmentDetail> ShipmentDetailRepository { get; }
        IGenericRepository<Supplier> SupplierRepository { get; }
        IGenericRepository<User> UserRepository { get; }
        IGenericRepository<Warehouse> WarehouseRepository { get; }
        IGenericRepository<RefreshToken> RefreshTokenRepository { get; }
        IGenericRepository<Sport> SportRepository { get; }
        IGenericRepository<CustomerDetail> CustomerDetailRepository { get; }
        IGenericRepository<Employee> EmployeeRepository { get; }
        IGenericRepository<EmployeeDetail> EmployeeDetailRepository { get; }
        IGenericRepository<ErrorLog> ErrorLogRepository { get; }
        IGenericRepository<Attendance> AttendanceRepository { get; }
        IGenericRepository<Guest> GuestRepository { get; }
        IGenericRepository<RentalOrder> RentalOrderRepository { get; }

        Task<IDbContextTransaction> BeginTransactionAsync();
        void Save();
        Task<bool> SaveChanges();
    }
}
