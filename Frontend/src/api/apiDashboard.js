import axios from 'axios';

const API_BASE_URL = 'https://twosport-api-offcial-685025377967.asia-southeast1.run.app/api/Order';

export const fetchOrdersAPI = () => {
  return axios.get(`${API_BASE_URL}/get-all-orders`, {
    headers: {
      'Content-Type': 'application/json'
    }
  });
};

export const fetchOrdersbyStatusAPI = (month, status) => {
  return axios.delete(`${API_BASE_URL}/get-orders-sales-by-status`, {
    headers: {
      'Accept': '*/*',
    },
    params: {
      month: month,
      status: status
    }
  });
};