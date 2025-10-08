import { create } from 'zustand';
import { Product, Cart, ProductFilter, FilterOption, Screen, TelegramUser } from './types';
import { apiService } from './api';

interface AppStore {
  // Состояние данных
  products: Product[];
  cart: Cart | null;
  loading: boolean;
  error: string | null;
  
  // Фильтры и опции
  filters: ProductFilter;
  filterOptions: {
    warehouses: FilterOption[];
    productTypes: FilterOption[];
    gosts: FilterOption[];
    steelGrades: FilterOption[];
  };
  
  // Пагинация
  currentPage: number;
  totalPages: number;
  totalProducts: number;
  
  // Навигация
  currentScreen: Screen;
  
  // Telegram данные
  telegramUser: TelegramUser | null;
  
  // Действия
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  setCurrentScreen: (screen: Screen) => void;
  setTelegramUser: (user: TelegramUser | null) => void;
  
  // Продукция
  loadProducts: (filters?: ProductFilter) => Promise<void>;
  setFilters: (filters: ProductFilter) => void;
  loadFilterOptions: () => Promise<void>;
  
  // Корзина
  loadCart: () => Promise<void>;
  addToCart: (productId: number, quantityMeters?: number, quantityTons?: number) => Promise<void>;
  updateCartItem: (itemId: number, quantityMeters?: number, quantityTons?: number) => Promise<void>;
  removeFromCart: (itemId: number) => Promise<void>;
  clearCart: () => Promise<void>;
  
  // Заказы
  createOrder: (customerData: {
    customerName: string;
    customerPhone: string;
    customerEmail?: string;
    deliveryAddress?: string;
    comment?: string;
  }) => Promise<number>;
}

export const useAppStore = create<AppStore>((set, get) => ({
  // Начальное состояние
  products: [],
  cart: null,
  loading: false,
  error: null,
  
  filters: {
    pageNumber: 1,
    pageSize: 10,
  },
  filterOptions: {
    warehouses: [],
    productTypes: [],
    gosts: [],
    steelGrades: [],
  },
  
  currentPage: 1,
  totalPages: 0,
  totalProducts: 0,
  
  currentScreen: 'catalog',
  telegramUser: null,
  
  // Основные действия
  setLoading: (loading) => set({ loading }),
  setError: (error) => set({ error }),
  setCurrentScreen: (screen) => set({ currentScreen: screen }),
  setTelegramUser: (user) => set({ telegramUser: user }),
  
  // Продукция
  loadProducts: async (filters) => {
    set({ loading: true, error: null });
    try {
      const newFilters = filters || get().filters;
      const response = await apiService.getProducts(newFilters);
      
      set({
        products: response.data,
        currentPage: response.pageNumber,
        totalPages: response.totalPages,
        totalProducts: response.totalCount,
        filters: newFilters,
        loading: false,
      });
    } catch (error) {
      set({ 
        error: error instanceof Error ? error.message : 'Ошибка загрузки продукции',
        loading: false 
      });
    }
  },
  
  setFilters: (filters) => {
    set({ filters: { ...get().filters, ...filters } });
  },
  
  loadFilterOptions: async () => {
    try {
      const [warehouses, productTypes, gosts, steelGrades] = await Promise.all([
        apiService.getWarehouses(),
        apiService.getProductTypes(),
        apiService.getGOSTs(),
        apiService.getSteelGrades(),
      ]);
      
      set({
        filterOptions: {
          warehouses,
          productTypes,
          gosts,
          steelGrades,
        },
      });
    } catch (error) {
      set({ error: 'Ошибка загрузки опций фильтров' });
    }
  },
  
  // Корзина
  loadCart: async () => {
    const user = get().telegramUser;
    if (!user) return;
    
    try {
      const cart = await apiService.getCart(user.id);
      set({ cart });
    } catch (error) {
      // Корзина может не существовать - это нормально
      set({ cart: null });
    }
  },
  
  addToCart: async (productId, quantityMeters, quantityTons) => {
    const user = get().telegramUser;
    if (!user) {
      set({ error: 'Пользователь не авторизован' });
      return;
    }
    
    set({ loading: true });
    try {
      const cart = await apiService.addToCart({
        telegramUserId: user.id,
        productId,
        quantityMeters,
        quantityTons,
      });
      set({ cart, loading: false });
    } catch (error) {
      set({ 
        error: error instanceof Error ? error.message : 'Ошибка добавления в корзину',
        loading: false 
      });
    }
  },
  
  updateCartItem: async (itemId, quantityMeters, quantityTons) => {
    set({ loading: true });
    try {
      const cart = await apiService.updateCartItem(itemId, quantityMeters, quantityTons);
      set({ cart, loading: false });
    } catch (error) {
      set({ 
        error: error instanceof Error ? error.message : 'Ошибка обновления корзины',
        loading: false 
      });
    }
  },
  
  removeFromCart: async (itemId) => {
    set({ loading: true });
    try {
      const cart = await apiService.removeFromCart(itemId);
      set({ cart, loading: false });
    } catch (error) {
      set({ 
        error: error instanceof Error ? error.message : 'Ошибка удаления из корзины',
        loading: false 
      });
    }
  },
  
  clearCart: async () => {
    const user = get().telegramUser;
    if (!user) return;
    
    set({ loading: true });
    try {
      await apiService.clearCart(user.id);
      set({ cart: null, loading: false });
    } catch (error) {
      set({ 
        error: error instanceof Error ? error.message : 'Ошибка очистки корзины',
        loading: false 
      });
    }
  },
  
  // Заказы
  createOrder: async (customerData) => {
    const user = get().telegramUser;
    if (!user) {
      throw new Error('Пользователь не авторизован');
    }
    
    set({ loading: true });
    try {
      const response = await apiService.createOrder({
        telegramUserId: user.id,
        ...customerData,
      });
      
      // Очищаем корзину после успешного заказа
      set({ cart: null, loading: false });
      
      return response.orderId;
    } catch (error) {
      set({ 
        error: error instanceof Error ? error.message : 'Ошибка создания заказа',
        loading: false 
      });
      throw error;
    }
  },
}));