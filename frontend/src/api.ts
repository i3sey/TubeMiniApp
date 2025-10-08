import { Product, ProductFilter, Cart, AddToCartDto, CreateOrderDto, FilterOption, OrderSummary, UserProfile } from './types';

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || '/api';

class ApiService {
  private getTelegramInitData(): string | null {
    if (typeof window === 'undefined') {
      return null;
    }

    return window.Telegram?.WebApp?.initData || null;
  }

  private async request<T>(endpoint: string, options?: RequestInit): Promise<T> {
    const url = `${API_BASE_URL}${endpoint}`;
    
    // Debug: логируем запрос (только в development)
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log('🔄 API Request:', {
        url,
        method: options?.method || 'GET',
        headers: options?.headers,
        body: options?.body
      });
    }

    try {
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      };

      if (options?.headers) {
        if (options.headers instanceof Headers) {
          options.headers.forEach((value, key) => {
            headers[key] = value;
          });
        } else if (Array.isArray(options.headers)) {
          options.headers.forEach(([key, value]) => {
            headers[key] = value;
          });
        } else {
          Object.assign(headers, options.headers);
        }
      }

      const telegramInitData = this.getTelegramInitData();

      if (telegramInitData) {
        headers['X-Telegram-Init-Data'] = telegramInitData;
      } else if (process.env.NODE_ENV !== 'development') {
        throw new Error('Доступ к API разрешен только из Telegram-приложения');
      }

      const response = await fetch(url, {
        headers,
        ...options,
      });

      // Debug: логируем статус ответа (только в development)
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.log('📡 API Response Status:', {
          url,
          status: response.status,
          statusText: response.statusText,
          ok: response.ok
        });
      }

      if (!response.ok) {
        // Попытаемся получить детали ошибки
        let errorDetails = '';
        try {
          const errorText = await response.text();
          errorDetails = errorText;
          if (process.env.NODE_ENV === 'development') {
            // eslint-disable-next-line no-console
            console.error('❌ API Error Details:', {
              url,
              status: response.status,
              statusText: response.statusText,
              errorBody: errorText
            });
          }
        } catch (e) {
          if (process.env.NODE_ENV === 'development') {
            // eslint-disable-next-line no-console
            console.error('❌ Could not read error response body');
          }
        }
        
        throw new Error(`API Error: ${response.status} ${response.statusText}${errorDetails ? ' - ' + errorDetails : ''}`);
      }

      const responseData = await response.json();
      
      // Debug: логируем успешный ответ (только в development)
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.log('✅ API Response Data:', {
          url,
          data: responseData
        });
      }

      return responseData;
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.error('🔥 API Request Failed:', {
          url,
          error: error instanceof Error ? error.message : error
        });
      }
      throw error;
    }
  }

  // Продукция
  async getProducts(filter: ProductFilter = {}) {
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log('🔍 Getting products with filter:', filter);
    }
    
    const queryParams = new URLSearchParams();
    
    Object.entries(filter).forEach(([key, value]: [string, unknown]) => {
      if (value !== undefined && value !== null && value !== '') {
        queryParams.append(key, String(value));
      }
    });

    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log('🔗 Query params:', queryParams.toString());
    }

    const response = await this.request<{
      data: Product[];
      totalCount: number;
      pageNumber: number;
      pageSize: number;
      totalPages: number;
    }>(`/products?${queryParams.toString()}`);
    
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log('📦 Products response:', response);
    }
    return response;
  }

  async getProduct(id: number): Promise<Product> {
    return this.request<Product>(`/products/${id}`);
  }

  // Опции для фильтров
  async getWarehouses(): Promise<FilterOption[]> {
    const warehouses = await this.request<string[]>('/products/warehouses');
    return warehouses.map(warehouse => ({ value: warehouse, label: warehouse }));
  }

  async getProductTypes(): Promise<FilterOption[]> {
    const types = await this.request<string[]>('/products/types');
    return types.map(type => ({ value: type, label: type }));
  }

  async getGOSTs(): Promise<FilterOption[]> {
    const gosts = await this.request<string[]>('/products/gosts');
    return gosts.map(gost => ({ value: gost, label: gost }));
  }

  async getSteelGrades(): Promise<FilterOption[]> {
    const grades = await this.request<string[]>('/products/steel-grades');
    return grades.map(grade => ({ value: grade, label: grade }));
  }

  // Корзина
  async getCart(telegramUserId: number): Promise<Cart> {
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log('🛒 Getting cart for user:', telegramUserId);
    }
    return this.request<Cart>(`/cart/${telegramUserId}`);
  }

  async addToCart(dto: AddToCartDto): Promise<Cart> {
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log('➕ Adding to cart:', dto);
    }
    return this.request<Cart>('/cart/add', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async updateCartItem(itemId: number, quantityMeters?: number, quantityTons?: number): Promise<Cart> {
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log('🔄 Updating cart item:', { itemId, quantityMeters, quantityTons });
    }
    return this.request<Cart>(`/cart/item/${itemId}`, {
      method: 'PUT',
      body: JSON.stringify({ quantityMeters, quantityTons }),
    });
  }

  async removeFromCart(itemId: number): Promise<Cart> {
    return this.request<Cart>(`/cart/item/${itemId}`, {
      method: 'DELETE',
    });
  }

  async clearCart(telegramUserId: number): Promise<void> {
    await this.request<void>(`/cart/${telegramUserId}`, {
      method: 'DELETE',
    });
  }

  // Заказы
  async createOrder(dto: CreateOrderDto): Promise<{ orderId: number; message: string }> {
    return this.request<{ orderId: number; message: string }>('/orders', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async getUserOrders(telegramUserId: number): Promise<OrderSummary[]> {
    return this.request<OrderSummary[]>(`/orders/user/${telegramUserId}`);
  }

  async getUserProfile(telegramUserId: number): Promise<UserProfile | null> {
    try {
      return await this.request<UserProfile>(`/orders/user/${telegramUserId}/profile`);
    } catch (error) {
      if (error instanceof Error && error.message.includes('404')) {
        return null;
      }
      throw error;
    }
  }

  // Скидки
  async getDiscounts(): Promise<unknown[]> {
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log('💰 Getting discounts');
    }
    return this.request<unknown[]>('/discounts');
  }

  // Debug методы
  async testConnection(): Promise<boolean> {
    try {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.log('🔌 Testing API connection...');
      }
      // Пробуем простой запрос
      await this.request('/products?pageSize=1');
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.log('✅ API connection successful');
      }
      return true;
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.error('❌ API connection failed:', error);
      }
      return false;
    }
  }

  async debugRequest(endpoint: string): Promise<unknown> {
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log('🐛 Debug request to:', endpoint);
    }
    try {
      const result = await this.request(endpoint);
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.log('🐛 Debug result:', result);
      }
      return result;
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.error('🐛 Debug error:', error);
      }
      throw error;
    }
  }
}

export const apiService = new ApiService();