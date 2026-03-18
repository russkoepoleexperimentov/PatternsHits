// pages/credits/CreditsPage.jsx
import React, { useEffect, useState } from 'react';
import {
  Table,
  Tag,
  Button,
  message,
  Spin,
  Drawer,
  Space,
  Typography,
  Modal,
  Form,
  Input,
  InputNumber,
  Tooltip,
} from 'antd';
import {
  EyeOutlined,
  CheckOutlined,
  CloseOutlined,
  UserOutlined,
} from '@ant-design/icons';
import { useAuth } from '../../context/authContext';
import { getCredits, approveCredit, rejectCredit, getCreditById } from '../../services/credit';
import { authApiRequest } from '../../services/api';
import dayjs from 'dayjs';

const { Text } = Typography;

const statusColors = {
  Pending: 'orange',
  Approved: 'green',
  Rejected: 'red',
  Closed: 'gray',
};

export const CreditsPage = () => {
  const [credits, setCredits] = useState([]);
  const [loading, setLoading] = useState(false);
  const [usersMap, setUsersMap] = useState({});

  const [selectedCredit, setSelectedCredit] = useState(null);
  const [drawerVisible, setDrawerVisible] = useState(false);
  const [detailsLoading, setDetailsLoading] = useState(false);

  const [approveModalVisible, setApproveModalVisible] = useState(false);
  const [rejectModalVisible, setRejectModalVisible] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);

  const [approveForm] = Form.useForm();
  const [rejectForm] = Form.useForm();

  const fetchCredits = async () => {
    setLoading(true);
    try {
      const data = await getCredits();
      setCredits(data);

      // Загружаем данные владельцев
      const uniqueUserIds = [...new Set(data.map(c => c.userId).filter(Boolean))];
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
      message.error('Не удалось загрузить кредиты: ' + error.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCredits();
  }, []);

  const handleRowClick = async (record) => {
    setSelectedCredit(record);
    setDrawerVisible(true);
    // Можно загрузить свежие данные, но пока используем то, что есть
    // Если нужно точнее, можно вызвать getCreditById(record.id)
  };

  const closeDrawer = () => {
    setDrawerVisible(false);
    setSelectedCredit(null);
  };

  const handleApprove = async (values) => {
    setActionLoading(true);
    try {
      await approveCredit(selectedCredit.id, values);
      message.success('Кредит одобрен');
      setApproveModalVisible(false);
      approveForm.resetFields();
      // Обновляем список и детали
      fetchCredits();
      if (selectedCredit) {
        const updated = await getCreditById(selectedCredit.id);
        setSelectedCredit(updated);
      }
    } catch (error) {
      message.error('Ошибка при одобрении: ' + error.message);
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async (values) => {
    setActionLoading(true);
    try {
      await rejectCredit(selectedCredit.id, values);
      message.success('Кредит отклонён');
      setRejectModalVisible(false);
      rejectForm.resetFields();
      fetchCredits();
      if (selectedCredit) {
        const updated = await getCreditById(selectedCredit.id);
        setSelectedCredit(updated);
      }
    } catch (error) {
      message.error('Ошибка при отклонении: ' + error.message);
    } finally {
      setActionLoading(false);
    }
  };

  const columns = [
    {
      title: 'ID кредита',
      dataIndex: 'id',
      key: 'id',
      render: (id) => <code>{id.substring(0, 8)}...</code>,
    },
    {
      title: 'Выдан',
      key: 'user',
      render: (_, record) => {
        const owner = usersMap[record.userId];
        if (!owner) return <Text type="secondary">—</Text>;
        return (
          <Tooltip title={owner.email}>
            <Space>
                <UserOutlined />
                {owner.credentials || owner.email}
            </Space>
          </Tooltip>
        );
      },
    },
    {
      title: 'Сумма',
      dataIndex: 'amount',
      key: 'amount',
      render: (val) => `${val.toLocaleString()} ₽`,
    },
    {
      title: 'Остаток долга',
      dataIndex: 'remainingDebt',
      key: 'remainingDebt',
      render: (val) => `${val.toLocaleString()} ₽`,
    },
    {
      title: 'Срок (дней)',
      dataIndex: 'termDays',
      key: 'termDays',
    },
    {
      title: 'Статус',
      dataIndex: 'status',
      key: 'status',
      render: (status) => <Tag color={statusColors[status]}>{status}</Tag>,
    },
    {
      title: 'Дата создания',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date) => dayjs(date).format('DD.MM.YYYY HH:mm'),
    },
    {
      title: 'Действия',
      key: 'actions',
      render: (_, record) => (
        <Button
          icon={<EyeOutlined />}
          size="small"
          onClick={(e) => {
            e.stopPropagation();
            handleRowClick(record);
          }}
        >
          Просмотр
        </Button>
      ),
    },
  ];

  return (
    <div>
      <h2>Управление кредитами</h2>
      {loading ? (
        <Spin size="large" style={{ display: 'block', margin: '50px auto' }} />
      ) : (
        <Table
          dataSource={credits}
          columns={columns}
          rowKey="id"
          pagination={{ pageSize: 10 }}
          onRow={(record) => ({
            onClick: () => handleRowClick(record),
            style: { cursor: 'pointer' },
          })}
        />
      )}

      <Drawer
        title={`Детали кредита ${selectedCredit?.id.substring(0, 8)}...`}
        placement="right"
        width={600}
        onClose={closeDrawer}
        open={drawerVisible}
      >
        {selectedCredit ? (
          <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            <div>
              <Text strong>ID:</Text> <Text>{selectedCredit.id}</Text>
            </div>
            <div>
              <Text strong>Владелец:</Text>{' '}
              <Text>
                {usersMap[selectedCredit.userId]?.email || selectedCredit.userId}
              </Text>
            </div>
            <div>
              <Text strong>Сумма:</Text> <Text>{selectedCredit.amount} ₽</Text>
            </div>
            <div>
              <Text strong>Остаток долга:</Text> <Text>{selectedCredit.remainingDebt} ₽</Text>
            </div>
            <div>
              <Text strong>Срок (дней):</Text> <Text>{selectedCredit.termDays}</Text>
            </div>
            <div>
              <Text strong>Статус:</Text>{' '}
              <Tag color={statusColors[selectedCredit.status]}>{selectedCredit.status}</Tag>
            </div>
            <div>
              <Text strong>Дата создания:</Text>{' '}
              <Text>{dayjs(selectedCredit.createdAt).format('DD.MM.YYYY HH:mm')}</Text>
            </div>
            {selectedCredit.approvedAmount && (
              <div>
                <Text strong>Одобренная сумма:</Text> <Text>{selectedCredit.approvedAmount} ₽</Text>
              </div>
            )}
            {selectedCredit.approvedBy && (
              <div>
                <Text strong>Кем одобрен:</Text> <Text>{selectedCredit.approvedBy}</Text>
              </div>
            )}
            {selectedCredit.rejectionReason && (
              <div>
                <Text strong>Причина отказа:</Text> <Text>{selectedCredit.rejectionReason}</Text>
              </div>
            )}

            {selectedCredit.status === 'Pending' && (
              <Space style={{ marginTop: 20 }}>
                <Button
                  type="primary"
                  icon={<CheckOutlined />}
                  onClick={() => setApproveModalVisible(true)}
                >
                  Одобрить
                </Button>
                <Button
                  danger
                  icon={<CloseOutlined />}
                  onClick={() => setRejectModalVisible(true)}
                >
                  Отказать
                </Button>
              </Space>
            )}
          </Space>
        ) : (
          <Text>Кредит не выбран</Text>
        )}
      </Drawer>

      {/* Модальное окно одобрения */}
      <Modal
        title="Одобрение кредита"
        open={approveModalVisible}
        onCancel={() => setApproveModalVisible(false)}
        footer={null}
        destroyOnClose
      >
        <Form form={approveForm} layout="vertical" onFinish={handleApprove}>
          <Form.Item name="approvedAmount" label="Одобренная сумма (оставьте пустым для суммы заявки)">
            <InputNumber
              style={{ width: '100%' }}
              placeholder="Сумма"
              min={0.01}
              step={0.01}
              precision={2}
            />
          </Form.Item>
          <Form.Item name="comment" label="Комментарий">
            <Input.TextArea rows={3} placeholder="Комментарий (необязательно)" />
          </Form.Item>
          <Form.Item style={{ textAlign: 'right' }}>
            <Space>
              <Button onClick={() => setApproveModalVisible(false)}>Отмена</Button>
              <Button type="primary" htmlType="submit" loading={actionLoading}>
                Одобрить
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>

      {/* Модальное окно отказа */}
      <Modal
        title="Отказ в кредите"
        open={rejectModalVisible}
        onCancel={() => setRejectModalVisible(false)}
        footer={null}
        destroyOnClose
      >
        <Form form={rejectForm} layout="vertical" onFinish={handleReject}>
          <Form.Item name="reason" label="Причина отказа" rules={[{ required: true, message: 'Введите причину' }]}>
            <Input.TextArea rows={3} placeholder="Причина отказа" />
          </Form.Item>
          <Form.Item style={{ textAlign: 'right' }}>
            <Space>
              <Button onClick={() => setRejectModalVisible(false)}>Отмена</Button>
              <Button type="primary" danger htmlType="submit" loading={actionLoading}>
                Отказать
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};