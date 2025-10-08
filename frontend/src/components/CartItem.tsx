import React, { useState } from 'react';
import { CartItem as CartItemType } from '../types';
import { apiService } from '../api';
import { formatNumberForInput, normalizeDecimalInput, parseDecimalInput } from '../utils/numberFormat';

interface CartItemProps {
  item: CartItemType;
  onUpdate: () => void;
}

const CartItem: React.FC<CartItemProps> = ({ item, onUpdate }) => {
  const [loading, setLoading] = useState(false);
  const [editMode, setEditMode] = useState(false);
  
  // Определяем единицу измерения на основе preferredUnit
  const isMeters = item.preferredUnit === 'meters';
  const currentQuantity = isMeters ? (item.quantityMeters || 0) : (item.quantityTons || 0);
  const unit = isMeters ? 'м' : 'т';
  
  const [quantity, setQuantity] = useState(() =>
    formatNumberForInput(currentQuantity, isMeters ? 2 : 3) || currentQuantity.toString()
  );

  const parsedQuantityValue = parseDecimalInput(quantity);

  const renderMetaRow = (label: string, value?: string | number | null) => {
    if (value === undefined || value === null || value === '') {
      return null;
    }

    return (
      <div className="cart-item-meta-row">
        <span className="cart-item-meta-label">{label}</span>
        <span className="cart-item-meta-value">{value}</span>
      </div>
    );
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 0,
    }).format(price);
  };

  const formatNumber = (num: number) => {
    return new Intl.NumberFormat('ru-RU', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(num);
  };

  const handleRemove = async () => {
    if (loading) return;
    
    setLoading(true);
    try {
      await apiService.removeFromCart(item.id);
      onUpdate();
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.error('Ошибка удаления товара:', error);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateQuantity = async () => {
    if (loading || !quantity) return;

    setLoading(true);
    try {
      const newQuantity = parseDecimalInput(quantity);
      
      // Валидация: количество должно быть положительным
      if (Number.isNaN(newQuantity) || newQuantity <= 0) {
        alert('Количество должно быть больше нуля');
        setLoading(false);
        return;
      }
      
      // Валидация: не превышать остатки на складе
      if (isMeters) {
        if (newQuantity > item.product.availableStockMeters) {
          alert(`Доступно только ${formatNumber(item.product.availableStockMeters)} м`);
          setLoading(false);
          return;
        }
        await apiService.updateCartItem(item.id, newQuantity, undefined);
      } else {
        if (newQuantity > item.product.availableStockTons) {
          alert(`Доступно только ${formatNumber(item.product.availableStockTons)} т`);
          setLoading(false);
          return;
        }
        await apiService.updateCartItem(item.id, undefined, newQuantity);
      }
      setEditMode(false);
      onUpdate();
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        // eslint-disable-next-line no-console
        console.error('Ошибка обновления количества:', error);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="cart-item">
      <div className="cart-item-header">
        <h4 className="cart-item-title">
          {item.product.productType} D{item.product.diameter}×{item.product.wallThickness}
        </h4>
        <button
          className="remove-button"
          onClick={handleRemove}
          disabled={loading}
        >
          ✕
        </button>
      </div>

      <div className="cart-item-meta">
        {renderMetaRow('Склад', item.product.warehouse)}
        {renderMetaRow('ГОСТ', item.product.gost)}
        {renderMetaRow('Марка стали', item.product.steelGrade)}
      </div>

      <div className="cart-item-quantity">
        {editMode ? (
          <div className="quantity-edit">
            <input
              type="text"
              inputMode="decimal"
              className="input"
              value={quantity}
              onChange={(e) => setQuantity(normalizeDecimalInput(e.target.value))}
              placeholder={isMeters ? '0,0' : '0,00'}
              maxLength={12}
            />
            <span className="unit">{unit}</span>
            <div className="edit-actions">
              <button
                className="button button-small button-secondary"
                onClick={() => {
                  setEditMode(false);
                  setQuantity(formatNumberForInput(currentQuantity, isMeters ? 2 : 3));
                }}
              >
                Отмена
              </button>
              <button
                className="button button-small button-primary"
                onClick={handleUpdateQuantity}
                disabled={
                  loading ||
                  !quantity ||
                  Number.isNaN(parsedQuantityValue) ||
                  parsedQuantityValue <= 0
                }
              >
                Сохранить
              </button>
            </div>
          </div>
        ) : (
          <div
            className="quantity-display"
            onClick={() => {
              setEditMode(true);
              setQuantity(formatNumberForInput(currentQuantity, isMeters ? 2 : 3));
            }}
          >
            <span className="quantity-value">
              {formatNumber(currentQuantity)} {unit}
            </span>
            {isMeters ? (
              <span className="quantity-conversion">
                ≈ {formatNumber(item.quantityTons || 0)} т
              </span>
            ) : (
              <span className="quantity-conversion">
                ≈ {formatNumber(item.quantityMeters || 0)} м
              </span>
            )}
            <span className="edit-hint">Нажмите для изменения</span>
          </div>
        )}
      </div>

      <div className="cart-item-pricing">
        <div className="price-row">
          <span>Цена за тонну:</span>
          <span>{formatPrice(item.product.pricePerTon)}</span>
        </div>
        
        <div className="price-row">
          <span>Количество:</span>
          <span>
            {formatNumber(item.quantityMeters || 0)} м / {formatNumber(item.quantityTons || 0)} т
          </span>
        </div>
        
        {item.discount > 0 && (
          <div className="price-row discount">
            <span>Скидка:</span>
            <span>-{formatPrice(item.discount)}</span>
          </div>
        )}
        
        <div className="price-row total">
          <span>Итого:</span>
          <span>{formatPrice(item.totalPrice)}</span>
        </div>
      </div>

      {loading && (
        <div className="item-loading">
          <div className="spinner"></div>
        </div>
      )}
    </div>
  );
};

export default CartItem;