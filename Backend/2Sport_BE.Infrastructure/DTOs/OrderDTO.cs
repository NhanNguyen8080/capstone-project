﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2Sport_BE.Service.DTOs
{
    public class OrderDTO
    {
        public List<OrderDetailCM> orderDetailCMs { get; set; }
    }
    //SaleOrder, RentalOrder
    public class OrderCM : OrderDTO
    {
        public int UserID { get; set; }
        public int ShipmentDetailID { get; set; }
        public int PaymentMethodID { get; set; }
        public int OrderType { get; set; }
        public string? DiscountCode { get; set; } // Option
        public int BranchId { get; set; } //Branch nao nhan order
        public string? Note { get; set; }

    }
    public class GuestOrderCM : OrderDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int OrderType { get; set; }
        public int PaymentMethodID { get; set; }
        public string? DiscountCode { get; set; } // Option
        public int? BranchId { get; set; } //Branch nao nhan order
        public string? Note { get; set; }
    }
    public class GuestOrderVM : OrderDTO
    {
        public int OrderID { get; set; }
        public string? OrderCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string? Status { get; set; }
        public string? TotalPrice { get; set; }
        public string? TransportFee { get; set; }
        public string? IntoMoney { get; set; }
        public int? PaymentMethodId { get; set; }
        public string? PaymentLink { get; set; }
        public string? Note { get; set; }
        public DateTime? CreateDate { get; set; }
    }
    public class OrderUM
    {
        [Required]
        public int? BranchId { get; set; }
        [Required]
        public int? ShipmentDetailID { get; set; }
        [Required]
        public decimal TotalPrice { get; set; } //Tong tien cua item
        [Required]
        public decimal? TranSportFee { get; set; } //Phi ship
        [Required]
        public decimal? NewIntoMoney { get; set; } //Tong tien cua order
        public string? Note { get; set; }
        [Required]
        public int? Status { get; set; }
        public List<OrderDetailUM> orderDetailUMs { get; set; }

    }
    public class GuestOrderUM
    {
        [Required]
        public int? BranchId { get; set; }
        [Required]
        public decimal TotalPrice { get; set; }
        [Required]
        public decimal? TranSportFee { get; set; }
        [Required]
        public decimal? NewIntoMoney { get; set; }
        [Required]
        public string? Note { get; set; }
        [Required]
        public int? Status { get; set; }
        public List<OrderDetailUM> orderDetailUMs { get; set; }
        [Required]
        public GuestUM guestUM { get; set; }

    }
    public class OrderVM
    {
        public int OrderID { get; set; }
        public string? OrderCode { get; set; }
        public int? UserID { get; set; }
        public int? ShipmentDetailId { get; set; }
        public int? PaymentMethodId { get; set; }
        public string? TotalPrice { get; set; }
        public string? TransportFee { get; set; }
        public string? IntoMoney { get; set; }
        public string? Status { get; set; }
        public string? PaymentLink { get; set; }
        public DateTime? CreateDate { get; set; }
        public List<OrderDetailVM> orderDetailVMs { get; set; }
    }
    public class RevenueVM
    {
        public int TotalOrders { get; set; }
        public string TotalPrice { get; set; }
    }
    public class OrdersSales
    {
        public int TotalOrders { get; set; }
        public decimal TotalIntoMoney { get; set; }
        public int orderGrowthRatio { get; set; }
        public bool IsIncrease { get; set; }
    }
}
