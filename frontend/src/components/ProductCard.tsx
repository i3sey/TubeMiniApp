import React, { useState } from 'react';
import { Product } from '../types';
import { normalizeDecimalInput, parseDecimalInput } from '../utils/numberFormat';

interface ProductCardProps {
  product: Product;
  onAddToCart: (productId: number, quantityMeters?: number, quantityTons?: number) => void;
}

const ProductCard: React.FC<ProductCardProps> = ({ product, onAddToCart }) => {
  const [showAddForm, setShowAddForm] = useState(false);
  const [quantityMeters, setQuantityMeters] = useState<string>('');
  const [quantityTons, setQuantityTons] = useState<string>('');
  const [measurementType, setMeasurementType] = useState<'meters' | 'tons'>('meters');
  const [error, setError] = useState<string>('');

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

  const hasWeightPerMeter = product.weightPerMeter > 0;

  const tonsPerMeterFallback = product.availableStockMeters > 0
    ? product.availableStockTons / product.availableStockMeters
    : 0;

  const tonsPerMeter = hasWeightPerMeter
    ? product.weightPerMeter / 1000
    : tonsPerMeterFallback;

  const pricePerMeter = tonsPerMeter > 0
    ? product.pricePerTon * tonsPerMeter
    : 0;

  const pricePerMeterLabel = pricePerMeter > 0
    ? formatPrice(pricePerMeter)
    : '—';

  const convertMetersToTons = (meters: number) => {
    if (meters <= 0) return 0;
    if (hasWeightPerMeter) {
      return (meters * product.weightPerMeter) / 1000;
    }
    return meters * tonsPerMeterFallback;
  };

  const convertTonsToMeters = (tons: number) => {
    if (tons <= 0) return 0;
    if (hasWeightPerMeter) {
      return (tons * 1000) / product.weightPerMeter;
    }
    return tonsPerMeterFallback > 0 ? tons / tonsPerMeterFallback : 0;
  };

  const calculatePrice = () => {
    if (measurementType === 'meters' && quantityMeters) {
      const meters = parseDecimalInput(quantityMeters);
      if (Number.isNaN(meters) || meters <= 0) return 0; // Защита от отрицательных значений
      const tons = convertMetersToTons(meters);
      return tons * product.pricePerTon;
    }

    if (measurementType === 'tons' && quantityTons) {
      const tons = parseDecimalInput(quantityTons);
      if (Number.isNaN(tons) || tons <= 0) return 0; // Защита от отрицательных значений
      return tons * product.pricePerTon;
    }

    return 0;
  };

  const validateQuantity = (): boolean => {
    setError('');
    
    if (measurementType === 'meters') {
      const meters = parseDecimalInput(quantityMeters);
      if (Number.isNaN(meters) || meters <= 0) {
        setError('Укажите корректное количество метров');
        return false;
      }
      if (meters > product.availableStockMeters) {
        setError(`Доступно только ${formatNumber(product.availableStockMeters)} м`);
        return false;
      }
    } else if (measurementType === 'tons') {
      const tons = parseDecimalInput(quantityTons);
      if (Number.isNaN(tons) || tons <= 0) {
        setError('Укажите корректное количество тонн');
        return false;
      }
      if (tons > product.availableStockTons) {
        setError(`Доступно только ${formatNumber(product.availableStockTons)} т`);
        return false;
      }
    }
    
    return true;
  };

  const handleAddToCart = () => {
    if (!validateQuantity()) {
      return;
    }
    
    const metersValue = measurementType === 'meters' ? parseDecimalInput(quantityMeters) : NaN;
    const tonsValue = measurementType === 'tons' ? parseDecimalInput(quantityTons) : NaN;

    const meters = Number.isNaN(metersValue) ? undefined : metersValue;
    const tons = Number.isNaN(tonsValue) ? undefined : tonsValue;

    if ((meters ?? 0) > 0 || (tons ?? 0) > 0) {
      onAddToCart(product.id, meters, tons);
      setShowAddForm(false);
      setQuantityMeters('');
      setQuantityTons('');
      setError('');
    }
  };

  const handleQuantityChange = (value: string, type: 'meters' | 'tons') => {
    // Разрешаем только положительные числа и пустую строку
    const normalized = normalizeDecimalInput(value);

    if (type === 'meters') {
      setQuantityMeters(normalized);
    } else {
      setQuantityTons(normalized);
    }

    setError(''); // Сбрасываем ошибку при изменении
  };

  const isAvailable = product.availableStockTons > 0 || product.availableStockMeters > 0;
  const parsedMeters = parseDecimalInput(quantityMeters);
  const parsedTons = parseDecimalInput(quantityTons);

  return (
    <div className="product-card">
      <div className="product-header">
        <h3 className="product-title">
          {product.productType} D{product.diameter}×{product.wallThickness}
        </h3>
        <div className="product-price-block">
          <span className="product-price">
            {formatPrice(product.pricePerTon)}/т
          </span>
          <span className="product-price-secondary">
            {pricePerMeterLabel}/м
          </span>
        </div>
      </div>

      <div className="product-details">
        <div className="product-detail">
          <span className="product-detail-label">Склад</span>
          <span className="product-detail-value">{product.warehouse}</span>
        </div>
        <div className="product-detail">
          <span className="product-detail-label">ГОСТ</span>
          <span className="product-detail-value">{product.gost}</span>
        </div>
        <div className="product-detail">
          <span className="product-detail-label">Марка стали</span>
          <span className="product-detail-value">{product.steelGrade}</span>
        </div>
        <div className="product-detail">
          <span className="product-detail-label">Вес м.п.</span>
          <span className="product-detail-value">{formatNumber(product.weightPerMeter)} кг</span>
        </div>
      </div>

      <div className="product-stock">
        <div className="stock-item">
          <div className="stock-value">{formatNumber(product.availableStockMeters)}</div>
          <div className="stock-label">метров в наличии</div>
        </div>
        <div className="stock-item">
          <div className="stock-value">{formatNumber(product.availableStockTons)}</div>
          <div className="stock-label">тонн в наличии</div>
        </div>
      </div>

      {showAddForm ? (
        <div className="add-to-cart-form">
          <div className="measurement-tabs">
            <button
              className={`tab ${measurementType === 'meters' ? 'active' : ''}`}
              onClick={() => setMeasurementType('meters')}
            >
              Метры
            </button>
            <button
              className={`tab ${measurementType === 'tons' ? 'active' : ''}`}
              onClick={() => setMeasurementType('tons')}
            >
              Тонны
            </button>
          </div>

          <div className="quantity-input">
            {measurementType === 'meters' ? (
              <input
                type="text"
                inputMode="decimal"
                className="input"
                placeholder="Количество в метрах (например, 12,5)"
                value={quantityMeters}
                onChange={(e) => handleQuantityChange(e.target.value, 'meters')}
                maxLength={12}
              />
            ) : (
              <input
                type="text"
                inputMode="decimal"
                className="input"
                placeholder="Количество в тоннах (например, 0,75)"
                value={quantityTons}
                onChange={(e) => handleQuantityChange(e.target.value, 'tons')}
                maxLength={12}
              />
            )}
          </div>

          {error && (
            <div className="error-message">
              {error}
            </div>
          )}

          {(quantityMeters || quantityTons) && !error && (
            <>
              <div className="quantity-conversion">
                {measurementType === 'meters' && quantityMeters && !Number.isNaN(parsedMeters) && parsedMeters > 0 && (
                  <div className="conversion-text">
                    ≈ {formatNumber(convertMetersToTons(parsedMeters))} т
                  </div>
                )}
                {measurementType === 'tons' && quantityTons && !Number.isNaN(parsedTons) && parsedTons > 0 && (
                  <div className="conversion-text">
                    ≈ {formatNumber(convertTonsToMeters(parsedTons))} м
                  </div>
                )}
              </div>
              <div className="price-preview">
                Стоимость: {formatPrice(calculatePrice())}
              </div>
            </>
          )}

          <div className="form-actions">
            <button
              className="button button-secondary"
              onClick={() => setShowAddForm(false)}
            >
              Отмена
            </button>
            <button
              className="button button-primary"
              onClick={handleAddToCart}
              disabled={!quantityMeters && !quantityTons}
            >
              Добавить в корзину
            </button>
          </div>
        </div>
      ) : (
        <button
          className={`button button-full ${isAvailable ? 'button-primary' : 'button-secondary'}`}
          onClick={() => setShowAddForm(true)}
          disabled={!isAvailable}
        >
          {isAvailable ? 'Добавить в корзину' : 'Нет в наличии'}
        </button>
      )}

      {product.sku && (
        <div className="product-sku">
          Артикул: {product.sku}
        </div>
      )}
    </div>
  );
};

export default ProductCard;