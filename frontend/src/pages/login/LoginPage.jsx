import React from 'react';
import { Form, Input, Button, Card, message } from 'antd';
import { UserOutlined, LockOutlined } from '@ant-design/icons';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/authContext';

export const LoginPage = () => {
  const navigate = useNavigate();
  const { login } = useAuth();

  const onFinish = async (values) => {
    const { email, password } = values;
    const result = await login(email, password);
    if (result.success) {
      message.success('Вход выполнен успешно');
      navigate('/');
    } else {
      message.error(result.error || 'Ошибка входа');
    }
  };

  return (
    <div style={{ display: 'flex', justifyContent: 'center', marginTop: '50px' }}>
      <Card title="Вход в систему" style={{ width: 400 }}>
        <Form name="login" onFinish={onFinish} autoComplete="off">
          <Form.Item
            name="email"
            rules={[
              { required: true, message: 'Введите email' },
              { type: 'email', message: 'Некорректный email' },
            ]}
          >
            <Input prefix={<UserOutlined />} placeholder="Email" />
          </Form.Item>

          <Form.Item
            name="password"
            rules={[{ required: true, message: 'Введите пароль' }]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Пароль" />
          </Form.Item>

          <Form.Item>
            <Button type="primary" htmlType="submit" block>
              Войти
            </Button>
          </Form.Item>
          <div style={{ textAlign: 'center' }}>
            Нет аккаунта? <Link to="/register">Зарегистрироваться</Link>
          </div>
        </Form>
      </Card>
    </div>
  );
};