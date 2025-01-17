import React, { useState, useEffect, Fragment } from 'react';
import { Dialog, Transition } from '@headlessui/react';
import { useDispatch } from 'react-redux';
import { addUserShipmentDetail } from '../../services/shipmentService';
import { addShipment } from '../../redux/slices/shipmentSlice';
import AddressForm from '../AddressForm';
import { useTranslation } from 'react-i18next';
import { Button, Input } from "@material-tailwind/react";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

export default function AddShipment({ refreshShipments, setReload }) {
  const [isOpen, setIsOpen] = useState(false);
  const [formData, setFormData] = useState({
    fullName: '',
    phoneNumber: '',
    email: '',
    address: ''
  });
  const [address, setAddress] = useState("");
  const token = localStorage.getItem("token");
  const dispatch = useDispatch();
  const { t } = useTranslation();
  const [wardCode, setWardCode] = useState(null); 
  const [districtId, setDistrictId] = useState(null);

  

  const handleAddShipment = async () => {
    try {
      const newShipment = await addUserShipmentDetail(token, { ...formData, address });
      dispatch(addShipment({
        ...newShipment.data, 
        wardCode, 
        districtId 
      }));
      toast.success("Thêm địa chỉ mới thành công!");
      refreshShipments();
      setReload(true);
      closeModal();
    } catch (error) {
      console.error('Error adding shipment:', error);
    }
  };

  useEffect(() => {
  }, [dispatch]);
  
  const handleInputChange = (event) => {
    const { name, value } = event.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleAddressChange = (newAddress, wardCode, districtId) => {
    setAddress(newAddress);
    setWardCode(wardCode);
    setDistrictId(districtId);
  };
  const openModal = () => {
    setIsOpen(true);
  };

  const closeModal = () => {
    setIsOpen(false);
    setFormData({
      fullName: '',
      phoneNumber: '',
      email: '',
      address: ''
    });
  };

  return (
    <>
      <div>
        <button
          className="rounded-lg px-3 py-2 bg-orange-500 text-white  hover:bg-orange-700"
          type="button"
          onClick={openModal}
        >
          + Thêm mới
        </button>
      </div>
      <Transition appear show={isOpen} as={Fragment}>
        <Dialog
          as="div"
          className="relative z-[999999999999]"
          onClose={closeModal}
        >
          <Transition.Child
            as={Fragment}
            enter="ease-out duration-300"
            enterFrom="opacity-0"
            enterTo="opacity-100"
            leave="ease-in duration-200"
            leaveFrom="opacity-100"
            leaveTo="opacity-0"
          >
            <Dialog.Overlay className="fixed inset-0 bg-black opacity-50" />
          </Transition.Child>

          <div className="fixed inset-0 overflow-y-auto">
            <div className="flex items-center justify-center min-h-screen">
              <Transition.Child
                as={Fragment}
                enter="ease-out duration-300"
                enterFrom="opacity-0 scale-95"
                enterTo="opacity-100 scale-100"
                leave="ease-in duration-200"
                leaveFrom="opacity-100 scale-100"
                leaveTo="opacity-0 scale-95"
              >
                <Dialog.Panel className="bg-white p-6 rounded-md shadow-xl w-fit">
                  <Dialog.Title className="text-lg font-bold">
                    Thêm địa chỉ mới
                  </Dialog.Title>
                  <div className="mt-4">
                    <label className="block text-sm font-medium text-gray-700">
                      Họ và tên
                    </label>
                    <Input
                      required
                      type="text"
                      name="fullName"
                      value={formData.fullName}
                      onChange={handleInputChange}
                      className="mt-1 block w-full border rounded-md p-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>
                  <div className="mt-4">
                    <label className="block text-sm font-medium text-gray-700">
                      Số điện thoại
                    </label>
                    <Input
                      required
                      type="tel"
                      name="phoneNumber"
                      value={formData.phoneNumber}
                      onChange={handleInputChange}
                      className="mt-1 block w-full border rounded-md p-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>
                  <div className="mt-4">
                    <label className="block text-sm font-medium text-gray-700">
                      Địa chỉ email
                    </label>
                    <Input
                      required
                      type="email"
                      name="email"
                      value={formData.email}
                      onChange={handleInputChange}
                      className="mt-1 block w-full border rounded-md p-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>
                  <div className="mt-4">
                    <label className="block text-sm font-medium text-gray-700">
                      Địa chỉ
                    </label>
                    <AddressForm onAddressChange={handleAddressChange} />
                  </div>
                  <div className="mt-6 flex justify-end">
                    <Button
                      className="bg-gray-500 text-white px-4 py-2 rounded-md mr-2"
                      onClick={closeModal}
                    >
                      Hủy
                    </Button>
                    <Button
                      className="bg-orange-500 text-white px-4 py-2 rounded-md hover:bg-orange-700"
                      onClick={handleAddShipment}
                    >
                      Xác nhận
                    </Button>
                  </div>
                </Dialog.Panel>
              </Transition.Child>
            </div>
          </div>
        </Dialog>
      </Transition>
    </>
  );
}
