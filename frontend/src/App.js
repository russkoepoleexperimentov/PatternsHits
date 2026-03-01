import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/authContext';
import { PrivateRoute } from './components/privateRoute/PrivateRoute';
import { PublicRoute } from './components/publicRoute/PublicRoute';
import { AppLayout } from './components/layout/AppLayout';
import { HomePage } from './pages/home/HomePage';
import { LoginPage } from './pages/login/LoginPage';
import { ProfilePage } from './pages/profile/ProfilePage';
import { NotFoundPage } from './pages/notFound/NotFoundPage';
import { Spin } from 'antd'; // для индикатора загрузки
import { ForbiddenPage } from './pages/forbidden/ForbiddenPage';
import { AccountsPage } from './pages/accounts/AccountsPage';

function AppRoutes() {
  const { loading } = useAuth();
  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', marginTop: 100 }}>
        <Spin size="large" />
      </div>
    );
  }
  return (
    <Routes>
      <Route
        path="/"
        element={
          <PrivateRoute>
            <HomePage />
          </PrivateRoute>
        }
      />
      <Route
        path="/login"
        element={
          <PublicRoute>
            <LoginPage />
          </PublicRoute>
        }
      />
      <Route
        path="/profile"
        element={
          <PrivateRoute>
            <ProfilePage />
          </PrivateRoute>
        }
      />
      <Route
        path="/accounts"
        element={
          <PrivateRoute>
            <AccountsPage />
          </PrivateRoute>
        }
      />
      <Route path="/forbidden" element={<ForbiddenPage/>} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <AppLayout>
          <AppRoutes />
        </AppLayout>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;