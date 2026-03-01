import { Tag } from 'antd';
import dayjs from 'dayjs';


const typeMap = {
    Unclassified: "неклассифицировано",
    Deposit: "Пополнение ч/з банкомат",
    Withdrawal: "Снятие ч/з банкомат",
    Transfer: "Перевод",
    CreditPayment: "Выплата кредита",
    CreditIncoming: "Взятие кредита"
}

// Колонки для таблицы транзакций с фильтрами
export const transactionColumns = (data, selectedAccount) => {
    // Вычисляем уникальные значения для фильтров
const typeFilters = [...new Set(data.map(t => t.displayType))]
  .map(type => ({
    text: typeMap[type] || type,
    value: type
  }));

const statusFilters = [...new Set(data.map(t => t.status))]
  .map(status => ({
    text: status,
    value: status
  }));
return[
  {
    title: 'Дата',
    dataIndex: 'createdAt',
    key: 'createdAt',
    render: (date) => dayjs(date).format('DD.MM.YYYY HH:mm'),
    // Можно добавить фильтр по дате с помощью filterDropdown,
    // но для простоты здесь не реализовано
  },
  {
    title: 'Тип',
    dataIndex: 'displayType', 
    key: 'type',
    render: (type) => typeMap[type] || typeMap.Unclassified,
    filters: typeFilters,       // массив объектов { text, value }
    onFilter: (value, record) => record.type === value,
  },
  {
    title: 'Сумма',
    dataIndex: 'amount',
    key: 'amount',
    render: (amount, record) => (
      <Tag color={record.targetId == selectedAccount.id ? 'green' : 'red'}>
        {record.targetId == selectedAccount.id ? '+' : '-'} {amount} ₽
      </Tag>
    ),
    // Для числовых фильтров можно использовать filterDropdown с RangePicker,
    // но здесь не реализовано для краткости
  },
  {
    title: 'Описание',
    dataIndex: 'description',
    key: 'description',
    // Можно добавить текстовый поиск через глобальный поиск таблицы,
    // либо через filterDropdown с Input.Search
  },
  {
    title: 'Статус',
    dataIndex: 'status',
    key: 'status',
    render: (status) => (
      <Tag color={{ Pending: 'yellow', Completed: 'green', Failed: 'red' }[status]}>
        {status}
      </Tag>
    ),
    filters: statusFilters,
    onFilter: (value, record) => record.status === value,
  },
]};