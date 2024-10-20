﻿using _2Sport_BE.Repository.Data;
using _2Sport_BE.Repository.Interfaces;
using _2Sport_BE.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace _2Sport_BE.Repository.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TwoSportCapstoneDbContext _dbContext;
        public UnitOfWork(TwoSportCapstoneDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        private IGenericRepository<Blog> _blogRepository;
        private IGenericRepository<Brand> _brandRepository;
        private IGenericRepository<Branch> _branchRepository;
        private IGenericRepository<BrandBranch> _brandBranchRepository;
        private IGenericRepository<Cart> _cartRepository;
        private IGenericRepository<CartItem> _cartItemRepository;
        private IGenericRepository<Category> _categoryRepository;
        private IGenericRepository<ImagesVideo> _imagesVideoRepository;
        private IGenericRepository<ImportHistory> _importHistoryRepository;
        private IGenericRepository<Like> _likeRepository;
        private IGenericRepository<Order> _orderRepository;
        private IGenericRepository<OrderDetail> _orderDetailRepository;
        private IGenericRepository<PaymentMethod> _paymentMethodRepository;
        private IGenericRepository<Product> _productRepository;
        private IGenericRepository<Review> _reviewRepository;
        private IGenericRepository<Role> _roleRepository;
        private IGenericRepository<ShipmentDetail> _shipmentDetailRepository;
        private IGenericRepository<Supplier> _supplierRepository;
        private IGenericRepository<User> _userRepository;
        private IGenericRepository<Warehouse> _warehouseRepository;
        private IGenericRepository<RefreshToken> _refreshTokenRepository;
        private IGenericRepository<Sport> _sportRepository;
        private IGenericRepository<Employee> _employeeRepository;
        private IGenericRepository<EmployeeDetail> _employeeDetailRepository;
        private IGenericRepository<CustomerDetail> _customerDetailRepository;
        private IGenericRepository<ErrorLog> _errorLogRepository;
        private IGenericRepository<Attendance> _attendanceRepository;
        private IGenericRepository<Guest> _guestRepository;
        private IGenericRepository<RentalOrder> _rentalOrderRepository;
        public IGenericRepository<RentalOrder> RentalOrderRepository
        {
            get
            {
                if (_rentalOrderRepository == null)
                {
                    _rentalOrderRepository = new GenericRepository<RentalOrder>(_dbContext);
                }
                return _rentalOrderRepository;
            }
        }
        public IGenericRepository<Guest> GuestRepository
        {
            get
            {
                if (_guestRepository == null)
                {
                    _guestRepository = new GenericRepository<Guest>(_dbContext);
                }
                return _guestRepository;
            }
        }
        public IGenericRepository<Attendance> AttendanceRepository
        {
            get
            {
                if (_attendanceRepository == null)
                {
                    _attendanceRepository = new GenericRepository<Attendance>(_dbContext);
                }
                return _attendanceRepository;
            }
        }
        public IGenericRepository<ErrorLog> ErrorLogRepository
        {
            get
            {
                if (_errorLogRepository == null)
                {
                    _errorLogRepository = new GenericRepository<ErrorLog>(_dbContext);
                }
                return _errorLogRepository;
            }
        }
        public IGenericRepository<Blog> BlogRepository
        {
            get
            {
                if (_blogRepository == null)
                {
                    _blogRepository = new GenericRepository<Blog>(_dbContext);
                }
                return _blogRepository;
            }
        }
        public IGenericRepository<Brand> BrandRepository
        {
            get
            {
                if (_brandRepository == null)
                {
                    _brandRepository = new GenericRepository<Brand>(_dbContext);
                }
                return _brandRepository;
            }
        }
        public IGenericRepository<Branch> BranchRepository
        {
            get
            {
                if (_branchRepository == null)
                {
                    _branchRepository = new GenericRepository<Branch>(_dbContext);
                }
                return _branchRepository;
            }
        }


        public IGenericRepository<BrandBranch> BrandBranchRepository
        {
            get
            {
                if (_brandBranchRepository == null)
                {
                    _brandBranchRepository = new GenericRepository<BrandBranch>(_dbContext);
                }
                return _brandBranchRepository;
            }
        }

        public IGenericRepository<Cart> CartRepository
        {
            get
            {
                if (_cartRepository == null)
                {
                    _cartRepository = new GenericRepository<Cart>(_dbContext);
                }
                return _cartRepository;
            }
        }
        public IGenericRepository<CartItem> CartItemRepository
        {
            get
            {
                if (_cartItemRepository == null)
                {
                    _cartItemRepository = new GenericRepository<CartItem>(_dbContext);
                }
                return _cartItemRepository;
            }
        }
        public IGenericRepository<Category> CategoryRepository
        {
            get
            {
                if (_categoryRepository == null)
                {
                    _categoryRepository = new GenericRepository<Category>(_dbContext);
                }
                return _categoryRepository;
            }
        }
        public IGenericRepository<ImagesVideo> ImagesVideoRepository
        {
            get
            {
                if (_imagesVideoRepository == null)
                {
                    _imagesVideoRepository = new GenericRepository<ImagesVideo>(_dbContext);
                }
                return _imagesVideoRepository;
            }
        }
        public IGenericRepository<ImportHistory> ImportHistoryRepository
        {
            get
            {
                if (_importHistoryRepository == null)
                {
                    _importHistoryRepository = new GenericRepository<ImportHistory>(_dbContext);
                }
                return _importHistoryRepository;
            }
        }
        public IGenericRepository<Like> LikeRepository
        {
            get
            {
                if (_likeRepository == null)
                {
                    _likeRepository = new GenericRepository<Like>(_dbContext);
                }
                return _likeRepository;
            }
        }
        public IGenericRepository<Order> OrderRepository
        {
            get
            {
                if (_orderRepository == null)
                {
                    _orderRepository = new GenericRepository<Order>(_dbContext);
                }
                return _orderRepository;
            }
        }
        public IGenericRepository<OrderDetail> OrderDetailRepository
        {
            get
            {
                if (_orderDetailRepository == null)
                {
                    _orderDetailRepository = new GenericRepository<OrderDetail>(_dbContext);
                }
                return _orderDetailRepository;
            }
        }
        public IGenericRepository<PaymentMethod> PaymentMethodRepository
        {
            get
            {
                if (_paymentMethodRepository == null)
                {
                    _paymentMethodRepository = new GenericRepository<PaymentMethod>(_dbContext);
                }
                return _paymentMethodRepository;
            }
        }
        public IGenericRepository<Product> ProductRepository
        {
            get
            {
                if (_productRepository == null)
                {
                    _productRepository = new GenericRepository<Product>(_dbContext);
                }
                return _productRepository;
            }
        }
        public IGenericRepository<Review> ReviewRepository
        {
            get
            {
                if (_reviewRepository == null)
                {
                    _reviewRepository = new GenericRepository<Review>(_dbContext);
                }
                return _reviewRepository;
            }
        }
        public IGenericRepository<Role> RoleRepository
        {
            get
            {
                if (_roleRepository == null)
                {
                    _roleRepository = new GenericRepository<Role>(_dbContext);
                }
                return _roleRepository;
            }
        }
        public IGenericRepository<ShipmentDetail> ShipmentDetailRepository
        {
            get
            {
                if (_shipmentDetailRepository == null)
                {
                    _shipmentDetailRepository = new GenericRepository<ShipmentDetail>(_dbContext);
                }
                return _shipmentDetailRepository;
            }
        }
        public IGenericRepository<Supplier> SupplierRepository
        {
            get
            {
                if (_supplierRepository == null)
                {
                    _supplierRepository = new GenericRepository<Supplier>(_dbContext);
                }
                return _supplierRepository;
            }
        }
        public IGenericRepository<User> UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new GenericRepository<User>(_dbContext);
                }
                return _userRepository;
            }
        }
        public IGenericRepository<Warehouse> WarehouseRepository
        {
            get
            {
                if (_warehouseRepository == null)
                {
                    _warehouseRepository = new GenericRepository<Warehouse>(_dbContext);
                }
                return _warehouseRepository;
            }
        }
        public IGenericRepository<RefreshToken> RefreshTokenRepository
        {
            get
            {
                if (_refreshTokenRepository == null)
                {
                    _refreshTokenRepository = new GenericRepository<RefreshToken>(_dbContext);
                }
                return _refreshTokenRepository;
            }
        }
        public IGenericRepository<Sport> SportRepository
        {
            get
            {
                if (_sportRepository == null)
                {
                    _sportRepository = new GenericRepository<Sport>(_dbContext);
                }
                return _sportRepository;
            }
        }
        public IGenericRepository<CustomerDetail> CustomerDetailRepository
        {
            get
            {
                if (_customerDetailRepository == null)
                {
                    _customerDetailRepository = new GenericRepository<CustomerDetail>(_dbContext);
                }
                return _customerDetailRepository;
            }
        }
        public IGenericRepository<Employee> EmployeeRepository
        {
            get
            {
                if (_employeeRepository == null)
                {
                    _employeeRepository = new GenericRepository<Employee>(_dbContext);
                }
                return _employeeRepository;
            }
        }
        public IGenericRepository<EmployeeDetail> EmployeeDetailRepository
        {
            get
            {
                if (_employeeDetailRepository == null)
                {
                    _employeeDetailRepository = new GenericRepository<EmployeeDetail>(_dbContext);
                }
                return _employeeDetailRepository;
            }
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }

        public async Task<bool> SaveChanges()
        {
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
