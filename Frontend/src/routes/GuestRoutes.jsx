import React from "react";
import { Route, Routes } from "react-router-dom";
import NotFoundPage from "../pages/NotFoundPage";
import GuestOrder from "../pages/GuestOrder";
import GuestRentOrder from "../pages/GuestRentOrder"
import GuestOrderDetail from "../pages/GuestOrderDetail";
import GuestRentOrderDetail from "../pages/GuestRentOrderDetail";
import GuestManagement from "../pages/GuestManagement";
const GuestRoutes = () => {
  return (
    <Routes>
      <Route path="/" element={<GuestManagement />}>
        <Route path="guest-sale-order" element={<GuestOrder />} />
        <Route path="guest-sale-order/:orderCode" element={<GuestOrderDetail />} />
        <Route path="guest-rent-order" element={<GuestRentOrder />} />
        <Route
          path="guest-rent-order/:orderCode"
          element={<GuestRentOrderDetail />}
        />
         <Route path="*" element={<NotFoundPage />} />
      </Route>
     
    </Routes>
  );
};

export default GuestRoutes;