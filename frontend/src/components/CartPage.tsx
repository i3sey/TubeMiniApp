import React from 'react';
import { Cart } from '../types';
import CartItem from './CartItem';

interface CartPageProps {
  cart: Cart | null;
  onUpdateCart: () => void;
  onCheckout: () => void;
  loading: boolean;
}

const CartPage: React.FC<CartPageProps> = ({
  cart,
  onUpdateCart,
  onCheckout,
  loading,
}) => {
  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 0,
    }).format(price);
  };

  if (loading) {
    return (
      <div className="page">
        <div className="loader">
          <div className="spinner"></div>
        </div>
      </div>
    );
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="page">
        <div className="header">
          <h1 className="page-title">Корзина</h1>
        </div>
        
        <div className="empty-state">
          <div className="empty-icon">🛒</div>
          <h3>Корзина пустая</h3>
          <p>Добавьте товары из каталога</p>
        </div>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="header">
        <h1 className="page-title">Корзина</h1>
        <p className="cart-summary">
          {cart.items.length} {cart.items.length === 1 ? 'товар' : 'товара'}
        </p>
      </div>

      <div className="cart-items">
        {cart.items.map((item) => (
          <CartItem
            key={item.id}
            item={item}
            onUpdate={onUpdateCart}
          />
        ))}
      </div>

      <div className="cart-footer">
        <div className="cart-totals">
          {cart.totalDiscount > 0 && (
            <div className="total-row discount">
              <span>Скидка:</span>
              <span>{formatPrice(cart.totalDiscount)}</span>
            </div>
          )}
          
          <div className="total-row final">
            <span>Итого:</span>
            <span>{formatPrice(cart.totalAmount)}</span>
          </div>
        </div>

        <button
          className="button button-primary button-full"
          onClick={onCheckout}
          disabled={loading}
        >
          Оформить заказ
        </button>
      </div>
    </div>
  );
};

export default CartPage;