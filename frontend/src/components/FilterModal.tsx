import React, { useState } from 'react';
import { ProductFilter, FilterOption } from '../types';

interface FilterModalProps {
  filters: ProductFilter;
  filterOptions: {
    warehouses: FilterOption[];
    productTypes: FilterOption[];
    gosts: FilterOption[];
    steelGrades: FilterOption[];
  };
  onApply: (filters: ProductFilter) => void;
  onClose: () => void;
}

const FilterModal: React.FC<FilterModalProps> = ({
  filters,
  filterOptions,
  onApply,
  onClose,
}) => {
  const [localFilters, setLocalFilters] = useState<ProductFilter>(filters);

  const handleFilterChange = (key: keyof ProductFilter, value: string | number | undefined) => {
    setLocalFilters(prev => ({
      ...prev,
      [key]: value === '' ? undefined : value,
    }));
  };

  const handleApply = () => {
    onApply({ ...localFilters, pageNumber: 1 });
  };

  const handleReset = () => {
    const resetFilters: ProductFilter = {
      pageNumber: 1,
      pageSize: 10,
    };
    setLocalFilters(resetFilters);
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Фильтры</h2>
          <button className="close-button" onClick={onClose}>
            ✕
          </button>
        </div>

        <div className="modal-body">
          <div className="input-group">
            <label className="label">Склад</label>
            <select
              className="select"
              value={localFilters.warehouse || ''}
              onChange={(e) => handleFilterChange('warehouse', e.target.value)}
            >
              <option value="">Все склады</option>
              {filterOptions.warehouses.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          <div className="input-group">
            <label className="label">Вид продукции</label>
            <select
              className="select"
              value={localFilters.productType || ''}
              onChange={(e) => handleFilterChange('productType', e.target.value)}
            >
              <option value="">Все виды</option>
              {filterOptions.productTypes.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          <div className="input-group">
            <label className="label">Диаметр (мм)</label>
            <div className="range-inputs">
              <input
                type="number"
                className="input"
                placeholder="От"
                value={localFilters.diameterMin || ''}
                onChange={(e) => handleFilterChange('diameterMin', parseFloat(e.target.value) || undefined)}
              />
              <span>—</span>
              <input
                type="number"
                className="input"
                placeholder="До"
                value={localFilters.diameterMax || ''}
                onChange={(e) => handleFilterChange('diameterMax', parseFloat(e.target.value) || undefined)}
              />
            </div>
          </div>

          <div className="input-group">
            <label className="label">Толщина стенки (мм)</label>
            <div className="range-inputs">
              <input
                type="number"
                className="input"
                placeholder="От"
                step="0.1"
                value={localFilters.wallThicknessMin || ''}
                onChange={(e) => handleFilterChange('wallThicknessMin', parseFloat(e.target.value) || undefined)}
              />
              <span>—</span>
              <input
                type="number"
                className="input"
                placeholder="До"
                step="0.1"
                value={localFilters.wallThicknessMax || ''}
                onChange={(e) => handleFilterChange('wallThicknessMax', parseFloat(e.target.value) || undefined)}
              />
            </div>
          </div>

          <div className="input-group">
            <label className="label">ГОСТ</label>
            <select
              className="select"
              value={localFilters.gost || ''}
              onChange={(e) => handleFilterChange('gost', e.target.value)}
            >
              <option value="">Все ГОСТы</option>
              {filterOptions.gosts.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          <div className="input-group">
            <label className="label">Марка стали</label>
            <select
              className="select"
              value={localFilters.steelGrade || ''}
              onChange={(e) => handleFilterChange('steelGrade', e.target.value)}
            >
              <option value="">Все марки</option>
              {filterOptions.steelGrades.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="modal-footer">
          <button className="button button-secondary" onClick={handleReset}>
            Сбросить
          </button>
          <button className="button button-primary" onClick={handleApply}>
            Применить
          </button>
        </div>
      </div>
    </div>
  );
};

export default FilterModal;