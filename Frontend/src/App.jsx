import { useState } from 'react'
import { Router, Route, Routes } from "react-router-dom";
import './App.css'
import Header from './layouts/Header'
import LandingPage from './pages/LandingPage';
import ProductPage from './pages/ProductPage';
import Productv2Page from './pages/Productv2Page';
import Footer from './layouts/Footer';
import ManageAccount from './pages/ManageAccount';
import { BreadcrumbsDefault } from './layouts/BreadcrumbsDefault';
import NotFoundPage from './pages/NotFoundPage';
import ProductList from './pages/ProductList';
import ProductRoutes from './routes/ProductRoutes';
import Checkout from './pages/Checkout';
import { useSelector } from 'react-redux';
import { selectUser } from './redux/slices/authSlice';
import UserShipment from './components/User/UserShipment';
import UserRoutes from './routes/UserRoutes';
import Dashboard from './components/Admin/Dashboard';
import ManageUser from './components/Admin/ManageUser';
import ContactUs from './pages/ContactUs';
import AboutUs from './pages/AboutUs';
import OrderSuccess from './components/Payment/OrderSuccess';
import OrderCancel from './components/Payment/OrderCancel';
import SidebarStaff from './layouts/SidebarStaff';
import Warehouse from './components/Staff/Warehouse'
import PrivateRoute from './components/PrivateRoute';
import AdminRoutes from './routes/AdminRoutes';
import PlacedOrder from './components/Order/PlacedOrder';
import Cart from './pages/Cart';
import GuestOrder from './pages/GuestOrder';
import GuestOrderDetail from './pages/GuestOrderDetail';
import BranchSystem from './components/BranchButton';
import ListBranchs from './pages/ListBranchs';
import RentalOrder from './components/Rental/RentalOrder';



function App() {
  const user = useSelector(selectUser);
  const isStaffOrAdmin =
    user && (user.role === "staff" || user.role === "Admin");
  return (
    <>
      {/* {!isStaffOrAdmin && ( */}
        <div>
          <Header />
          <div className="z-50 relative">
            <div className="fixed bottom-0 left-0 mb-4 ml-4">
              <div className="bg-blue-500 text-white py-2 px-4 rounded">
                <BranchSystem />
              </div>
            </div>
          </div>
          {/* <BreadcrumbsDefault/> */}
          <Routes>
            <Route path="/" element={<LandingPage />} />
            <Route path="/manage-account/*" element={<UserRoutes />} />
            {/* <Route path="/productv2" element={<Productv2Page />} /> */}
            <Route path="/product/*" element={<ProductRoutes />} />
            <Route path="/cart" element={<Cart />} />
            <Route path="/placed-order" element={<PlacedOrder />} />
            <Route path="/rental-order" element={<RentalOrder />} />
            <Route path="/checkout" element={<Checkout />} />
            <Route path="/guest-order" element={<GuestOrder />} />
            <Route path="/guest-order/:orderId" element={<GuestOrderDetail />} />
            <Route path="/shipment" element={<UserShipment />} />
            <Route path="/branch-system" element={<ListBranchs />} />

            {/* <Route path="/dashboard" element={<Dashboard />} /> */}
            <Route path="/manage-user" element={<ManageUser />} />
            <Route path="/contact-us" element={<ContactUs />} />
            <Route path="/about-us" element={<AboutUs />} />
            <Route path="/order_success" element={<OrderSuccess />} />
            <Route path="/order_cancel" element={<OrderCancel />} />
            <Route path="/employee/warehouse" element={<Warehouse />} />
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
          <Footer />
        </div>
      {/* )}
      <Routes>
        <Route
          path="/admin/*"
          element={
            <PrivateRoute
              allowedRoles={['Admin']}
            >
              <AdminRoutes />
            </PrivateRoute>
          }
        />
      </Routes> */}
    </>
  );
}

export default App;

