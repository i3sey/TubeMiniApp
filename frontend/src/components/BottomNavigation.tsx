import React from 'react';
import { Screen } from '../types';

interface BottomNavigationProps {
  currentScreen: Screen;
  onNavigate: (screen: Screen) => void;
  cartItemsCount: number;
}

const BottomNavigation: React.FC<BottomNavigationProps> = ({
  currentScreen,
  onNavigate,
  cartItemsCount,
}) => {
  return (
    <nav className="bottom-nav">
      <button
        className={`nav-item ${currentScreen === 'catalog' ? 'active' : ''}`}
        onClick={() => onNavigate('catalog')}
      >
        <div className="nav-item-icon">🏪</div>
        <div className="nav-item-text">Каталог</div>
      </button>

      <button
        className={`nav-item ${currentScreen === 'cart' ? 'active' : ''}`}
        onClick={() => onNavigate('cart')}
      >
        <div className="nav-item-icon" style={{ position: 'relative' }}>
          🛒
          {cartItemsCount > 0 && (
            <div className="nav-badge">{cartItemsCount}</div>
          )}
        </div>
        <div className="nav-item-text">Корзина</div>
      </button>

      <button
        className={`nav-item ${currentScreen === 'profile' ? 'active' : ''}`}
        onClick={() => onNavigate('profile')}
      >
        <div className="nav-item-icon">👤</div>
        <div className="nav-item-text">Профиль</div>
      </button>
    </nav>
  );
};

export default BottomNavigation;