import React from 'react';
import { Layout, Menu } from 'antd';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/authContext';
import { AccountBookOutlined, ContactsOutlined, ExceptionOutlined, HomeOutlined, ProfileOutlined } from '@ant-design/icons';

const { Header, Content, Footer } = Layout;

export const AppLayout = ({ children }) => {
  const location = useLocation();
  const { user, logout } = useAuth();

  // Если страница логина – показываем только содержимое (без шапки и футера)
  if (location.pathname === '/login') {
    return <>{children}</>;
  }

  const menuItems = [
    { key: '/', icon: <HomeOutlined/>, label: <Link to="/">Главная</Link> },
    { key: '/users', icon: <ContactsOutlined/>, label: <Link to="/users">Пользователи</Link> },
    { key: '/accounts', icon: <AccountBookOutlined/>, label: <Link to="/accounts">Счета</Link> },
    user ? { key: '/profile', icon: <ProfileOutlined/>, label: <Link to="/profile">Профиль</Link> } : null,
    user
      ? { key: 'logout', icon: <ExceptionOutlined/>, label: <span onClick={logout}>Выйти</span> }
      : { key: '/login', label: <Link to="/login">Вход</Link> },
  ].filter(Boolean);

  return (
    <Layout className="layout" style={{ minHeight: '100vh' }}>
      <Header>
        <div className="logo" style={{ float: 'left', color: '#fff', marginRight: 20 }}>
          Банк 
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
      <Footer style={{ textAlign: 'center' }}>Банк</Footer>
    </Layout>
  );
};