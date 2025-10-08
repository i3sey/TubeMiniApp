import React, { useState, useEffect, useMemo } from 'react';
import { Product, ProductFilter, Cart, TelegramUser, Screen, FilterOption, CustomerData, UserProfile, OrderSummary } from '../types';
import { apiService } from '../api';
import ProductList from './ProductList';
import CartPage from './CartPage';
import OrderPage from './OrderPage';
import FilterModal from './FilterModal';
import BottomNavigation from './BottomNavigation';
import LoadingSpinner from './LoadingSpinner';
import ProfilePage from './ProfilePage';

const App: React.FC = () => {
  // Состояние приложения
  const [currentScreen, setCurrentScreen] = useState<Screen>('catalog');
  const [products, setProducts] = useState<Product[]>([]);
  const [totalProductsCount, setTotalProductsCount] = useState(0);
  const [cart, setCart] = useState<Cart | null>(null);
  const [loading, setLoading] = useState(false);
  const [profileLoading, setProfileLoading] = useState(false);
  const [ordersLoading, setOrdersLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [telegramUser, setTelegramUser] = useState<TelegramUser | null>(null);
  const [showFilters, setShowFilters] = useState(false);
  const [userProfile, setUserProfile] = useState<UserProfile | null>(null);
  const [userOrders, setUserOrders] = useState<OrderSummary[]>([]);
  
  // Фильтры
  const [filters, setFilters] = useState<ProductFilter>({
    pageNumber: 1,
    pageSize: 50,
  });
  
  const [filterOptions, setFilterOptions] = useState<{
    warehouses: FilterOption[];
    productTypes: FilterOption[];
    gosts: FilterOption[];
    steelGrades: FilterOption[];
  }>({
    warehouses: [],
    productTypes: [],
    gosts: [],
    steelGrades: [],
  });

  const isTelegramContext = typeof window !== 'undefined' && !!window.Telegram?.WebApp;

  // Инициализация Telegram WebApp
  useEffect(() => {
    if (!isTelegramContext) {
      return;
    }

    const tg = window.Telegram.WebApp;
    tg.ready();
    tg.expand();
      
      // Применяем тему Telegram
      if (tg.colorScheme === 'dark') {
        document.documentElement.setAttribute('data-tg-theme', 'dark');
      }
      
      if (tg.initDataUnsafe?.user) {
        setTelegramUser(tg.initDataUnsafe.user);
      }
      
      // Настройка haptic feedback
      const enableHaptic = () => {
        if (tg.HapticFeedback) {
          tg.HapticFeedback.impactOccurred('light');
        }
      };
      
      // Добавляем haptic feedback к кнопкам
      document.addEventListener('click', (e) => {
        const target = e.target as HTMLElement;
        if (target.classList.contains('button') || target.classList.contains('nav-item')) {
          enableHaptic();
        }
      });
      
      // Настройка кнопки "Назад"
      const handleBackButton = () => {
        if (currentScreen !== 'catalog') {
          setCurrentScreen('catalog');
        } else {
          tg.close();
        }
      };
      
      tg.BackButton.onClick(handleBackButton);
      
    return () => {
      tg.BackButton.offClick(handleBackButton);
    };
  }, [currentScreen, isTelegramContext]);

  // Загрузка данных
  useEffect(() => {
    if (!isTelegramContext) {
      return;
    }

    initializeApp();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isTelegramContext]);

  const initializeApp = async () => {
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log('🚀 Initializing app...');
    }
    
    // Тестируем подключение к API
    const connected = await apiService.testConnection();
    
    if (connected) {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.log('✅ API is available, loading data from API');
      }
    } else {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.log('⚠️ API is not available');
      }
      setError('API недоступен. Пожалуйста, попробуйте позже.');
    }
    
    loadProducts();
    loadFilterOptions();
  };

  useEffect(() => {
    if (!isTelegramContext) {
      return;
    }

    if (telegramUser) {
      loadCart();
      loadUserProfile();
      loadUserOrders();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [telegramUser, isTelegramContext]);

  const loadProducts = async (newFilters?: ProductFilter) => {
    setLoading(true);
    setError(null);
    
    if (process.env.NODE_ENV === 'development') {
      // eslint-disable-next-line no-console
      console.log('🔄 Loading products with filters:', newFilters || filters);
    }
    
    try {
      const filtersToUse = newFilters || filters;
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.log('📡 Attempting API request with filters:', filtersToUse);
      }
      
      const response = await apiService.getProducts(filtersToUse);
      
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.log('✅ API response received:', response);
      }
      setProducts(response.data);
      setTotalProductsCount(response.totalCount);
      
      if (newFilters) {
        setFilters(newFilters);
      }
        
    } catch (err) {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.error('💥 Error in loadProducts:', err);
      }
      setError(err instanceof Error ? err.message : 'Ошибка загрузки продукции');
    } finally {
      setLoading(false);
    }
  };

  const loadFilterOptions = async () => {
    try {
      try {
        const [warehouses, productTypes, gosts, steelGrades] = await Promise.all([
          apiService.getWarehouses(),
          apiService.getProductTypes(),
          apiService.getGOSTs(),
          apiService.getSteelGrades(),
        ]);
        
        setFilterOptions({
          warehouses,
          productTypes,
          gosts,
          steelGrades,
        });
      } catch (apiError) {
        if (process.env.NODE_ENV === 'development') {
          // eslint-disable-next-line no-console
          console.warn('API недоступен для фильтров:', apiError);
        }
        setError('Не удалось загрузить опции фильтров');
      }
    } catch (err) {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.error('Ошибка загрузки опций фильтров:', err);
      }
      setError('Ошибка загрузки опций фильтров');
    }
  };

  const loadCart = async () => {
    if (!telegramUser) return;
    
    try {
      const cartData = await apiService.getCart(telegramUser.id);
      setCart(cartData);
    } catch (err) {
      // Корзина может не существовать - это нормально
      setCart(null);
    }
  };

  const loadUserProfile = async () => {
    if (!telegramUser) return;
    setProfileLoading(true);
    try {
      const profileData = await apiService.getUserProfile(telegramUser.id);
      setUserProfile(profileData);
    } catch (err) {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.warn('Не удалось загрузить профиль пользователя', err);
      }
      setUserProfile(null);
    } finally {
      setProfileLoading(false);
    }
  };

  const loadUserOrders = async () => {
    if (!telegramUser) return;
    setOrdersLoading(true);
    try {
      const ordersData = await apiService.getUserOrders(telegramUser.id);
      setUserOrders(ordersData);
    } catch (err) {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.warn('Не удалось загрузить историю заказов', err);
      }
      setUserOrders([]);
    } finally {
      setOrdersLoading(false);
    }
  };

  const handleAddToCart = async (productId: number, quantityMeters?: number, quantityTons?: number) => {
    if (!telegramUser) {
      setError('Пользователь не авторизован');
      return;
    }

    setLoading(true);
    try {
      // Определяем предпочтительную единицу измерения
      const preferredUnit = quantityMeters ? 'meters' : 'tons';
      
      const updatedCart = await apiService.addToCart({
        telegramUserId: telegramUser.id,
        productId,
        quantityMeters,
        quantityTons,
        preferredUnit,
      });
      setCart(updatedCart);
      
      // Показываем уведомление о добавлении
      if (window.Telegram?.WebApp) {
        window.Telegram.WebApp.MainButton.setText('Товар добавлен в корзину');
        window.Telegram.WebApp.MainButton.show();
        setTimeout(() => {
          window.Telegram.WebApp.MainButton.hide();
        }, 2000);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ошибка добавления в корзину');
    } finally {
      setLoading(false);
    }
  };

  const handleApplyFilters = (newFilters: ProductFilter) => {
    setShowFilters(false);
    loadProducts(newFilters);
  };

  const handleCreateOrder = async (customerData: CustomerData) => {
    if (!telegramUser) {
      setError('Пользователь не авторизован');
      return;
    }

    setLoading(true);
    try {
      await apiService.createOrder({
        telegramUserId: telegramUser.id,
        customerName: customerData.customerName,
        customerPhone: customerData.customerPhone,
        customerEmail: customerData.customerEmail || undefined,
        customerInn: customerData.customerInn || undefined,
        deliveryAddress: customerData.deliveryAddress || undefined,
        companyName: customerData.customerCompany || undefined,
        comment: customerData.comment || undefined,
      });
      
      // Очищаем корзину
      setCart(null);
      setCurrentScreen('catalog');
      await Promise.all([loadUserProfile(), loadUserOrders()]);
      
      // Показываем успешное сообщение
      if (window.Telegram?.WebApp) {
        window.Telegram.WebApp.MainButton.setText('Заказ успешно оформлен!');
        window.Telegram.WebApp.MainButton.show();
        setTimeout(() => {
          window.Telegram.WebApp.MainButton.hide();
        }, 3000);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ошибка создания заказа');
    } finally {
      setLoading(false);
    }
  };

  // Управление видимостью кнопки "Назад"
  useEffect(() => {
    if (!isTelegramContext) {
      return;
    }

    if (currentScreen !== 'catalog') {
      window.Telegram.WebApp.BackButton.show();
    } else {
      window.Telegram.WebApp.BackButton.hide();
    }
  }, [currentScreen, isTelegramContext]);

  // Подсчет количества товаров в корзине
  const cartItemsCount = cart?.items?.length || 0;

  const defaultOrderData = useMemo(() => {
    if (!userProfile) {
      return undefined;
    }

    return {
      customerName: userProfile.customerName ?? '',
      customerPhone: userProfile.customerPhone ?? '',
      customerEmail: userProfile.customerEmail ?? '',
      customerInn: userProfile.customerInn ?? '',
      deliveryAddress: userProfile.deliveryAddress ?? '',
      customerCompany: userProfile.companyName ?? '',
    };
  }, [userProfile]);

  // Перезагружаем корзину при изменении экрана на 'cart'
  useEffect(() => {
    if (!isTelegramContext) {
      return;
    }

    if (currentScreen === 'cart' && telegramUser) {
      loadCart();
    }
  }, [currentScreen, telegramUser, isTelegramContext]);

  // Если приложение открыто не в Telegram, показываем специальное сообщение
  if (!isTelegramContext) {
    return (
      <div className="app">
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          height: '100vh',
          padding: '20px',
          textAlign: 'center',
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          color: 'white'
        }}>
          <div style={{ fontSize: '64px', marginBottom: '20px' }}>📱</div>
          <h1 style={{ marginBottom: '16px', fontSize: '24px' }}>Приложение доступно только в Telegram</h1>
          <p style={{ marginBottom: '24px', opacity: 0.9, lineHeight: 1.5 }}>
            Этот сайт является мини-приложением Telegram.<br/>
            Для использования откройте его через Telegram Bot.
          </p>
          <div style={{ 
            background: 'rgba(255,255,255,0.1)', 
            padding: '16px', 
            borderRadius: '12px',
            fontSize: '14px',
            opacity: 0.8
          }}>
            💡 Найдите наш бот в Telegram и запустите его
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      {error && (
        <div className="error-message">
          {error}
          <button 
            className="button button-small" 
            onClick={() => setError(null)}
            style={{ marginLeft: '8px' }}
          >
            ✕
          </button>
        </div>
      )}

      {loading && <LoadingSpinner />}

      {currentScreen === 'catalog' && (
        <ProductList
          products={products}
          totalCount={totalProductsCount}
          onAddToCart={handleAddToCart}
          onShowFilters={() => setShowFilters(true)}
          loading={loading}
        />
      )}

      {currentScreen === 'cart' && (
        <CartPage
          cart={cart}
          onUpdateCart={loadCart}
          onCheckout={() => setCurrentScreen('order')}
          loading={loading}
        />
      )}

      {currentScreen === 'order' && (
        <OrderPage
          cart={cart}
          onCreateOrder={handleCreateOrder}
          onBack={() => setCurrentScreen('cart')}
          loading={loading}
          defaultCustomerData={defaultOrderData}
        />
      )}

      {currentScreen === 'profile' && (
        <ProfilePage
          telegramUser={telegramUser}
          profile={userProfile}
          orders={userOrders}
          loading={profileLoading || ordersLoading}
          onReloadOrders={loadUserOrders}
          onEditPersonalData={() => setCurrentScreen('order')}
        />
      )}

      {showFilters && (
        <FilterModal
          filters={filters}
          filterOptions={filterOptions}
          onApply={handleApplyFilters}
          onClose={() => setShowFilters(false)}
        />
      )}

      <BottomNavigation
        currentScreen={currentScreen}
        onNavigate={setCurrentScreen}
        cartItemsCount={cartItemsCount}
      />
    </div>
  );
};

export default App;