import React from 'react';
import { Result, Button } from 'antd';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/authContext';

export const ForbiddenPage = () => {
  const { logout } = useAuth();
  return (
    <Result
      status="403"
      title="403"
      subTitle="Извините, у вас нет доступа к этой странице."
      extra={[
        <Button type="primary" key="home">
          <Link to="/">На главную</Link>
        </Button>,
        <Button key="logout" onClick={logout}>
          Выйти
        </Button>,
      ]}
    />
  );
};