import React, { useMemo, useState } from 'react';
import { OrderSummary, OrderStatus, TelegramUser, UserProfile } from '../types';

interface ProfilePageProps {
  telegramUser: TelegramUser | null;
  profile: UserProfile | null;
  orders: OrderSummary[];
  loading: boolean;
  onReloadOrders: () => void;
  onEditPersonalData?: () => void;
  onOpenHelp?: () => void;
}

const ProfilePage: React.FC<ProfilePageProps> = ({
  telegramUser,
  profile,
  orders,
  loading,
  onReloadOrders,
  onEditPersonalData,
  onOpenHelp,
}) => {
  const [expandedSection, setExpandedSection] = useState<string | null>(null);

  const displayName = useMemo(() => {
    if (!telegramUser) {
      return 'Гость';
    }
    const name = `${telegramUser.first_name ?? ''} ${telegramUser.last_name ?? ''}`.trim();
    return name || telegramUser.username || 'Пользователь Telegram';
  }, [telegramUser]);

  const secondaryText = useMemo(() => {
    if (profile?.customerPhone) {
      return profile.customerPhone;
    }
    if (telegramUser?.username) {
      return `@${telegramUser.username}`;
    }
    return '—';
  }, [profile?.customerPhone, telegramUser?.username]);

  const toggleSection = (key: string) => {
    setExpandedSection(prev => (prev === key ? null : key));
  };

  const latestOrderDate = profile?.lastOrderAt
    ? new Date(profile.lastOrderAt).toLocaleDateString('ru-RU', {
        day: '2-digit',
        month: 'long',
        year: 'numeric',
      })
    : null;

  const avatar = (() => {
    if (telegramUser?.photo_url) {
      return <img src={telegramUser.photo_url} alt={displayName} className="profile-avatar-image" />;
    }

    const initials = displayName
      .split(' ')
      .map(part => part.trim()[0])
      .filter(Boolean)
      .join('')
      .slice(0, 2)
      .toUpperCase();

    return <div className="profile-avatar-fallback">{initials || 'TG'}</div>;
  })();

  return (
    <div className="page profile-page">
      <div className="profile-header card">
        <div className="profile-avatar">{avatar}</div>

        <div className="profile-info">
          <h2 className="profile-name">{displayName}</h2>
          <div className="profile-username">{secondaryText}</div>

          {latestOrderDate && (
            <div className="profile-meta">Последний заказ: {latestOrderDate}</div>
          )}
          {!profile?.hasOrders && (
            <div className="profile-meta">Заказы пока не оформлялись</div>
          )}
        </div>
      </div>

      <div className="card profile-menu">
        <button
          className={`profile-menu-item ${expandedSection === 'history' ? 'expanded' : ''}`}
          onClick={() => toggleSection('history')}
        >
          <span>История заказов</span>
          <span className="profile-menu-icon">{expandedSection === 'history' ? '▲' : '›'}</span>
        </button>
        {expandedSection === 'history' && (
          <div className="profile-section">
            {loading && <div className="profile-placeholder">Загружаем заказы...</div>}
            {!loading && orders.length === 0 && (
              <div className="profile-placeholder">История пока пустая</div>
            )}
            {!loading && orders.length > 0 && (
              <div className="profile-orders">
                {orders.map(order => {
                  const statusKey = resolveStatusKey(order.status);
                  return (
                  <div key={order.id} className="profile-order-item">
                    <div className="profile-order-header">
                      <span className="profile-order-number">{order.orderNumber}</span>
                      <span className={`profile-order-status status-${statusKey.toLowerCase()}`}>
                        {mapStatus(order.status)}
                      </span>
                    </div>
                    <div className="profile-order-meta">
                      <span>{new Date(order.createdAt).toLocaleDateString('ru-RU')}</span>
                      <span>{formatCurrency(order.totalAmount)}</span>
                    </div>
                    <div className="profile-order-products">
                      {order.items.slice(0, 2).map(item => (
                        <div key={`${order.id}-${item.productId}`} className="profile-order-product">
                          {item.product
                            ? `${item.product.productType} D${item.product.diameter}×${item.product.wallThickness}`
                            : `Товар #${item.productId}`}
                        </div>
                      ))}
                      {order.items.length > 2 && (
                        <div className="profile-order-product profile-order-more">
                          + ещё {order.items.length - 2}
                        </div>
                      )}
                    </div>
                  </div>
                );
                })}
                <button className="button button-secondary button-small" onClick={onReloadOrders}>
                  Обновить
                </button>
              </div>
            )}
          </div>
        )}

        <button
          className={`profile-menu-item ${expandedSection === 'personal' ? 'expanded' : ''}`}
          onClick={() => toggleSection('personal')}
        >
          <span>Персональные данные</span>
          <span className="profile-menu-icon">{expandedSection === 'personal' ? '▲' : '›'}</span>
        </button>
        {expandedSection === 'personal' && (
          <div className="profile-section">
            {profile?.hasOrders ? (
              <div className="profile-details">
                <div>
                  <span className="detail-label">Имя</span>
                  <span className="detail-value">{profile.customerName || '—'}</span>
                </div>
                <div>
                  <span className="detail-label">Телефон</span>
                  <span className="detail-value">{profile.customerPhone || '—'}</span>
                </div>
                <div>
                  <span className="detail-label">Email</span>
                  <span className="detail-value">{profile.customerEmail || '—'}</span>
                </div>
                <div>
                  <span className="detail-label">ИНН</span>
                  <span className="detail-value">{profile.customerInn || '—'}</span>
                </div>
                <div>
                  <span className="detail-label">Адрес доставки</span>
                  <span className="detail-value">{profile.deliveryAddress || '—'}</span>
                </div>
              </div>
            ) : (
              <div className="profile-placeholder">
                Оформите заказ, чтобы сохранить контактные данные
              </div>
            )}
            <button
              className="button button-primary button-small"
              onClick={onEditPersonalData}
            >
              Редактировать
            </button>
          </div>
        )}

        <button
          className={`profile-menu-item ${expandedSection === 'notifications' ? 'expanded' : ''}`}
          onClick={() => toggleSection('notifications')}
        >
          <span>Уведомления</span>
          <span className="profile-menu-icon">{expandedSection === 'notifications' ? '▲' : '›'}</span>
        </button>
        {expandedSection === 'notifications' && (
          <div className="profile-section">
            <div className="profile-placeholder">
              Настройки уведомлений появятся позже. Мы оставим вас в курсе!
            </div>
          </div>
        )}

        <button
          className={`profile-menu-item ${expandedSection === 'help' ? 'expanded' : ''}`}
          onClick={() => {
            toggleSection('help');
            onOpenHelp?.();
          }}
        >
          <span>Помощь</span>
          <span className="profile-menu-icon">{expandedSection === 'help' ? '▲' : '›'}</span>
        </button>
        {expandedSection === 'help' && (
          <div className="profile-section">
            <div className="profile-placeholder">
              Напишите нам в Telegram или позвоните по телефону +7 (800) 000-00-00.
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

function formatCurrency(value: number) {
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency: 'RUB',
    minimumFractionDigits: 0,
  }).format(value);
}

function resolveStatusKey(status: OrderSummary['status']) {
  if (typeof status === 'number') {
    const statusMap: OrderStatus[] = ['New', 'Processing', 'Confirmed', 'Shipped', 'Completed', 'Cancelled'];
    return statusMap[status] ?? 'New';
  }
  return status;
}

function mapStatus(status: OrderSummary['status']) {
  const key = resolveStatusKey(status);
  switch (key) {
    case 'New':
      return 'Новый';
    case 'Processing':
      return 'В обработке';
    case 'Confirmed':
      return 'Подтверждён';
    case 'Shipped':
      return 'Отгружен';
    case 'Completed':
      return 'Завершён';
    case 'Cancelled':
      return 'Отменён';
    default:
      return key;
  }
}

export default ProfilePage;
