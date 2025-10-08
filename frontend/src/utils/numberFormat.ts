export const normalizeDecimalInput = (value: string): string => {
  if (!value) {
    return '';
  }

  const sanitized = value
    .replace(/[^\d.,]/g, '')
    .replace(/\./g, ',');

  const parts = sanitized.split(',');
  const integerPart = parts.shift() ?? '';
  const fractionalPart = parts.join('');
  const hasTrailingComma = sanitized.endsWith(',');

  let resultInteger = integerPart.replace(/^0+(\d)/, '$1');
  if (resultInteger === '') {
    resultInteger = sanitized.startsWith(',') || integerPart !== '' ? '0' : '';
  }

  let result = resultInteger;

  if (fractionalPart.length > 0 || hasTrailingComma) {
    result += ',';
    result += fractionalPart.replace(/,/g, '');
    if (fractionalPart.length === 0 && hasTrailingComma) {
      // keep trailing comma for user input like "1,"
    }
  }

  return result;
};

export const parseDecimalInput = (value: string): number => {
  if (!value) {
    return NaN;
  }

  const normalized = value
    .replace(/\s+/g, '')
    .replace(',', '.')
    .replace(/,$/, '.');

  const parsed = Number.parseFloat(normalized);
  return Number.isFinite(parsed) ? parsed : NaN;
};

export const formatNumberForInput = (value: number, maximumFractionDigits = 2): string => {
  if (!Number.isFinite(value)) {
    return '';
  }

  return value
    .toLocaleString('ru-RU', {
      minimumFractionDigits: 0,
      maximumFractionDigits,
    })
    .replace(/\s+/g, '');
};
