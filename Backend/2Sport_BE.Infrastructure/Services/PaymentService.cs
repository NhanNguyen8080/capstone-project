﻿using _2Sport_BE.Infrastructure.DTOs;
using _2Sport_BE.Infrastructure.Enums;
using _2Sport_BE.Infrastructure.Helpers;
using _2Sport_BE.Repository.Interfaces;
using _2Sport_BE.Repository.Models;
using _2Sport_BE.Service.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Net.payOS;
using Net.payOS.Types;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Web;

namespace _2Sport_BE.Infrastructure.Services
{
    public class PaymentInfor
    {
        public string OrderCode { get; set; }
        public string AmountPaid { get; set; }
        public string BankName { get; set; }
        public string PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
    }
    public class PayOSSettings
    {
        public string ClientId { get; set; }
        public string ApiKey { get; set; }
        public string ChecksumKey { get; set; }
    }
    public interface IPaymentService
    {
        Task<ResponseDTO<string>> ProcessSaleOrderPayment(int orderId, HttpContext context);
        Task<ResponseDTO<string>> ProcessRentalOrderPayment(int orderId, HttpContext context, bool isPartiallyDeposit);

    }
    public interface IPayOsService
    {
        #region Process_PayOS_Response
        Task<ResponseDTO<SaleOrderVM>> ProcessCancelledSaleOrder(PaymentResponse paymentResponse);
        Task<ResponseDTO<SaleOrderVM>> ProcessCompletedSaleOrder(PaymentResponse paymentResponse);
        Task<ResponseDTO<RentalOrderVM>> ProcessCancelledRentalOrder(PaymentResponse paymentResponse);
        Task<ResponseDTO<RentalOrderVM>> ProcessCompletedRentalOrder(PaymentResponse paymentResponse);
        #endregion
        Task<ResponseDTO<PaymentLinkInformation>> GetPaymentDetails(string orderCode);

    }
    public class PayOsPaymentService : IPaymentService, IPayOsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private PayOS _payOs;
        private PayOSSettings payOSSettings;
        private readonly SaleOrderService _saleOrderService;
        private readonly RentalOrderService _rentalOrderService;
        private readonly INotificationService _notificationService;
        private readonly IMethodHelper _methodHelper;
        public PayOsPaymentService(IUnitOfWork unitOfWork,
                                    IConfiguration configuration,
                                    SaleOrderService saleOrderService,
                                    RentalOrderService rentalOrderService,
                                    INotificationService notificationService,
                                    IMethodHelper methodHelper)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            payOSSettings = new PayOSSettings()
            {
                ClientId = _configuration["PayOSSettings:ClientId"],
                ApiKey = _configuration["PayOSSettings:ApiKey"],
                ChecksumKey = _configuration["PayOSSettings:ChecksumKey"]
            };
            _payOs = new PayOS(payOSSettings.ClientId, payOSSettings.ApiKey, payOSSettings.ChecksumKey);
            _saleOrderService = saleOrderService;
            _rentalOrderService = rentalOrderService;
            _notificationService = notificationService;
            _methodHelper = methodHelper;
        }
        public async Task<ResponseDTO<string>> ProcessSaleOrderPayment(int orderId, HttpContext context)
        {
            var response = new ResponseDTO<string>();
            try
            {
                var order = await _unitOfWork.SaleOrderRepository
                                .GetObjectAsync(o => o.Id == orderId, new string[] { "OrderDetails" });
                if (order is null)
                {
                    response.IsSuccess = false;
                    response.Message = "Không tìm thấy đơn hàng với mã ID được cung cấp.";
                    return response;
                }

                List<ItemData> orders = new List<ItemData>();
                var listOrderDetail = order.OrderDetails.ToList();

                for (int i = 0; i < listOrderDetail.Count; i++)
                {
                    var product = await _unitOfWork.ProductRepository
                                            .GetObjectAsync(p => p.Id == listOrderDetail[i].ProductId);
                    if (product is null)
                    {
                        response.IsSuccess = false;
                        response.Message = $"Không tìm thấy sản phẩm với mã ID: {listOrderDetail[i].ProductId}";
                        return response;
                    }
                    ItemData item = new ItemData(product.ProductName,
                                                (int)(listOrderDetail[i].Quantity),
                                                Convert.ToInt32(listOrderDetail[i].UnitPrice.ToString()));
                    orders.Add(item);
                }
                if (order.TranSportFee > 0)
                {
                    ItemData item = new ItemData("Chi phi van chuyen", 1, (int)order.TranSportFee);
                    orders.Add(item);
                }
                string content = $"Mã đơn hàng: {order.SaleOrderCode}";
                int expiredAt = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * 5));

                var returnStr = _configuration["PayOSSettings:ReturnUrlSaleOrder"];
                var cancelStr = _configuration["PayOSSettings:CancelUrlSaleOrder"];

                PaymentData data = new PaymentData(Convert.ToInt64(order.SaleOrderCode),
                        Int32.Parse(order.TotalAmount.ToString()),
                        content, orders,
                        cancelStr,
                        returnStr,
                        null, order.FullName,
                        order.Email, order.ContactPhone,
                        order.Address,
                        expiredAt);
                var createPayment = await _payOs.createPaymentLink(data);

                response.IsSuccess = true;
                response.Message = "Mã dao dịch tạo thành công";
                response.Data = createPayment.checkoutUrl;

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Mã dao dịch tạo thất bại";
                response.Data = ex.Message;
                return response;
            }
        }
        public async Task<ResponseDTO<string>> ProcessRentalOrderPayment(int orderId, HttpContext context, bool isPartiallyDeposit)
        {
            var response = new ResponseDTO<string>();
            try
            {
                var order = await _unitOfWork.RentalOrderRepository
                                .GetObjectAsync(o => o.Id == orderId);
                if (order is null)
                {
                    response.IsSuccess = false;
                    response.Message = "Không tìm thấy đơn hàng với mã ID được cung cấp.";
                    return response;
                }

                List<ItemData> orders = new List<ItemData>();
                var childOrders = (await _unitOfWork.RentalOrderRepository
                                            .GetAsync(o => o.ParentOrderCode.Equals(order.RentalOrderCode))).ToList();
                if (childOrders != null && childOrders.Count > 0)
                {
                    foreach (var childOrder in childOrders)
                    {
                        ItemData item = new ItemData($"Sản phẩm thuê: {childOrder.ProductName}",
                                                (int)(childOrder.Quantity),
                                                Convert.ToInt32(childOrder.RentPrice.ToString()));
                        orders.Add(item);
                    }
                }
                else
                {
                    ItemData item = new ItemData($"Sản phẩm thuê: {order.ProductName}",
                                                (int)(order.Quantity),
                                                Convert.ToInt32(order.RentPrice.ToString()));
                    orders.Add(item);
                }

                if (order.TranSportFee > 0)
                {
                    ItemData item = new ItemData("Chi phi van chuyen", 1, (int)order.TranSportFee);
                    orders.Add(item);
                }

                string content = $"Hoa Don Thue: {order.RentalOrderCode}";
                int expiredAt = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * 5));

                var returnStr = _configuration["PayOSSettings:ReturnUrlRentalOrder"];
                var cancelStr = _configuration["PayOSSettings:CancelUrlRentalOrder"];

                int totalAmount = isPartiallyDeposit
                    ? (int)Math.Ceiling((decimal)order.TotalAmount / 2)
                    : (int)order.TotalAmount;

                PaymentData data = new PaymentData(Convert.ToInt64(order.RentalOrderCode),
                        totalAmount,
                        content, orders,
                        cancelStr,
                        returnStr,
                        null, order.FullName,
                        order.Email, order.ContactPhone,
                        order.Address,
                        expiredAt);
                var createPayment = await _payOs.createPaymentLink(data);

                response.IsSuccess = true;
                response.Message = "Mã dao dịch tạo thành công";
                response.Data = createPayment.checkoutUrl;


            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Mã dao dịch tạo thất bại";
                response.Data = ex.Message;
            }
            return response;
        }
        #region Process_PayOs_Response
        public async Task<ResponseDTO<SaleOrderVM>> ProcessCancelledSaleOrder(PaymentResponse paymentResponse)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var saleOrder = await _unitOfWork.SaleOrderRepository.GetObjectAsync(o => o.SaleOrderCode == paymentResponse.OrderCode);
                    if (saleOrder == null)
                        return _saleOrderService.GenerateErrorResponse($"SaleOrder with code {paymentResponse.OrderCode} is not found!");

                    saleOrder.PaymentStatus = (int)PaymentStatus.CANCELED;
                    saleOrder.PaymentDate = _methodHelper.GetTimeInUtcPlus7();
                    saleOrder.UpdatedAt = _methodHelper.GetTimeInUtcPlus7();
                    await _unitOfWork.SaleOrderRepository.UpdateAsync(saleOrder);
                    await _notificationService.NotifyPaymentCancellation(saleOrder.SaleOrderCode, false, saleOrder.BranchId);

                    await transaction.CommitAsync();

                    return _saleOrderService.GenerateSuccessResponse(saleOrder, $"Cancelled SaleOrder with code {paymentResponse.OrderCode}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return _saleOrderService.GenerateErrorResponse(ex.Message);
                }
            }
        }
        public async Task<ResponseDTO<SaleOrderVM>> ProcessCompletedSaleOrder(PaymentResponse paymentResponse)
        {
            try
            {
                var saleOrder = await _unitOfWork.SaleOrderRepository.GetObjectAsync(o => o.SaleOrderCode == paymentResponse.OrderCode);
                if (saleOrder == null)
                {
                    return _saleOrderService.GenerateErrorResponse($"SaleOrder with code {paymentResponse.OrderCode} is not found!");
                }
                saleOrder.PaymentStatus = (int)PaymentStatus.PAID;
                saleOrder.PaymentDate = _methodHelper.GetTimeInUtcPlus7();
                saleOrder.UpdatedAt = _methodHelper.GetTimeInUtcPlus7();

                await _unitOfWork.SaleOrderRepository.UpdateAsync(saleOrder);

                await _notificationService.NotifyPaymentPaid(saleOrder.SaleOrderCode, false, saleOrder.BranchId);

                return _saleOrderService.GenerateSuccessResponse(saleOrder, $"Completed SaleOrder with code {paymentResponse.OrderCode}");
            }
            catch (Exception ex)
            {
                return _saleOrderService.GenerateErrorResponse(ex.Message);
            }
        }
        public async Task<ResponseDTO<RentalOrderVM>> ProcessCancelledRentalOrder(PaymentResponse paymentResponse)
        {
            var response = new ResponseDTO<int>();
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var rentalOrder = await _unitOfWork.RentalOrderRepository.GetObjectAsync(o => o.RentalOrderCode == paymentResponse.OrderCode);
                    if (rentalOrder == null)
                        return _rentalOrderService.GenerateErrorResponse($"Rental Order with code {paymentResponse.OrderCode} is not found!");

                    var childOrders = await _unitOfWork.RentalOrderRepository
                                       .GetAsync(od => od.ParentOrderCode == rentalOrder.RentalOrderCode);

                    if (childOrders.Any())
                    {
                        foreach (var item in childOrders)
                        {
                            item.TransactionId = item.ParentOrderCode;
                            item.PaymentStatus = (int)PaymentStatus.CANCELED;
                            item.DepositStatus = (int)DepositStatus.NOT_PAID;
                            item.DepositDate = _methodHelper.GetTimeInUtcPlus7();
                            item.PaymentDate = _methodHelper.GetTimeInUtcPlus7();
                            item.UpdatedAt = _methodHelper.GetTimeInUtcPlus7();
                            await _unitOfWork.RentalOrderRepository.UpdateAsync(item);
                        }
                    }
                    rentalOrder.TransactionId = rentalOrder.ParentOrderCode;
                    rentalOrder.PaymentStatus = (int)PaymentStatus.CANCELED;
                    rentalOrder.DepositStatus = (int)PaymentStatus.CANCELED;

                    rentalOrder.DepositDate = _methodHelper.GetTimeInUtcPlus7();
                    rentalOrder.PaymentDate = _methodHelper.GetTimeInUtcPlus7();
                    rentalOrder.UpdatedAt = _methodHelper.GetTimeInUtcPlus7();

                    await _unitOfWork.RentalOrderRepository.UpdateAsync(rentalOrder);

                    await _notificationService.NotifyPaymentCancellation(rentalOrder.RentalOrderCode, true, rentalOrder.BranchId);
                    await transaction.CommitAsync();

                    return _rentalOrderService.GenerateSuccessResponse(rentalOrder, null, $"Cancelled Rental Oder with code {paymentResponse.OrderCode} Succesfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return _rentalOrderService.GenerateErrorResponse(ex.Message);
                }
            }
        }
        public async Task<ResponseDTO<RentalOrderVM>> ProcessCompletedRentalOrder(PaymentResponse paymentResponse)
        {
            try
            {
                var rentalOrder = await _unitOfWork.RentalOrderRepository.GetObjectAsync(o => o.RentalOrderCode == paymentResponse.OrderCode);
                if (rentalOrder == null)
                    return _rentalOrderService.GenerateErrorResponse($"Rental Order with code {paymentResponse.OrderCode} is not found!");

                if (rentalOrder.DepositStatus == (int)DepositStatus.PARTIALLY_PENDING)
                {
                    rentalOrder.DepositStatus = (int)DepositStatus.PARTIALLY_PENDING;
                    rentalOrder.DepositAmount = rentalOrder.SubTotal / 2;
                }
                else
                {
                    rentalOrder.DepositStatus = (int)DepositStatus.FULL_PAID;
                    rentalOrder.DepositAmount = rentalOrder.SubTotal;
                }

                rentalOrder.PaymentStatus = (int)PaymentStatus.DEPOSIT;
                rentalOrder.PaymentDate = _methodHelper.GetTimeInUtcPlus7();
                rentalOrder.DepositDate = _methodHelper.GetTimeInUtcPlus7();
                rentalOrder.UpdatedAt = _methodHelper.GetTimeInUtcPlus7();

                await _unitOfWork.RentalOrderRepository.UpdateAsync(rentalOrder);

                await _notificationService.NotifyPaymentPaid(rentalOrder.RentalOrderCode, true, rentalOrder.BranchId);

                return _rentalOrderService.GenerateSuccessResponse(rentalOrder, null, $"Completed SaleOrder with code {paymentResponse.OrderCode}");
            }
            catch (Exception ex)
            {
                return _rentalOrderService.GenerateErrorResponse(ex.Message);
            }
        }

        public async Task<ResponseDTO<PaymentLinkInformation>> GetPaymentDetails(string orderCode)
        {
            var response = new ResponseDTO<PaymentLinkInformation>();
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOs.getPaymentLinkInformation(long.Parse(orderCode));

                response.IsSuccess = true;
                response.Message = $"Query succesffully";
                response.Data = paymentLinkInformation;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return response;
            }
        }
        #endregion

    }
    public class PaypalPaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public PaypalPaymentService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
        }

        public Task<ResponseDTO<string>> ProcessPayment(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDTO<string>> ProcessRentalOrderPayment(int orderId, HttpContext context, bool isPartiallyDeposit)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDTO<string>> ProcessSaleOrderPayment(int orderId, HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
    public interface IVnPayService
    {
        Task<ResponseDTO<RentalOrderVM>> PaymentRentalOrderExecute(IQueryCollection collections);
        Task<ResponseDTO<SaleOrderVM>> PaymentSaleOrderExecute(IQueryCollection collections);

        public PaymentInfor QueryTransaction(string orderCode, string transactionId,string paymentMethod, DateTime payDate, HttpContext context);
    }
    public class VnPayPaymentService : IPaymentService, IVnPayService
    {
        public class QueryRequest
        {
            public string OrderId { get; set; }
            public string PayDate { get; set; }
        }

        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IMethodHelper _methodHelper;
        private readonly SaleOrderService _saleOrderService;
        private readonly RentalOrderService _rentalOrderService;
        private readonly INotificationService _notificationService;
        public VnPayPaymentService(IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IMethodHelper methodHelper,
            RentalOrderService rentalOrderService,
            SaleOrderService saleOrderService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _methodHelper = methodHelper;
            this._rentalOrderService = rentalOrderService;
            this._saleOrderService = saleOrderService;
            _notificationService = notificationService;
        }

        public async Task<ResponseDTO<string>> ProcessSaleOrderPayment(int orderId, HttpContext context)
        {
            var model = await _unitOfWork.SaleOrderRepository.GetObjectAsync(o => o.Id == orderId, new string[] { "OrderDetails" });
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["PaymentCallBack:ReturnUrlSaleOrder"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.TotalAmount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang mua: {model.SaleOrderCode}");
            pay.AddRequestData("vnp_OrderType", "sale order");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);//transactionId


            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return new ResponseDTO<string>()
            {
                Data = paymentUrl,
                IsSuccess = true,
                Message = "Generate payment string"
            };
        }
        public async Task<ResponseDTO<string>> ProcessRentalOrderPayment(int orderId, HttpContext context, bool isPartiallyDeposit)
        {
            var order = _unitOfWork.RentalOrderRepository.FindObject(o => o.Id == orderId);
            if (order == null)
            {
                return new ResponseDTO<string>()
                {
                    IsSuccess = false,
                    Message = "Order is not found"
                };
            }
            var model = await _unitOfWork.RentalOrderRepository.GetObjectAsync(o => o.Id == orderId);
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["PaymentCallBack:ReturnUrlRentalOrder"];

            int totalAmount = isPartiallyDeposit
                    ? (int)Math.Ceiling((decimal)order.TotalAmount / 2)
                    : (int)order.TotalAmount;

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", (totalAmount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang thue:" + model.RentalOrderCode);
            pay.AddRequestData("vnp_OrderType", "Rental Order");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return new ResponseDTO<string>()
            {
                Data = paymentUrl,
                IsSuccess = true,
                Message = "Generate payment string"
            };
        }
        public async Task<ResponseDTO<SaleOrderVM>> PaymentSaleOrderExecute(IQueryCollection collections)
        {
            var vnPayLib = new VnPayLibrary();
            var response = vnPayLib.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

            if (!response.IsSuccess)
                return _saleOrderService.GenerateErrorResponse("Có lỗi trong quá trình thanh toán, vui lòng thử lại sau.");

            var saleOrder = _unitOfWork.SaleOrderRepository.FindObject(o => o.SaleOrderCode == response.Data.OrderCode);
            if (saleOrder == null)
                return _saleOrderService.GenerateErrorResponse($"Không tìm thấy đơn mua - {response.Data.OrderCode}!");

            var paymentStatus = response.Data.TransactionStatus == "00" ? PaymentStatus.PAID : PaymentStatus.CANCELED;

            UpdateSaleOrder(saleOrder, paymentStatus, response.Data.TxnRef);
            await _unitOfWork.SaleOrderRepository.UpdateAsync(saleOrder);

            if (paymentStatus == PaymentStatus.PAID)
            {
                await _notificationService.NotifyPaymentPaid(saleOrder.SaleOrderCode, false, saleOrder.BranchId);
                return _saleOrderService.GenerateSuccessResponse(saleOrder, "Completed");
            }

            await _notificationService.NotifyPaymentCancellation(saleOrder.SaleOrderCode, false, saleOrder.BranchId);
            return _saleOrderService.GenerateSuccessResponse(saleOrder, "Canceled");
        }
        public async Task<ResponseDTO<RentalOrderVM>> PaymentRentalOrderExecute(IQueryCollection collections)
        {
            var vnPayLib = new VnPayLibrary();
            var response = vnPayLib.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

            if (response == null || response.Data == null) return _rentalOrderService.GenerateErrorResponse("Invalid response data!");

            var rentalOrder = _unitOfWork.RentalOrderRepository.FindObject(o => o.RentalOrderCode == response.Data.OrderCode);
            if (rentalOrder == null) return _rentalOrderService.GenerateErrorResponse("Order not found!");

            DepositStatus newDepositStatus = response.Data.TransactionStatus == "00"
                ? (rentalOrder.DepositStatus == (int)DepositStatus.PARTIALLY_PENDING ? DepositStatus.PARTIALLY_PAID : DepositStatus.FULL_PAID)
                : DepositStatus.CANCELED;

            PaymentStatus paymentStatus = response.Data.TransactionStatus == "00"
                ? PaymentStatus.DEPOSIT
                : PaymentStatus.CANCELED;

            var childOrders = await _unitOfWork.RentalOrderRepository
                                     .GetAsync(od => od.ParentOrderCode == rentalOrder.RentalOrderCode);
            if (response.Data.TransactionStatus != "00") // Huy
            {
                if (childOrders.Any())
                {
                    foreach (var item in childOrders)
                    {
                        UpdateOrderPaymentDetails(item, (int)paymentStatus, (int)newDepositStatus, response.Data.TxnRef);
                    }
                    await _unitOfWork.RentalOrderRepository.UpdateRangeAsync(childOrders.ToList());
                }

                UpdateOrderPaymentDetails(rentalOrder, (int)paymentStatus, (int)newDepositStatus, response.Data.TxnRef);

                await _unitOfWork.RentalOrderRepository.UpdateAsync(rentalOrder);
                await _notificationService.NotifyPaymentCancellation(rentalOrder.RentalOrderCode, true, rentalOrder.BranchId);

                return _rentalOrderService.GenerateSuccessResponse(rentalOrder, null, "Canceled");
            }
            else
            {
                
                rentalOrder.TransactionId = response.Data.TxnRef;
                rentalOrder.DepositStatus = (int)newDepositStatus;
                rentalOrder.PaymentStatus = (int)paymentStatus;
                rentalOrder.DepositAmount = response.Data.Amount;

                rentalOrder.PaymentDate= _methodHelper.GetTimeInUtcPlus7();
                rentalOrder.DepositDate = _methodHelper.GetTimeInUtcPlus7();
                rentalOrder.UpdatedAt = _methodHelper.GetTimeInUtcPlus7();

                await _unitOfWork.RentalOrderRepository.UpdateAsync(rentalOrder);

                await _notificationService.NotifyPaymentPaid(rentalOrder.RentalOrderCode, true, rentalOrder.BranchId);
                return _rentalOrderService.GenerateSuccessResponse(rentalOrder, null, "Completed");
            }
        }

        public PaymentInfor QueryTransaction(string orderCode, string transactionId,string paymentMethod, DateTime payDate, HttpContext context)
        {
            try
            {
                var pay = new VnPayLibrary();
                var vnp_Api = _configuration["Vnpay:vnp_Api"];
                var vnp_HashSecret = _configuration["Vnpay:HashSecret"];
                var vnp_RequestId = DateTime.Now.Ticks.ToString();
                var vnp_Version = _configuration["Vnpay:Version"];
                var vnp_Command = "querydr";
                var vnp_TmnCode = _configuration["Vnpay:TmnCode"];
                var vnp_TxnRef = transactionId;
                var vnp_OrderInfo = $"Thanh toan don hang mua: {orderCode}";
                var vnp_TransactionDate = _methodHelper.GetFormattedDateInGMT7(payDate);
                var vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
                var vnp_IpAddr = pay.GetIpAddress(context);

                var signData = $"{vnp_RequestId}|{vnp_Version}|{vnp_Command}|{vnp_TmnCode}|{vnp_TxnRef}|{vnp_TransactionDate}|{vnp_CreateDate}|{vnp_IpAddr}|{vnp_OrderInfo}";
                var vnp_SecureHash = pay.HmacSha512(vnp_HashSecret, signData);

                var qdrData = new
                {
                    vnp_RequestId,
                    vnp_Version,
                    vnp_Command,
                    vnp_TmnCode,
                    vnp_TxnRef,
                    vnp_OrderInfo,
                    vnp_TransactionDate,
                    vnp_CreateDate,
                    vnp_IpAddr,
                    vnp_SecureHash
                };

                var jsonData = JsonConvert.SerializeObject(qdrData);

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(vnp_Api);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonData);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string response = streamReader.ReadToEnd();

                    var tempDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                    if (tempDictionary == null || tempDictionary.Count == 0)
                        throw new Exception("Response dictionary is null or empty.");

                    var dictionary = new Dictionary<string, StringValues>();
                    foreach (var item in tempDictionary)
                    {
                        dictionary.Add(item.Key, new StringValues(item.Value ?? string.Empty));
                    }

                    var queryCollection = new QueryCollection(dictionary);

                    DateTime parsedDateTime = DateTime.MinValue;
                    if (queryCollection.TryGetValue("vnp_PayDate", out var payDateValue) && !StringValues.IsNullOrEmpty(payDateValue))
                    {
                        if (!DateTime.TryParseExact(payDateValue.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
                        {
                            parsedDateTime = DateTime.MinValue;
                        }
                    }

                    var paymentInfor = new PaymentInfor()
                    {
                        AmountPaid = queryCollection.TryGetValue("vnp_Amount", out var amountValue) && !StringValues.IsNullOrEmpty(amountValue)
                            ? amountValue.ToString()
                            : "0",
                        BankName = queryCollection.TryGetValue("vnp_BankCode", out var bankCodeValue) && !StringValues.IsNullOrEmpty(bankCodeValue)
                            ? bankCodeValue.ToString()
                            : "UNKNOWN",
                        OrderCode = orderCode,
                        PaymentStatus = queryCollection.TryGetValue("vnp_TransactionStatus", out var transactionStatusValue) && transactionStatusValue == "00"
                            ? "PAID"
                            : "CANCELED",
                        PaymentDate = parsedDateTime == DateTime.MinValue
                            ? "N/A"
                            : parsedDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                    };

                    return paymentInfor;
                }
            }
            catch (WebException ex)
            {
                using (var responseStream = ex.Response?.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var errorResponse = reader.ReadToEnd();
                            throw new Exception($"HTTP request failed with error: {errorResponse}", ex);
                        }
                    }
                }
                throw new Exception("HTTP request failed and no response was received.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to parse JSON response.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while querying the transaction: {ex.Message}", ex);
            }
        }

        private void UpdateOrderPaymentDetails(RentalOrder order, int paymentStatus, int depositStatus, string txnRef)
        {
            order.PaymentStatus = paymentStatus;
            order.DepositStatus = depositStatus;
            order.TransactionId = txnRef;
            order.PaymentDate = _methodHelper.GetTimeInUtcPlus7();
            order.DepositDate = _methodHelper.GetTimeInUtcPlus7();
            order.UpdatedAt = _methodHelper.GetTimeInUtcPlus7();
        }
        private void UpdateSaleOrder(SaleOrder order, PaymentStatus paymentStatus, string txnRef)
        {
            order.TransactionId = txnRef;
            order.PaymentStatus = (int)paymentStatus;
            order.PaymentDate = _methodHelper.GetTimeInUtcPlus7();
            order.UpdatedAt = _methodHelper.GetTimeInUtcPlus7();
        }
    }
}
