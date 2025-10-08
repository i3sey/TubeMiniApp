import React, { useState } from 'react';
import { Product } from '../types';
import ProductCard from './ProductCard';

interface ProductListProps {
  products: Product[];
  onAddToCart: (productId: number, quantityMeters?: number, quantityTons?: number) => void;
  onShowFilters: () => void;
  loading: boolean;
}

const ProductList: React.FC<ProductListProps> = ({
  products,
  onAddToCart,
  onShowFilters,
  loading,
}) => {
  const [searchTerm, setSearchTerm] = useState('');

  const filteredProducts = products.filter(product =>
    product.productType.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.steelGrade.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.gost.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.warehouse.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="page">
      <div className="header">
        <h1 className="page-title">Каталог продукции</h1>
        
        <div className="search-controls">
          <div className="search-bar">
            <input
              type="text"
              className="input"
              placeholder="Поиск по названию, марке стали, ГОСТ..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          
          <button className="button button-secondary" onClick={onShowFilters}>
            <span className="filter-icon">🔍</span>
            Фильтры
          </button>
        </div>
      </div>

      {loading && (
        <div className="loader">
          <div className="spinner"></div>
        </div>
      )}

      {!loading && filteredProducts.length === 0 && (
        <div className="empty-state">
          <div className="empty-icon">📦</div>
          <h3>Продукция не найдена</h3>
          <p>Попробуйте изменить критерии поиска или фильтры</p>
        </div>
      )}

      <div className="products-list">
        {filteredProducts.map((product) => (
          <ProductCard
            key={product.id}
            product={product}
            onAddToCart={onAddToCart}
          />
        ))}
      </div>

      {!loading && filteredProducts.length > 0 && (
        <div className="results-info">
          Найдено: {filteredProducts.length} товаров
        </div>
      )}
    </div>
  );
};

export default ProductList;