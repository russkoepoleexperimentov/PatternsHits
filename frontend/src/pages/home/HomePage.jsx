import React from 'react';
import { Typography, Card, Row, Col, Space } from 'antd';
import { UserOutlined, AccountBookOutlined, CreditCardOutlined, ReconciliationOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/authContext';

const { Title, Paragraph } = Typography;

export const HomePage = () => {
  const { user } = useAuth();

  const services = [
    {
      title: 'Пользователи',
      icon: <UserOutlined style={{ fontSize: 48, color: '#1890ff' }} />,
      link: '/users',
      description: 'Управление пользователями, блокировка, назначение ролей',
    },
    {
      title: 'Счета',
      icon: <AccountBookOutlined style={{ fontSize: 48, color: '#52c41a' }} />,
      link: '/accounts',
      description: 'Просмотр счетов, транзакции, пополнение и снятие',
    },
    {
      title: 'Тарифы',
      icon: <CreditCardOutlined style={{ fontSize: 48, color: '#faad14' }} />,
      link: '/tariffs',
      description: 'Управление тарифами по кредитам',
    },
    {
      title: 'Кредиты',
      icon: <ReconciliationOutlined style={{ fontSize: 48, color: '#722ed1' }} />,
      link: '/credits',
      description: 'Просмотр кредитных заявок, одобрение и отказ',
    },
  ];

  return (
    <div>
      <Title level={2}>Добро пожаловать, {user?.credentials || user?.email}!</Title>
      <Paragraph type="secondary">
        Это панель управления банковской системой. Выберите раздел для работы.
      </Paragraph>

      <Row gutter={[24, 24]} style={{ marginTop: 32 }}>
        {services.map((service) => (
          <Col xs={24} sm={12} md={8} lg={6} key={service.link}>
            <Link to={service.link}>
              <Card
                hoverable
                style={{ textAlign: 'center', height: '100%' }}
                cover={<div style={{ padding: '20px 0' }}>{service.icon}</div>}
              >
                <Card.Meta
                  title={service.title}
                  description={service.description}
                />
              </Card>
            </Link>
          </Col>
        ))}
      </Row>
    </div>
  );
};