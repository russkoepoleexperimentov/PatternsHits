// pages/users/UsersPage.jsx
import React, { useEffect, useState, useCallback } from 'react';
import { Table, Input, Button, Drawer, Spin, message, Typography, Space } from 'antd';
import { SearchOutlined, EyeOutlined } from '@ant-design/icons';
import { useAuth } from '../../context/authContext';
import { searchUsers, getUserById } from '../../services/users';
import debounce from 'lodash/debounce'; // установите lodash или используйте setTimeout

const { Title, Text } = Typography;

export const UsersPage = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedUser, setSelectedUser] = useState(null);
  const [drawerVisible, setDrawerVisible] = useState(false);
  const [userDetailsLoading, setUserDetailsLoading] = useState(false);

  // Загрузка пользователей с учётом поискового запроса
  const fetchUsers = async (query) => {
    setLoading(true);
    try {
      const data = await searchUsers(query);
      setUsers(data);
    } catch (error) {
      message.error(error.message || 'Ошибка загрузки пользователей');
    } finally {
      setLoading(false);
    }
  };

  // Первоначальная загрузка (пустой запрос — все пользователи)
  useEffect(() => {
    fetchUsers('');
  }, []);

  // Debounced поиск (ждём 500 мс после остановки ввода)
  const debouncedSearch = useCallback(
    debounce((query) => {
      fetchUsers(query);
    }, 500),
    []
  );

  const handleSearchChange = (e) => {
    const value = e.target.value;
    setSearchQuery(value);
    debouncedSearch(value);
  };

  // Открытие Drawer с загрузкой деталей пользователя
  const showUserDetails = async (user) => {
    setSelectedUser(user);
    setDrawerVisible(true);
    setUserDetailsLoading(true);
    try {
      // Можно повторно запросить свежие данные, но можно использовать уже имеющиеся
      // Для простоты используем то, что есть в таблице
      // Если нужно точнее — раскомментируйте:
      // const detailed = await getUserById(user.id);
      // setSelectedUser(detailed);
    } catch (error) {
      message.error('Не удалось загрузить детали пользователя');
    } finally {
      setUserDetailsLoading(false);
    }
  };

  const closeDrawer = () => {
    setDrawerVisible(false);
    setSelectedUser(null);
  };

  const columns = [
    {
      title: 'ID',
      dataIndex: 'id',
      key: 'id',
      render: (id) => <code>{id.substring(0, 8)}...</code>,
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
    },
    {
      title: 'Имя',
      dataIndex: 'credentials',
      key: 'credentials',
      render: (text) => text || '—',
    },
    {
      title: 'Действия',
      key: 'actions',
      render: (_, record) => (
        <Button
          icon={<EyeOutlined />}
          size="small"
          onClick={(e) => {
            e.stopPropagation(); // предотвращаем срабатывание клика по строке
            showUserDetails(record);
          }}
        >
          Просмотр
        </Button>
      ),
    },
  ];

  return (
    <div>
      <Title level={2}>Пользователи</Title>

      {/* Поле поиска */}
      <Input
        placeholder="Поиск по имени"
        prefix={<SearchOutlined />}
        value={searchQuery}
        onChange={handleSearchChange}
        style={{ width: 300, marginBottom: 20 }}
        allowClear
      />

      {loading ? (
        <Spin size="large" style={{ display: 'block', margin: '50px auto' }} />
      ) : (
        <Table
          dataSource={users}
          columns={columns}
          rowKey="id"
          pagination={{ pageSize: 10 }}
          onRow={(record) => ({
            onClick: () => showUserDetails(record),
            style: { cursor: 'pointer' },
          })}
        />
      )}

      {/* Боковая панель с деталями пользователя */}
      <Drawer
        title="Информация о пользователе"
        placement="right"
        width={500}
        onClose={closeDrawer}
        open={drawerVisible}
      >
        {userDetailsLoading ? (
          <Spin />
        ) : selectedUser ? (
          <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            <div>
              <Text strong>ID:</Text> <Text>{selectedUser.id}</Text>
            </div>
            <div>
              <Text strong>Email:</Text> <Text>{selectedUser.email}</Text>
            </div>
            <div>
              <Text strong>Имя:</Text> <Text>{selectedUser.credentials || '—'}</Text>
            </div>
            {/* Здесь можно добавить управление ролями, когда появится соответствующий API */}
          </Space>
        ) : (
          <Text>Пользователь не выбран</Text>
        )}
      </Drawer>
    </div>
  );
};