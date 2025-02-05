import React, { useState, useEffect } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import axios from "axios";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faArrowLeft,
  faUser,
  faEnvelope,
  faPhone,
  faMapMarkerAlt,
  faShoppingCart,
  faMoneyBillWave,
  faCalendarAlt,
  faTruck,
  faCoins,
  faBolt,
  faVenusMars,
  faDollarSign,
  faClock,
  faCheckCircle,
  faCogs,
  faArrowsDownToLine,
  faRecycle,
  faFlagCheckered,
  faCreditCard,
  faHouse,
} from "@fortawesome/free-solid-svg-icons";
import {
  Button,
  Tooltip,
  Typography,
  Input,
  Step,
  Stepper,
} from "@material-tailwind/react";
import CancelRentalOrderButton from "../components/User/CancelRentalOrderButton";
import DoneRentalOrderButton from "../components/User/DoneRentalOrderButton";
import OrderCancellationInfo from "../components/User/OrderCancellationInfo";
import OrderDepositInfo from "../components/Rental/OrderDepositInfo";
import ExtensionStatusMessage from "../components/Rental/ExtensionStatusMessage";
import ExtensionRequestButton from "../components/User/ExtensionRequestButton";
import RentalRefundRequestForm from "../components/Refund/RentalRefundRequestForm";
import RefundRequestPopup from "../components/Order/RefundRequestPopup";
import ReturnRentalProductButton from "../components/Rental/ReturnRentalProductButton";

const orderStatusColors = {
  "Chờ xử lý": "text-yellow-800",
  "Đã xác nhận": "text-orange-800",
  "Đang xử lý": "text-purple-800",
  "Đã giao cho ĐVVC": "text-blue-800",
  "Đã giao hàng": "text-indigo-800",
  "Đang gia hạn": "text-fuchsia-900",
  "Đã hủy": "text-red-900",
  "Đã hoàn thành": "text-green-800",
};

const paymentStatusColors = {
  "Đang chờ thanh toán": "text-yellow-800",
  "Đã đặt cọc": "text-blue-800",
  "Đã thanh toán": "text-green-800",
  "Đã hủy": "text-red-800",
};

const ORDER_STEPS = [
  { id: 1, label: "Chờ Xác Nhận Đơn Hàng" },
  { id: 2, label: "Đã Xác Nhận Thông Tin" },
  { id: 3, label: " Đang Xử Lý Đơn Hàng" },
  { id: 4, label: "Đã Giao Cho ĐVVC" },
  { id: 5, label: "Đã Nhận Được Hàng" },
  { id: 9, label: "Đang Gia Hạn" },
  { id: 14, label: "Đã Hoàn Thành" },
];

