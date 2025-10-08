#!/bin/bash

# Скрипт для настройки SSL сертификатов Let's Encrypt на VPS
# Использование: ./setup-ssl.sh

set -e

# Настройки
DOMAIN="sa05.me"
EMAIL="your-email@example.com"  # ЗАМЕНИТЕ НА ВАШ EMAIL!
PROJECT_DIR="/home/i3sey/TubeMiniApp"  # Путь к проекту на VPS

echo "🔐 Настройка SSL сертификатов для домена: $DOMAIN"
echo "================================================"

# Проверка прав root
if [ "$EUID" -ne 0 ]; then 
    echo "❌ Пожалуйста, запустите скрипт с sudo"
    exit 1
fi

# Шаг 1: Установка Certbot
echo ""
echo "📦 Шаг 1: Установка Certbot..."
apt update
apt install certbot -y

# Шаг 2: Остановка Docker контейнеров
echo ""
echo "🛑 Шаг 2: Остановка Docker контейнеров..."
cd $PROJECT_DIR
docker-compose down

# Шаг 3: Получение SSL сертификата
echo ""
echo "🔑 Шаг 3: Получение SSL сертификата от Let's Encrypt..."
certbot certonly --standalone \
    -d $DOMAIN \
    -d www.$DOMAIN \
    --non-interactive \
    --agree-tos \
    --email $EMAIL \
    --preferred-challenges http

# Проверка успешности получения сертификата
if [ $? -ne 0 ]; then
    echo "❌ Ошибка при получении SSL сертификата!"
    echo "Проверьте:"
    echo "  - DNS настройки домена"
    echo "  - Доступность портов 80 и 443"
    echo "  - Firewall настройки"
    exit 1
fi

# Шаг 4: Создание директории для сертификатов в проекте
echo ""
echo "📁 Шаг 4: Копирование сертификатов в проект..."
mkdir -p $PROJECT_DIR/nginx/ssl

# Копирование сертификатов
cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem $PROJECT_DIR/nginx/ssl/cert.pem
cp /etc/letsencrypt/live/$DOMAIN/privkey.pem $PROJECT_DIR/nginx/ssl/key.pem

# Установка прав доступа
chmod 644 $PROJECT_DIR/nginx/ssl/cert.pem
chmod 600 $PROJECT_DIR/nginx/ssl/key.pem
chown -R $(stat -c '%U:%G' $PROJECT_DIR) $PROJECT_DIR/nginx/ssl

echo "✅ Сертификаты скопированы!"
ls -lh $PROJECT_DIR/nginx/ssl/

# Шаг 5: Запуск Docker контейнеров
echo ""
echo "🚀 Шаг 5: Запуск Docker контейнеров..."
cd $PROJECT_DIR
docker-compose up -d

# Шаг 6: Настройка автоматического обновления
echo ""
echo "⏰ Шаг 6: Настройка автоматического обновления сертификатов..."

# Создание скрипта обновления
cat > /usr/local/bin/renew-ssl-certificates.sh << 'RENEWAL_SCRIPT'
#!/bin/bash

DOMAIN="sa05.me"
PROJECT_DIR="/home/i3sey/TubeMiniApp"

# Остановка контейнеров
cd $PROJECT_DIR
docker-compose down

# Обновление сертификатов
certbot renew --quiet

# Копирование обновленных сертификатов
cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem $PROJECT_DIR/nginx/ssl/cert.pem
cp /etc/letsencrypt/live/$DOMAIN/privkey.pem $PROJECT_DIR/nginx/ssl/key.pem
chmod 644 $PROJECT_DIR/nginx/ssl/cert.pem
chmod 600 $PROJECT_DIR/nginx/ssl/key.pem

# Запуск контейнеров
docker-compose up -d

echo "$(date): SSL сертификаты обновлены" >> /var/log/ssl-renewal.log
RENEWAL_SCRIPT

chmod +x /usr/local/bin/renew-ssl-certificates.sh

# Добавление задачи в cron (каждый день в 3:00 утра)
(crontab -l 2>/dev/null | grep -v renew-ssl-certificates; echo "0 3 * * * /usr/local/bin/renew-ssl-certificates.sh") | crontab -

echo "✅ Автоматическое обновление настроено (ежедневно в 3:00 AM)"

# Шаг 7: Проверка статуса
echo ""
echo "🔍 Шаг 7: Проверка статуса..."
sleep 5

docker-compose ps

echo ""
echo "✅ SSL сертификаты успешно установлены!"
echo "================================================"
echo ""
echo "📋 Информация:"
echo "  Домен: $DOMAIN"
echo "  Сертификат действителен до: $(openssl x509 -enddate -noout -in $PROJECT_DIR/nginx/ssl/cert.pem | cut -d= -f2)"
echo ""
echo "🌐 Проверьте сайт:"
echo "  https://$DOMAIN"
echo "  https://www.$DOMAIN"
echo ""
echo "🔄 Сертификаты будут автоматически обновляться каждый день в 3:00 AM"
echo ""
echo "📊 Проверить SSL рейтинг:"
echo "  https://www.ssllabs.com/ssltest/analyze.html?d=$DOMAIN"
echo ""
