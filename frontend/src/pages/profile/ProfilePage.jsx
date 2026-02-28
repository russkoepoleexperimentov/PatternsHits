import React, { useState } from 'react';
import { Typography, Card, Button, Form, Input, message, Tabs } from 'antd';
import { useAuth } from '../../context/authContext';

const { Title, Text } = Typography;
const { TabPane } = Tabs;

export const ProfilePage = () => {
  const { user, logout, updateProfile, changePassword } = useAuth();
  const [profileForm] = Form.useForm();
  const [passwordForm] = Form.useForm();
  const [updating, setUpdating] = useState(false);
  const [changing, setChanging] = useState(false);

  const handleUpdateProfile = async (values) => {
    setUpdating(true);
    const result = await updateProfile(values);
    setUpdating(false);
    if (result.success) {
      message.success('Профиль обновлён');
    } else {
      message.error(result.error);
    }
  };

  const handleChangePassword = async (values) => {
    setChanging(true);
    const result = await changePassword(values.oldPassword, values.newPassword);
    setChanging(false);
    if (result.success) {
      message.success('Пароль изменён');
      passwordForm.resetFields();
    } else {
      message.error(result.error);
    }
  };

  return (
    <Card style={{ maxWidth: 600, margin: '0 auto' }}>
      <Title level={2}>Профиль</Title>
      <Tabs defaultActiveKey="1">
        <TabPane tab="Основное" key="1">
          <div style={{ marginBottom: 20 }}>
            <Text strong>Email: </Text> <Text>{user?.email}</Text><br />
            <Text strong>Имя: </Text> <Text>{user?.credentials}</Text>
          </div>
          <Form
            form={profileForm}
            layout="vertical"
            onFinish={handleUpdateProfile}
            initialValues={{ email: user?.email, credentials: user?.credentials }}
          >
            <Form.Item
              name="email"
              label="Email"
              rules={[{ required: true, type: 'email', message: 'Введите корректный email' }]}
            >
              <Input />
            </Form.Item>
            <Form.Item
              name="credentials"
              label="Имя"
              rules={[{ required: true, message: 'Введите имя' }]}
            >
              <Input />
            </Form.Item>
            <Form.Item>
              <Button type="primary" htmlType="submit" loading={updating}>
                Обновить профиль
              </Button>
            </Form.Item>
          </Form>
        </TabPane>
        <TabPane tab="Смена пароля" key="2">
          <Form
            form={passwordForm}
            layout="vertical"
            onFinish={handleChangePassword}
          >
            <Form.Item
              name="oldPassword"
              label="Старый пароль"
              rules={[{ required: true, message: 'Введите старый пароль' }]}
            >
              <Input.Password />
            </Form.Item>
            <Form.Item
              name="newPassword"
              label="Новый пароль"
              rules={[{ required: true, message: 'Введите новый пароль' }]}
            >
              <Input.Password />
            </Form.Item>
            <Form.Item>
              <Button type="primary" htmlType="submit" loading={changing}>
                Сменить пароль
              </Button>
            </Form.Item>
          </Form>
        </TabPane>
      </Tabs>
      <div style={{ marginTop: 20 }}>
        <Button type="primary" danger onClick={logout}>
          Выйти
        </Button>
      </div>
    </Card>
  );
};