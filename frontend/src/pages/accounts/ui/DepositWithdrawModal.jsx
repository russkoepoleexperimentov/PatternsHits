import { Modal, Form, Input, InputNumber, Button, message } from 'antd';
import { useSubmit } from 'react-router-dom';
import { useEffect } from 'react';
import { deposit, withdraw } from '../../../services/core';

export const DepositWithdrawModal = ({ open, onClose, onInvalidate, type, accountId }) => {
  const [form] = Form.useForm();

  // Reset form when modal opens/closes or type changes
  useEffect(() => {
    if (open) {
      form.resetFields();
    }
  }, [open, form]);

  const handleFinish = async (values) => {
    const req = type == "deposit" ? deposit : withdraw;

    try{
        var json = await req(accountId, values);
    } catch (e) {
        message.error({ content: e.message })
    }
    onInvalidate();
    onClose();
  };

  const title = type === 'deposit' ? 'Пополнение счета' : 'Снятие со счета';
  const okText = type === 'deposit' ? 'Пополнить' : 'Снять';

  return (
    <Modal
      open={open}
      title={title}
      onCancel={onClose}
      footer={null} // We'll use the form's own buttons
      destroyOnClose
    >
      <Form
        form={form}
        layout="vertical"
        onFinish={handleFinish}
        initialValues={{ amount: undefined, description: '' }}
      >
        <Form.Item
          name="amount"
          label="Сумма"
          rules={[
            { required: true, message: 'Введите сумму' },
            { type: 'number', min: 0.01, message: 'Сумма должна быть больше 0' },
          ]}
        >
          <InputNumber
            style={{ width: '100%' }}
            placeholder="0.00"
            precision={2}
            step={0.01}
            min={0.01}
          />
        </Form.Item>

        <Form.Item
          name="description"
          label="Описание (необязательно)"
        >
          <Input.TextArea rows={3} placeholder="Введите описание транзакции" />
        </Form.Item>

        <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
          <Button onClick={onClose} style={{ marginRight: 8 }}>
            Отмена
          </Button>
          <Button type="primary" htmlType="submit">
            {okText}
          </Button>
        </Form.Item>
      </Form>
    </Modal>
  );
};