import axios from 'axios';

const API_BASE_URL = 'https://twosportapi-295683427295.asia-southeast2.run.app/api/Import';

export const importAPI = async (quantity, productId, supplierId) => {
  const url = `${API_BASE_URL}/import-product`;
  try {
    const response = await axios.post(url, { quantity, productId, supplierId }, {
      headers: {
        'accept': '*/*',
        'Content-Type': 'application/json'
      }
    });
    return response.data; // or return response if you need more data processing
  } catch (error) {
    throw error;
  }
};
