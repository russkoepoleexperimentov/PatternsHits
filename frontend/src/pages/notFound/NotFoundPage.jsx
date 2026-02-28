import React from 'react';
import { Result, Button } from 'antd';
import { Link } from 'react-router-dom';

export const NotFoundPage = () => {
  return (
    <Result
      status="404"
      title="404"
      subTitle="Извините, страница не найдена."
      extra={<Button type="primary"><Link to="/">Вернуться на главную</Link></Button>}
    />
  );
};