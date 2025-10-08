import React, { useEffect, useState } from 'react';
import { Cart, CustomerData } from '../types';

interface OrderPageProps {
  cart: Cart | null;
  onCreateOrder: (customerData: CustomerData) => void;
  onBack: () => void;
  loading: boolean;
  defaultCustomerData?: Partial<CustomerData>;
}

const OrderPage: React.FC<OrderPageProps> = ({
  cart,
  onCreateOrder,
  onBack,
  loading,
  defaultCustomerData,
}) => {
  const buildInitialFormData = (): CustomerData => ({
    customerName: defaultCustomerData?.customerName ?? '',
    customerPhone: defaultCustomerData?.customerPhone ?? '',
    customerEmail: defaultCustomerData?.customerEmail ?? '',
    customerCompany: defaultCustomerData?.customerCompany ?? '',
    customerInn: defaultCustomerData?.customerInn ?? '',
    deliveryAddress: defaultCustomerData?.deliveryAddress ?? '',
    comment: defaultCustomerData?.comment ?? '',
  });

  const [formData, setFormData] = useState<CustomerData>(buildInitialFormData);

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    setFormData(prev => ({
      ...prev,
      customerName: defaultCustomerData?.customerName ?? prev.customerName,
      customerPhone: defaultCustomerData?.customerPhone ?? prev.customerPhone,
      customerEmail: defaultCustomerData?.customerEmail ?? prev.customerEmail,
      customerCompany: defaultCustomerData?.customerCompany ?? prev.customerCompany,
      customerInn: defaultCustomerData?.customerInn ?? prev.customerInn,
      deliveryAddress: defaultCustomerData?.deliveryAddress ?? prev.deliveryAddress,
      comment: defaultCustomerData?.comment ?? prev.comment,
    }));
  }, [defaultCustomerData]);

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 0,
    }).format(price);
  };

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    if (!formData.customerName.trim()) {
      newErrors.customerName = 'Имя обязательно для заполнения';
    }

    if (!formData.customerPhone.trim()) {
      newErrors.customerPhone = 'Телефон обязателен для заполнения';
    } else if (!/^\+?[78][\d\s\-()]{10,}$/.test(formData.customerPhone.replace(/\s/g, ''))) {
      newErrors.customerPhone = 'Введите корректный номер телефона';
    }

    if (formData.customerEmail && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.customerEmail)) {
      newErrors.customerEmail = 'Введите корректный email';
    }

    // ИНН всегда обязателен
    const innValue = formData.customerInn ?? '';

    if (!innValue.trim()) {
      newErrors.customerInn = 'ИНН обязателен для заполнения';
    } else {
      const innDigits = innValue.replace(/\D/g, '');
      if (innDigits.length !== 10 && innDigits.length !== 12) {
        newErrors.customerInn = 'ИНН должен содержать 10 или 12 цифр';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value,
    }));

    // Убираем ошибку при изменении поля
    if (errors[field]) {
      setErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (validateForm()) {
      onCreateOrder(formData);
    }
  };

  if (!cart || cart.items.length === 0) {
    return (
      <div className="page">
        <div className="header">
          <button className="button button-ghost" onClick={onBack}>
            ← Назад
          </button>
          <h1 className="page-title">Оформление заказа</h1>
        </div>
        
        <div className="empty-state">
          <div className="empty-icon">❌</div>
          <h3>Корзина пуста</h3>
          <p>Добавьте товары для оформления заказа</p>
        </div>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="header">
        <button className="button button-ghost" onClick={onBack}>
          ← Назад к корзине
        </button>
        <h1 className="page-title">Оформление заказа</h1>
      </div>

      {/* Краткая информация о заказе */}
      <div className="order-summary card">
        <h3>Ваш заказ</h3>
        <div className="order-items">
          {cart.items.map((item, index) => (
            <div key={item.id} className="order-item">
              <span className="item-name">
                {item.product.productType} D{item.product.diameter}×{item.product.wallThickness}
              </span>
              <span className="item-quantity">
                {item.quantityMeters ? `${item.quantityMeters} м` : `${item.quantityTons} т`}
              </span>
              <span className="item-price">{formatPrice(item.totalPrice)}</span>
            </div>
          ))}
        </div>
        
        <div className="order-total">
          {cart.totalDiscount > 0 && (
            <div className="total-row">
              <span>Скидка:</span>
              <span className="discount">{formatPrice(cart.totalDiscount)}</span>
            </div>
          )}
          <div className="total-row final">
            <span>Итого:</span>
            <span>{formatPrice(cart.totalAmount)}</span>
          </div>
        </div>
      </div>

      {/* Форма данных клиента */}
      <form onSubmit={handleSubmit} className="order-form">
        <div className="form-section">
          <h3>Контактные данные</h3>
          
          <div className="input-group">
            <label className="label">Имя *</label>
            <input
              type="text"
              className={`input ${errors.customerName ? 'error' : ''}`}
              value={formData.customerName}
              onChange={(e) => handleInputChange('customerName', e.target.value)}
              placeholder="Введите ваше имя"
            />
            {errors.customerName && (
              <div className="error-text">{errors.customerName}</div>
            )}
          </div>

          <div className="input-group">
            <label className="label">Телефон *</label>
            <input
              type="tel"
              className={`input ${errors.customerPhone ? 'error' : ''}`}
              value={formData.customerPhone}
              onChange={(e) => handleInputChange('customerPhone', e.target.value)}
              placeholder="+7 (xxx) xxx-xx-xx"
            />
            {errors.customerPhone && (
              <div className="error-text">{errors.customerPhone}</div>
            )}
          </div>

          <div className="input-group">
            <label className="label">Email</label>
            <input
              type="email"
              className={`input ${errors.customerEmail ? 'error' : ''}`}
              value={formData.customerEmail}
              onChange={(e) => handleInputChange('customerEmail', e.target.value)}
              placeholder="your@email.com"
            />
            {errors.customerEmail && (
              <div className="error-text">{errors.customerEmail}</div>
            )}
          </div>

          <div className="input-group">
            <label className="label">Компания</label>
            <input
              type="text"
              className="input"
              value={formData.customerCompany ?? ''}
              onChange={(e) => handleInputChange('customerCompany', e.target.value)}
              placeholder="Название организации (необязательно)"
            />
          </div>

          <div className="input-group">
            <label className="label">
              ИНН *
            </label>
            <input
              type="text"
              className={`input ${errors.customerInn ? 'error' : ''}`}
              value={formData.customerInn}
              onChange={(e) => handleInputChange('customerInn', e.target.value)}
              placeholder="10 или 12 цифр"
              maxLength={12}
            />
            {errors.customerInn && (
              <div className="error-text">{errors.customerInn}</div>
            )}
          </div>
        </div>

        <div className="form-section">
          <h3>Доставка</h3>
          
          <div className="input-group">
            <label className="label">Адрес доставки</label>
            <textarea
              className="input"
              value={formData.deliveryAddress}
              onChange={(e) => handleInputChange('deliveryAddress', e.target.value)}
              placeholder="Укажите адрес для доставки (необязательно)"
              rows={3}
            />
          </div>

          <div className="input-group">
            <label className="label">Комментарий к заказу</label>
            <textarea
              className="input"
              value={formData.comment}
              onChange={(e) => handleInputChange('comment', e.target.value)}
              placeholder="Дополнительные пожелания или комментарии"
              rows={3}
            />
          </div>
        </div>

        <div className="form-footer">
          <button
            type="submit"
            className="button button-primary button-full"
            disabled={loading}
          >
            {loading ? 'Оформляем заказ...' : `Оформить заказ на ${formatPrice(cart.totalAmount)}`}
          </button>
          
          <p className="form-note">
            После оформления заказа с вами свяжется менеджер для уточнения деталей доставки и оплаты.
          </p>
        </div>
      </form>
    </div>
  );
};

export default OrderPage;