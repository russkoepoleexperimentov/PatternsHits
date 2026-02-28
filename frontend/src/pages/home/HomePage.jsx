import React from 'react';
import { Typography } from 'antd';
import { useAuth } from '../../context/authContext';

const { Title, Paragraph } = Typography;

export const HomePage = () => {
  const { user } = useAuth();
  return (
    <div>
      <Title>Добро пожаловать, {user?.credentials}!</Title>
      <Paragraph>
        Это главная страница, доступная только авторизованным пользователям.
      </Paragraph>
    </div>
  );
};