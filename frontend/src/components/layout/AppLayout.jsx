import React from 'react';
import { Layout, Menu } from 'antd';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/authContext';

const { Header, Content, Footer } = Layout;

export const AppLayout = ({ children }) => {
  const location = useLocation();
  const { user, logout } = useAuth();

  // Если страница логина – показываем только содержимое (без шапки и футера)
  if (location.pathname === '/login') {
    return <>{children}</>;
  }

  const menuItems = [
    { key: '/', label: <Link to="/">Главная</Link> },
    { key: '/accounts', label: <Link to="/accounts">Счета</Link> },
    user ? { key: '/profile', label: <Link to="/profile">Профиль</Link> } : null,
    user
      ? { key: 'logout', label: <span onClick={logout}>Выйти</span> }
      : { key: '/login', label: <Link to="/login">Вход</Link> },
  ].filter(Boolean);

  return (
    <Layout className="layout" style={{ minHeight: '100vh' }}>
      <Header>
        <div className="logo" style={{ float: 'left', color: '#fff', marginRight: 20 }}>
          MyApp
        </div>
        <Menu
          theme="dark"
          mode="horizontal"
          selectedKeys={[location.pathname]}
          items={menuItems}
        />
      </Header>
      <Content style={{ padding: '0 50px', marginTop: 20 }}>
        <div className="site-layout-content" style={{ background: '#fff', padding: 24, minHeight: 280 }}>
          {children}
        </div>
      </Content>
      <Footer style={{ textAlign: 'center' }}>MyApp ©2025</Footer>
    </Layout>
  );
};