// pages/tariffs/TariffsPage.jsx
import React, { useEffect, useState } from 'react';
import {
  Table,
  Button,
  Space,
  Popconfirm,
  message,
  Modal,
  Form,
  Input,
  InputNumber,
  Typography,
  Spin,
} from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { getTariffs, createTariff, updateTariff, deleteTariff } from '../../services/credit';

const { Title } = Typography;

export const TariffsPage = () => {
  const [tariffs, setTariffs] = useState([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingTariff, setEditingTariff] = useState(null); // null – создание, объект – редактирование
  const [form] = Form.useForm();
  const [submitting, setSubmitting] = useState(false);

  // Загрузка списка тарифов
  const fetchTariffs = async () => {
    setLoading(true);
    try {
      const data = await getTariffs();
      setTariffs(data);
    } catch (error) {
      message.error(error.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTariffs();
  }, []);

  // Открыть модалку для создания
  const handleCreate = () => {
    setEditingTariff(null);
    form.resetFields();
    setModalVisible(true);
  };

  // Открыть модалку для редактирования
  const handleEdit = (record) => {
    setEditingTariff(record);
    form.setFieldsValue({
      name: record.name,
      interestRate: record.interestRate,
      maxAmount: record.maxAmount,
      maxTermDays: record.maxTermDays,
    });
    setModalVisible(true);
  };

  // Удаление тарифа
  const handleDelete = async (id) => {
    try {
      await deleteTariff(id);
      message.success('Тариф удалён');
      fetchTariffs(); // обновляем список
    } catch (error) {
      message.error(error.message);
    }
  };

  // Сохранение (создание или обновление)
  const handleSave = async (values) => {
    setSubmitting(true);
    try {
      if (editingTariff) {
        await updateTariff(editingTariff.id, values);
        message.success('Тариф обновлён');
      } else {
        await createTariff(values);
        message.success('Тариф создан');
      }
      setModalVisible(false);
      fetchTariffs();
    } catch (error) {
      message.error(error.message);
    } finally {
      setSubmitting(false);
    }
  };

  // Колонки таблицы
  const columns = [
    {
      title: 'Название',
      dataIndex: 'name',
      key: 'name',
    },
    {
      title: 'Ставка %',
      dataIndex: 'interestRate',
      key: 'interestRate',
      render: (value) => `${value} %`,
    },
    {
      title: 'Макс. сумма',
      dataIndex: 'maxAmount',
      key: 'maxAmount',
      render: (value) => `${value.toLocaleString()} ₽`,
    },
    {
      title: 'Срок (дней)',
      dataIndex: 'maxTermDays',
      key: 'maxTermDays',
      render: (value) => `${value} дн.`,
    },
    {
      title: 'Действия',
      key: 'actions',
      render: (_, record) => (
        <Space>
          <Button
            icon={<EditOutlined />}
            size="small"
            onClick={() => handleEdit(record)}
          >
            Редактировать
          </Button>
          <Popconfirm
            title="Удалить тариф?"
            description="Вы уверены, что хотите удалить этот тариф?"
            okText="Да"
            cancelText="Нет"
            onConfirm={() => handleDelete(record.id)}
          >
            <Button icon={<DeleteOutlined />} size="small" danger>
              Удалить
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Title level={2}>Управление тарифами</Title>
      <Button
        type="primary"
        icon={<PlusOutlined />}
        onClick={handleCreate}
        style={{ marginBottom: 16 }}
      >
        Создать тариф
      </Button>

      {loading ? (
        <Spin size="large" style={{ display: 'block', margin: '50px auto' }} />
      ) : (
        <Table
          dataSource={tariffs}
          columns={columns}
          rowKey="id"
          pagination={{ pageSize: 10 }}
        />
      )}

      {/* Модальное окно создания/редактирования */}
      <Modal
        open={modalVisible}
        title={editingTariff ? 'Редактировать тариф' : 'Создать тариф'}
        onCancel={() => setModalVisible(false)}
        footer={null}
        destroyOnClose
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSave}
          initialValues={{
            interestRate: undefined,
            maxAmount: undefined,
            maxTermDays: undefined,
          }}
        >
          <Form.Item
            name="name"
            label="Название"
            rules={[{ required: true, message: 'Введите название тарифа' }]}
          >
            <Input />
          </Form.Item>

          <Form.Item
            name="interestRate"
            label="Процентная ставка (%)"
            rules={[
              { required: true, message: 'Введите ставку' },
              { type: 'number', min: 0.01, max: 100, message: 'Ставка от 0.01 до 100' },
            ]}
          >
            <InputNumber style={{ width: '100%' }} step={0.1} precision={2} />
          </Form.Item>

          <Form.Item
            name="maxAmount"
            label="Максимальная сумма"
            rules={[
              { required: true, message: 'Введите максимальную сумму' },
              { type: 'number', min: 0.01, message: 'Сумма должна быть больше 0' },
            ]}
          >
            <InputNumber style={{ width: '100%' }} step={1000} precision={2} />
          </Form.Item>

          <Form.Item
            name="maxTermDays"
            label="Максимальный срок (дней)"
            rules={[
              { required: true, message: 'Введите срок в днях' },
              { type: 'number', min: 1, message: 'Срок должен быть не менее 1 дня' },
            ]}
          >
            <InputNumber style={{ width: '100%' }} step={1} precision={0} />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Space>
              <Button onClick={() => setModalVisible(false)}>Отмена</Button>
              <Button type="primary" htmlType="submit" loading={submitting}>
                {editingTariff ? 'Сохранить' : 'Создать'}
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};