export default function GuestRentOrderDetail() {
  const { orderCode } = useParams();
  const navigate = useNavigate();
  const [orderDetail, setOrderDetail] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [reload, setReload] = useState(false);
  const [loading, setLoading] = useState(true);
  const [confirmReload, setConfirmReload] = useState(false);
  const [extendReload, setExtendReload] = useState(false);

  const compareDates = (date1, date2) => {
    const d1 = new Date(date1);
    const d2 = new Date(date2);

    return (
      d1.getFullYear() === d2.getFullYear() &&
      d1.getMonth() === d2.getMonth() &&
      d1.getDate() === d2.getDate()
    );
  };

  const getCurrentStepIndex = (orderStatus) => {
    const step = ORDER_STEPS.find((step) => step.id === orderStatus);
    return step ? ORDER_STEPS.indexOf(step) : -1;
  };

  const fetchOrderDetail = async () => {
    try {
      const response = await axios.get(
        `https://twosport-api-offcial-685025377967.asia-southeast1.run.app/api/RentalOrder/get-rental-order-by-orderCode?orderCode=${orderCode}`,
        {
          headers: {
            accept: "*/*",
          },
        }
      );

      if (response.data.isSuccess) {
        setOrderDetail(response.data.data);
      } else {
        setError("Failed to fetch order details");
      }
    } catch (err) {
      setError(err.message || "An error occurred while fetching order details");
    } finally {
      setIsLoading(false);
    }
  };
  useEffect(() => {
    fetchOrderDetail();
    getCurrentStepIndex();
  }, [orderCode, reload, confirmReload, extendReload]);

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <div className="animate-spin rounded-full h-32 w-32 border-t-2 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  if (error) {
    return <div className="text-center text-red-500 mt-4">Error: {error}</div>;
  }
  const {
    id,
    fullName,
    email,
    contactPhone,
    gender,
    address,
    rentalOrderCode,
    childOrders,
    isExtended,
    productName,
    rentPrice,
    rentalStartDate,
    rentalEndDate,
    totalAmount,
    paymentMethod,
    depositAmount,
    orderStatus,
    orderStatusId,
    paymentStatus,
    deliveryMethod,
    imgAvatarPath,
    updatedAt,
  } = orderDetail;

  const children = childOrders?.$values || [];

  return (
    <div className="container mx-auto p-4 min-h-screen bg-gray-200">
      <div className="max-w-4xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <button
            onClick={() => navigate(-1)}
            className="flex items-center gap-2 text-blue-500 hover:text-blue-700"
          >
            <FontAwesomeIcon icon={faArrowLeft} />
            Quay lại
          </button>
        </div>
        {orderStatus === "Đã hủy" && (
          <OrderCancellationInfo
            updatedAt={orderDetail.updatedAt}
            reason={orderDetail.reason}
          />
        )}
        {depositAmount != null && depositAmount > 0 && (
          <OrderDepositInfo
            depositAmount={depositAmount}
            depositDate={orderDetail.depositDate}
            paymentMethod={orderDetail.paymentMethod}
          />
        )}
        <div className="bg-white p-6 rounded-lg shadow-lg mb-6">
          <div className="py-4 bg-white rounded-md shadow-sm">
            {/* Stepper */}
            <div className="mb-16 bg-orange-100">
              <Stepper
                activeStep={getCurrentStepIndex(orderStatusId)}
                className="p-2 rounded-lg"
              >
                {ORDER_STEPS.map((status, index) => {
                  const currentIndex = getCurrentStepIndex(orderStatusId);

                  const isRedStep =
                    orderStatusId === 6 && index >= currentIndex;
                  const isCompleted = index <= currentIndex;
                  return (
                    <Step
                      key={index}
                      completed={isCompleted}
                      className={`${
                        isCompleted
                          ? "bg-blue-500 text-wrap w-10 text-green-600"
                          : isRedStep
                          ? "bg-red-600 text-red-600"
                          : "bg-green-600 text-green-600"
                      }`}
                    >
                      <div className="relative flex flex-col items-center">
                        <div
                          className={`w-10 h-10 flex items-center justify-center rounded-full ${
                            isCompleted
                              ? "bg-green-500 text-white"
                              : isRedStep
                              ? "bg-red-600 text-white"
                              : "bg-gray-300 text-gray-600"
                          }`}
                        >
                          <FontAwesomeIcon
                            icon={
                              index === 0
                                ? faClock
                                : index === 1
                                ? faCheckCircle
                                : index === 2
                                ? faCogs
                                : index === 3
                                ? faTruck
                                : index === 4
                                ? faArrowsDownToLine
                                : index === 5
                                ? faRecycle
                                : index === 6
                                ? faFlagCheckered
                                : faClock
                            }
                            className="text-lg"
                          />
                        </div>
                        <div
                          className={`absolute top-12 text-xs font-medium text-wrap w-20 text-center ${
                            isCompleted
                              ? "text-green-600"
                              : isRedStep
                              ? "text-red-600"
                              : "text-gray-600"
                          }`}
                        >
                          {status.label}
                        </div>
                      </div>
                    </Step>
                  );
                })}
              </Stepper>
            </div>
            <div className="flex justify-between items-center">
              <h2 className="text-2xl font-bold text-gray-800 flex-1">
                MÃ ĐƠN HÀNG THUÊ -{" "}
                <span className="text-orange-500">#{rentalOrderCode}</span>
              </h2>
              <div className="flex items-center gap-4">
                <h2 className="text-2xl font-semibold  text-gray-800 flex-1">
                  {orderStatus.toUpperCase()}
                </h2>
              </div>
            </div>
          </div>

          <hr className="mb-5" />
          <div className="grid grid-cols-4 gap-6">
            {/* Thong tin don hang */}
            <div className="col-span-3">
              {/* Thong tin khach hang */}
              <div>
                <h2 className="text-lg font-bold mb-2 text-gray-700">
                  Thông tin khách hàng
                </h2>
                <p className="flex items-center gap-2 mb-2">
                  <FontAwesomeIcon
                    icon={faUser}
                    className="text-blue-500 fa-sm"
                  />
                  <span className="font-base">Tên:</span>
                  <i>{fullName}</i>
                </p>
                <p className="flex items-center gap-2 mb-2">
                  <FontAwesomeIcon
                    icon={faVenusMars}
                    className="text-blue-500 fa-xs"
                  />
                  <span className="font-base">Giới tính:</span>
                  <i>{gender}</i>
                </p>
                <p className="flex items-center gap-2 mb-2">
                  <FontAwesomeIcon
                    icon={faEnvelope}
                    className="text-blue-500 fa-sm"
                  />
                  <span className="font-base">Email:</span>
                  <i>{email}</i>
                </p>
                <p className="flex items-center gap-2 mb-2">
                  <FontAwesomeIcon
                    icon={faPhone}
                    className="text-blue-500 fa-sm"
                  />
                  <span className="font-base">Số điện thoại:</span>
                  <i>{contactPhone}</i>
                </p>
                <p className="flex items-center gap-2 mb-2">
                  <FontAwesomeIcon
                    icon={faMapMarkerAlt}
                    className="text-blue-500"
                  />
                  <span className="font-base flex-shrink-0">Địa chỉ:</span>
                  <span className="break-words">
                    <i>{address}</i>
                  </span>
                </p>
              </div>
              {/* Thong tin don hang */}
              <div>
                <h2 className="text-lg font-bold mb-2 text-gray-700">
                  Thông tin đơn hàng
                </h2>
                <p className="flex items-center gap-2 mb-2">
                  <FontAwesomeIcon
                    icon={faMoneyBillWave}
                    className="text-blue-500"
                  />
                  <span className="font-base">Tình trạng thanh toán:</span>{" "}
                  <span
                    className={` mr-1.5 rounded-full text-xs font-bold ${
                      paymentStatusColors[paymentStatus] ||
                      "bg-gray-100 text-gray-800"
                    }`}
                  >
                    {paymentStatus}
                  </span>
                </p>

                <p className="flex items-center gap-2 mb-2">
                  <FontAwesomeIcon
                    icon={faCreditCard}
                    className="text-blue-500"
                  />
                  <span className="font-base">Phương thức thanh toán:</span>{" "}
                  <span className="break-words">
                    <i>{paymentMethod}</i>
                  </span>
                </p>
                <p className="flex items-start gap-2 mb-2">
                  <FontAwesomeIcon icon={faCoins} className="text-blue-500" />
                  <span className="font-base flex-shrink-0">
                    Số tiền đặt cọc:{" "}
                  </span>
                  <i>
                    {(depositAmount ? depositAmount : 0).toLocaleString(
                      "vi-VN"
                    )}
                    ₫
                  </i>
                </p>

                <p className="flex items-start gap-2 mb-2 w-full">
                  <FontAwesomeIcon
                    icon={faTruck}
                    className="text-blue-500 fa-sm"
                  />
                  <span className="font-base flex-shrink-0">
                    Phương thức nhận hàng:
                  </span>
                  <span className="break-words">
                    <i>{deliveryMethod}</i>
                  </span>
                </p>
              </div>
              {/* Thong tin chi nhanh giao hang */}
              <div className="pt-2">
                <h2 className="text-lg font-bold mb-2 text-gray-700">
                  Chi nhánh giao hàng
                </h2>
                <p className="flex items-center gap-2 mb-2">
                  <FontAwesomeIcon icon={faHouse} className="text-blue-500" />
                  <span className="font-base">Chi nhánh giao hàng:</span>{" "}
                  <span className="break-words">
                    {orderDetail.branchId ? (
                      <i>{orderDetail.branchName || orderDetail.branchId}</i>
                    ) : (
                      <i>Chưa chỉ định chi nhánh giao hàng</i>
                    )}
                  </span>
                </p>
              </div>
            </div>
            <div className="col-span-1 flex flex-col gap-4">
              {/* Thanh toan button */}
              {paymentStatus !== "Đã đặt cọc" &&
                (orderStatus === "Đã xác nhận" ||
                  orderStatus === "Chờ xử lý") && (
                  <Button
                    size="sm"
                    className="w-full text-blue-700 bg-white border border-blue-700 rounded-md hover:bg-blue-200"
                    onClick={() =>
                      navigate("/rental-checkout", {
                        state: { selectedOrder: orderDetail },
                      })
                    }
                  >
                    Thanh toán
                  </Button>
                )}
              {/* Huy don hang button */}
              {orderDetail.orderStatus === "Chờ xử lý" && (
                <CancelRentalOrderButton
                  rentalOrderId={id}
                  setReload={setReload}
                  className="w-full"
                />
              )}
              {/*  Đã nhận hàng button */}
              {orderDetail.orderStatus === "Đã giao cho ĐVVC" && (
                <DoneRentalOrderButton
                  rentalOrderId={orderDetail.id}
                  setConfirmReload={setConfirmReload}
                  className="w-full"
                />
              )}
              {/* REVIEW */}
              {orderDetail.orderStatus === "Đã hủy" &&
                (orderDetail.paymentStatus === "Đã thanh toán" ||
                  orderDetail.depositAmount > 0) &&
                orderDetail.refundRequests == null && (
                  <RentalRefundRequestForm orderDetail={orderDetail} />
                )}
              {orderDetail.refundRequests != null && (
                <RefundRequestPopup
                  refundRequests={orderDetail.refundRequests}
                />
              )}
            </div>
          </div>
        </div>

        <div className="bg-white p-4 rounded-lg shadow-lg">
          <h3 className="text-lg font-semibold mb-4 text-gray-700">
            Chi tiết sản phẩm
          </h3>
          {children.length > 0 ? (
            children.map((child) => (
              <div
                key={child.id}
                className="bg-gray-100 p-4 mt-4 rounded-lg shadow-sm "
              >
                <ExtensionStatusMessage
                  extensionStatus={child.extensionStatus}
                />
                <div className="flex flex-col md:flex-row gap-4">
                  <img
                    src={child.imgAvatarPath}
                    alt={child.productName}
                    className="w-full md:w-32 h-32 object-cover rounded"
                  />
                  <div className="flex-grow">
                    <h4 className="font-semibold text-lg mb-2 text-orange-500">
                      <Link to={`/product/${child.productCode}`}>
                        {child.productName}
                      </Link>
                    </h4>
                    <div className="grid grid-cols-2 gap-2">
                      <p>
                        <span className="font-semibold">Màu sắc:</span>{" "}
                        <i>{child.color}</i>
                      </p>
                      <p>
                        <span className="font-semibold">Số lượng:</span>{" "}
                        <i>{child.quantity}</i>
                      </p>
                      <p>
                        <span className="font-semibold">Tình trạng:</span>{" "}
                        <i>{child.condition}%</i>
                      </p>
                      <p>
                        <span className="font-semibold">Giá thuê:</span>{" "}
                        <i>{child.rentPrice.toLocaleString("vi-VN")}₫</i>
                      </p>
                      <p>
                        <span className="font-semibold">Kích thước:</span>{" "}
                        <i>{child.size}</i>
                      </p>
                      <p>
                        <span className="font-semibold">Thời gian thuê:</span>{" "}
                        <i>
                          {new Date(child.rentalStartDate).toLocaleDateString()}{" "}
                          - {new Date(child.rentalEndDate).toLocaleDateString()}
                        </i>
                      </p>
                    </div>
                  </div>
                </div>
                <div className="pt-2 flex justify-between items-center gap-4">
                  <div className="flex items-center gap-2">
                    <p className="flex items-center gap-1">
                      <span className="font-semibold text-black text-xl">
                        Tổng tiền:
                      </span>
                      <span>
                        <i className="text-gray-500 text-lg">
                          {child.subTotal.toLocaleString("vi-VN")}₫
                        </i>
                      </span>
                    </p>
                    <Tooltip
                      content={
                        <div className="w-90">
                          <Typography color="white" className="font-medium">
                            Tổng tiền:
                          </Typography>
                          <Typography
                            variant="small"
                            color="white"
                            className="font-normal opacity-80"
                          >
                            Tổng tiền được tính: Số lượng x Đơn giá bán x số
                            ngày thuê
                          </Typography>
                        </div>
                      }
                    >
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                        strokeWidth={2}
                        className="h-4 w-4 cursor-pointer text-blue-gray-500"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z"
                        />
                      </svg>
                    </Tooltip>
                  </div>
                  {orderStatus === "Đã giao hàng" &&
                    compareDates(child.rentalEndDate, new Date()) && (
                      <ExtensionRequestButton
                        parentOrder={orderDetail}
                        selectedChildOrder={child}
                        setExtendReload={setExtendReload}
                      />
                    )}
                  {child.orderStatus === "Đã giao hàng" ||
                  child.orderStatus === "Đang gia hạn" ? (
                    <ReturnRentalProductButton selectedOrderId={child.id} />
                  ) : null}
                </div>
              </div>
            ))
          ) : (
            <div className="bg-gray-100 p-4 rounded-lg shadow-sm">
              <ExtensionStatusMessage
                extensionStatus={orderDetail.extensionStatus}
              />
              <div className="flex flex-col md:flex-row gap-4">
                <img
                  src={imgAvatarPath}
                  alt={orderDetail.productName}
                  className="w-full md:w-32 h-32 object-cover rounded"
                />
                <div className="flex-grow">
                  <h4 className="font-semibold text-lg mb-2 text-orange-500">
                    <Link to={`/product/${orderDetail.productCode}`}>
                      {orderDetail.productName}
                    </Link>
                  </h4>
                  <div className="grid grid-cols-2 gap-2">
                    <p>
                      <span className="font-semibold">Màu sắc:</span>{" "}
                      <i>{orderDetail.color}</i>
                    </p>
                    <p>
                      <span className="font-semibold">Số lượng:</span>{" "}
                      <i>{orderDetail.quantity}</i>
                    </p>
                    <p>
                      <span className="font-semibold">Tình trạng:</span>{" "}
                      <i>{orderDetail.condition}%</i>
                    </p>
                    <p>
                      <span className="font-semibold">Giá thuê:</span>{" "}
                      <i>{orderDetail.rentPrice.toLocaleString("vi-VN")}₫</i>
                    </p>
                    <p>
                      <span className="font-semibold">Kích thước:</span>{" "}
                      <i>{orderDetail.size}</i>
                    </p>
                    <p>
                      <span className="font-semibold">Thời gian thuê:</span>{" "}
                      <i>
                        {new Date(
                          orderDetail.rentalStartDate
                        ).toLocaleDateString()}{" "}
                        -{" "}
                        {new Date(
                          orderDetail.rentalEndDate
                        ).toLocaleDateString()}
                      </i>
                    </p>
                    {orderDetail.extensionCost != 0 && (
                      <p>
                        <span className="font-semibold">Phí gia hạn:</span>{" "}
                        <i>
                          {orderDetail.extensionCost.toLocaleString("vi-VN")}₫
                          (x {orderDetail.extensionDays} ngày)
                        </i>
                      </p>
                    )}
                    {orderDetail.extendedDueDate && (
                      <p>
                        <span className="font-semibold">
                          Thời gian gia hạn:
                        </span>{" "}
                        <i>
                          {new Date(
                            orderDetail.extendedDueDate
                          ).toLocaleDateString()}
                        </i>
                      </p>
                    )}
                  </div>
                </div>
              </div>
              <div className="pt-2 flex justify-between items-center gap-4">
                {/* Phần thông tin Tổng tiền */}
                <div className="flex items-center gap-2">
                  <p className="flex items-center gap-1">
                    <span className="font-semibold text-black text-xl">
                      Tổng tiền:
                    </span>
                    <span>
                      <i className="text-gray-500 text-lg">
                        {orderDetail.subTotal.toLocaleString("vi-VN")}₫
                      </i>
                    </span>
                  </p>
                  <Tooltip
                    content={
                      <div className="w-90">
                        <Typography color="white" className="font-medium">
                          Tổng tiền:
                        </Typography>
                        <Typography
                          variant="small"
                          color="white"
                          className="font-normal opacity-80"
                        >
                          Tổng tiền được tính: Số lượng x Đơn giá bán x số ngày
                          thuê
                        </Typography>
                      </div>
                    }
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      strokeWidth={2}
                      className="h-4 w-4 cursor-pointer text-blue-gray-500"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z"
                      />
                    </svg>
                  </Tooltip>
                </div>

                {/* Button Yêu cầu gia hạn */}
                {orderStatus === "Đã giao hàng" && (
                  <ExtensionRequestButton
                    parentOrder={orderDetail}
                    selectedChildOrder={orderDetail}
                    setExtendReload={setExtendReload}
                  />
                )}
                {orderDetail.orderStatus === "Đã giao hàng" ||
                orderDetail.orderStatus === "Đang gia hạn" ? (
                  <ReturnRentalProductButton selectedOrderId={orderDetail.id} />
                ) : null}
              </div>
            </div>
          )}
        </div>
        <div className="bg-white p-6 rounded-lg shadow-lg mt-6">
          <p className="flex justify-between">
            <b className="text-gray-700 text-lg">Tạm tính: </b>
            <i>{orderDetail.subTotal.toLocaleString("vi-VN")}₫</i>
          </p>
          <p className="flex justify-between">
            <b className="text-gray-700 text-lg">Phí vận chuyển: </b>
            <i className="text-base">
              {orderDetail.totalAmount > 2000000 ||
              orderDetail.deliveryMethod === "Đến cửa hàng nhận" ? (
                <i>Miễn phí vận chuyển</i>
              ) : orderDetail.tranSportFee !== 0 ? (
                <>{`${orderDetail.tranSportFee.toLocaleString("vi-VN")}₫`}</>
              ) : (
                "2Sport sẽ liên hệ và thông báo sau"
              )}
            </i>
          </p>
          {orderDetail.extensionCost > 0 && (
            <p className="flex justify-between ">
              <b className="text-gray-700 text-lg">Phí gia hạn thêm: </b>
              <i className="text-base">
                {orderDetail.extensionCost.toLocaleString("vi-VN")}₫
              </i>
            </p>
          )}
          {orderStatus === "Đã trả sản phẩm" && (
            <p className="flex justify-between">
              <b className=" text-gray-700 text-lg">Phí trễ hạn: </b>
              <i className="text-base">
                {orderDetail.lateFee.toLocaleString("vi-VN")}₫
              </i>
            </p>
          )}
          {orderStatus === "Đã trả sản phẩm" && (
            <p className="flex justify-between">
              <b className=" text-gray-700 text-lg">Phí hư hỏng: </b>
              <i className="text-base">
                {orderDetail.damageFee.toLocaleString("vi-VN")}₫
              </i>
            </p>
          )}
          <p className="text-xl flex justify-between">
            <b>Thành tiền: </b>
            <i className="text-orange-500 font-bold">
              {orderDetail.totalAmount.toLocaleString("vi-VN")}₫
            </i>
          </p>
        </div>
      </div>
    </div>
  );
}
