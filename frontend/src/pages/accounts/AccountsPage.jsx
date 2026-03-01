import React, { useEffect, useState } from 'react';
import { Table, Tag, Button, message, Spin, Tooltip, Drawer, Space, Typography, Popconfirm } from 'antd';
import { UserOutlined, DeleteOutlined, CloseOutlined, MoneyCollectOutlined, MinusCircleOutlined, PlusCircleOutlined, BlockOutlined, EyeFilled, EyeOutlined } from '@ant-design/icons';
import { useAuth } from '../../context/authContext';
import { getAccounts, deleteAccount, getAccountTransactions } from '../../services/core';
import { authApiRequest } from '../../services/api';
import dayjs from 'dayjs'; // если есть, иначе использовать new Date()
import { transactionColumns } from './values/transactionTable';
import { DepositWithdrawModal } from './ui/DepositWithdrawModal';

const { Text } = Typography;

export const AccountsPage = () => {
  const [accounts, setAccounts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [usersMap, setUsersMap] = useState({});
  const [selectedAccount, setSelectedAccount] = useState(null);
  const [drawerVisible, setDrawerVisible] = useState(false);
  const [transactions, setTransactions] = useState([]);
  const [transactionsLoading, setTransactionsLoading] = useState(false);

  const [modalStatus, setModalStatus] = useState(null)

  const fetchAccounts = async () => {
    setLoading(true);
    try {
      const data = await getAccounts();
      setAccounts(data);

      // Загружаем данные владельцев
      const uniqueUserIds = [...new Set(data.map(acc => acc.userId).filter(Boolean))];
      if (uniqueUserIds.length > 0) {
        const usersData = await Promise.all(
          uniqueUserIds.map(async (id) => {
            try {
              const res = await authApiRequest(`/api/users/${id}`, { method: 'GET' });
              if (res.ok) return await res.json();
            } catch {
              return null;
            }
            return null;
          })
        );
        const map = {};
        usersData.forEach(u => {
          if (u && u.id) map[u.id] = u;
        });
        setUsersMap(map);
      }
    } catch (error) {
      message.error('Не удалось загрузить счета');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAccounts();
  }, []);

  const handleDelete = async (id, e) => {
    e.stopPropagation(); // предотвращаем открытие drawer при клике на удаление
    try {
      await deleteAccount(id);
      message.success('Счёт удалён');
    } catch (e){
      message.error('Ошибка при удалении счёта: ' + e.message);
    }
      fetchAccounts();
  };

  const handleRowClick = (record) => {
    setSelectedAccount(record);
    setDrawerVisible(true);
    loadTransactions(record.id);
  };

  const loadTransactions = async (accountId) => {
    setTransactionsLoading(true);
    try {
      const data = await getAccountTransactions(accountId); // нужно добавить в core.js
      setTransactions(data);
    } catch (error) {
      message.error('Не удалось загрузить транзакции');
      setTransactions([]);
    } finally {
      setTransactionsLoading(false);
    }
  };

  const closeDrawer = () => {
    setDrawerVisible(false);
    setSelectedAccount(null);
    setTransactions([]);
  };

  const invalidate = () => {
    fetchAccounts();
    if(selectedAccount?.id)
        loadTransactions(selectedAccount?.id)
  }

  const columns = [
    {
      title: 'ID счёта',
      dataIndex: 'id',
      key: 'id',
      render: (id) => <code>{id.substring(0, 8)}...</code>,
    },
    {
      title: 'Баланс',
      dataIndex: 'balance',
      key: 'balance',
      render: (balance) => <Tag color="green">{balance} ₽</Tag>,
    },
    {
      title: 'Статус',
      key: 'status',
      render: (_, record) => {
        if (!record.closedAt) {
          return <Tag color="green">Активен</Tag>;
        } else {
          const closedDate = dayjs(record.closedAt).format('DD.MM.YYYY HH:mm');
          return <Tag color="red">Закрыт {closedDate}</Tag>;
        }
      },
    },
    {
      title: 'Владелец',
      key: 'owner',
      render: (_, record) => {
        const owner = usersMap[record.userId];
        if (!owner) return '—';
        return (
          <Tooltip title={owner.email}>
            <UserOutlined /> {owner.credentials || owner.email}
          </Tooltip>
        );
      },
    },
    {
      title: 'Действия',
      key: 'actions',
      render: (_, record) => (
        <Space>
            <Popconfirm 
            title="Подтвердите действие"
            description="Вы уверены что хотите закрыть этот счёт?"
            okText="Закрыть"
            cancelText="Отмена"
            onConfirm={(e) => handleDelete(record.id, e)}>
                <Button
                    icon={<DeleteOutlined />}
                    danger
                    size="small"
                >Закрыть</Button>
            </Popconfirm>
            <Button
                icon={<EyeOutlined />}
                type='default'
                size="small"
                onClick={() => handleRowClick(record)}
            >Транзакции</Button>
        </Space>
      ),
    },
  ];

  

  return (
    <div>
      <h2>Счета клиентов</h2>
      {loading ? (
        <Spin size="large" style={{ display: 'block', margin: '50px auto' }} />
      ) : (
        <Table
          dataSource={accounts}
          columns={columns}
          rowKey="id"
          pagination={{ pageSize: 10 }}
        />
      )}

      <Drawer
        title={`Транзакции счета ${selectedAccount?.id}`}
        placement="right"
        width={800}
        onClose={closeDrawer}
        open={drawerVisible}
        extra={
          <Space>
            <Button type='primary' onClick={() => setModalStatus({open: true, type: 'deposit'})} icon={<PlusCircleOutlined/>}>Пополнить</Button>
            <Button type='default' onClick={() => setModalStatus({open: true, type: 'withdraw'})} icon={<MinusCircleOutlined/>}>Снять</Button>
          </Space>
        }
      >
        {transactionsLoading ? (
          <Spin />
        ) : (
            <Table
                dataSource={transactions}
                columns={transactionColumns(transactions, selectedAccount)}
                rowKey="id"
                pagination={{ pageSize: 20 }}
                locale={{ emptyText: 'Нет транзакций' }}
            />
        )}
      </Drawer>

      <DepositWithdrawModal 
        accountId={selectedAccount?.id}
        onInvalidate={invalidate}
        open={modalStatus?.open ?? false}
        type={modalStatus?.type ?? 'withdraw'}
        onClose={() => setModalStatus(null)}
      />
    </div>
  );
};