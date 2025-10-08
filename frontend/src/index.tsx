import React from 'react';
import { createRoot } from 'react-dom/client';
import App from './components/App';
import { apiService } from './api';
import './styles.css';
import './telegram.css';

// Debug helpers for browser console (development only)
declare global {
  interface Window {
    debugAPI: {
      test: () => Promise<boolean>;
      getProducts: () => Promise<unknown>;
      testEndpoint: (endpoint: string) => Promise<unknown>;
    };
  }
}

// Only add debug helpers in development
if (process.env.NODE_ENV === 'development') {
  window.debugAPI = {
    test: () => apiService.testConnection(),
    getProducts: () => apiService.debugRequest('/products?pageSize=5'),
    testEndpoint: (endpoint: string) => apiService.debugRequest(endpoint)
  };

  // eslint-disable-next-line no-console
  console.log('🐛 Debug API available as window.debugAPI');
  // eslint-disable-next-line no-console
  console.log('Examples:');
  // eslint-disable-next-line no-console
  console.log('  await window.debugAPI.test()');
  // eslint-disable-next-line no-console
  console.log('  await window.debugAPI.getProducts()');
  // eslint-disable-next-line no-console
  console.log('  await window.debugAPI.testEndpoint("/products/warehouses")');
}

const container = document.getElementById('root');
if (!container) {
  throw new Error('Root container not found');
}

const root = createRoot(container);
root.render(<App />);