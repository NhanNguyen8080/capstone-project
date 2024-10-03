﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2Sport_BE.Service.DTOs
{
    public class OrderDTO
    {
        public List<OrderDetailCM> orderDetailCMs;
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
        public DateTime? CreateDate { get; set; }
        public string? Status { get; set; }
        public string? IntoMoney { get; set; }
        public string? TotalPrice { get; set; }
        public int? PaymentMethodId { get; set; }
        public string? PaymentLink { get; set; }
        public string? Note { get; set; }
    }
    public class OrderUM : OrderDTO
    {
       
    }
    public class OrderVM : OrderDTO
    {
        public int OrderID { get; set; }
        public string? OrderCode { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public string? IntoMoney { get; set; }
        public string? TotalPrice { get; set; }
        public int? PaymentMethodId { get; set; }
        public int? ShipmentDetailId { get; set; }
        public string? PaymentLink { get; set; }
    }
    public class RevenueVM
    {
        public int TotalOrders { get; set; }
        public string TotalPrice { get; set; }
    }
}   