// Типы для API
export interface Product {
  id: number;
  warehouse: string;
  productType: string;
  diameter: number;
  wallThickness: number;
  gost: string;
  steelGrade: string;
  pricePerTon: number;
  weightPerMeter: number;
  availableStockTons: number;
  availableStockMeters: number;
  lastPriceUpdate: string;
  sku?: string;
}

export interface ProductFilter {
  warehouse?: string;
  productType?: string;
  diameterMin?: number;
  diameterMax?: number;
  wallThicknessMin?: number;
  wallThicknessMax?: number;
  gost?: string;
  steelGrade?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface CartItem {
  id: number;
  product: Product;
  quantityMeters?: number;
  quantityTons?: number;
  preferredUnit?: string; // 'meters' или 'tons'
  totalPrice: number;
  discount: number;
  discountPercent?: number; // Процент скидки от бэкенда
}

export interface Cart {
  id: number;
  telegramUserId: number;
  items: CartItem[];
  totalAmount: number;
  totalDiscount: number;
}

export interface OrderItemSummary {
  productId: number;
  quantityMeters: number;
  quantityTons: number;
  unitPrice: number;
  totalPrice: number;
  product?: Product;
}

export type OrderStatus = 'New' | 'Processing' | 'Confirmed' | 'Shipped' | 'Completed' | 'Cancelled';

export interface OrderSummary {
  id: number;
  telegramUserId: number;
  orderNumber: string;
  customerName: string;
  customerPhone: string;
  customerEmail?: string;
  deliveryAddress?: string;
  companyName?: string;
  inn?: string;
  items: OrderItemSummary[];
  totalAmount: number;
  totalDiscount: number;
  status: OrderStatus | number;
  comment?: string;
  createdAt: string;
  processedAt?: string;
}

export interface UserProfile {
  telegramUserId: number;
  customerName?: string;
  customerPhone?: string;
  customerEmail?: string;
  customerInn?: string;
  deliveryAddress?: string;
  companyName?: string;
  lastOrderAt?: string;
  hasOrders: boolean;
}

export interface AddToCartDto {
  telegramUserId: number;
  productId: number;
  quantityMeters?: number;
  quantityTons?: number;
  preferredUnit?: string; // 'meters' или 'tons'
}

export interface CreateOrderDto {
  telegramUserId: number;
  customerName: string;
  customerPhone: string;
  customerEmail?: string;
  customerInn?: string;
  deliveryAddress?: string;
  companyName?: string;
  comment?: string;
}

export interface FilterOption {
  value: string;
  label: string;
}

export interface CustomerData {
  customerName: string;
  customerPhone: string;
  customerEmail: string;
  customerCompany?: string;
  customerInn?: string;
  deliveryAddress?: string;
  comment?: string;
}

// Типы для состояния приложения
export interface AppState {
  products: Product[];
  loading: boolean;
  cart: Cart | null;
  filters: ProductFilter;
  filterOptions: {
    warehouses: FilterOption[];
    productTypes: FilterOption[];
    gosts: FilterOption[];
    steelGrades: FilterOption[];
  };
  currentPage: number;
  totalPages: number;
  totalProducts: number;
}

// Типы для навигации
export type Screen = 'catalog' | 'cart' | 'order' | 'filters' | 'profile';

export interface NavigationState {
  currentScreen: Screen;
  previousScreen?: Screen;
}

// Типы для Telegram WebApp
export interface TelegramUser {
  id: number;
  first_name: string;
  last_name?: string;
  username?: string;
  language_code?: string;
  photo_url?: string;
}

declare global {
  interface Window {
    Telegram: {
      WebApp: {
        initData: string;
        initDataUnsafe: {
          user?: TelegramUser;
        };
        ready: () => void;
        expand: () => void;
        close: () => void;
        MainButton: {
          text: string;
          color: string;
          textColor: string;
          isVisible: boolean;
          isActive: boolean;
          show: () => void;
          hide: () => void;
          enable: () => void;
          disable: () => void;
          setText: (text: string) => void;
          onClick: (callback: () => void) => void;
          offClick: (callback: () => void) => void;
        };
        BackButton: {
          isVisible: boolean;
          show: () => void;
          hide: () => void;
          onClick: (callback: () => void) => void;
          offClick: (callback: () => void) => void;
        };
        HapticFeedback?: {
          impactOccurred: (style: 'light' | 'medium' | 'heavy') => void;
          notificationOccurred: (type: 'error' | 'success' | 'warning') => void;
          selectionChanged: () => void;
        };
        themeParams: {
          bg_color?: string;
          text_color?: string;
          hint_color?: string;
          link_color?: string;
          button_color?: string;
          button_text_color?: string;
        };
        colorScheme: 'light' | 'dark';
      };
    };
  }
}