
namespace Core.Application.Dtos
{
    // тип транзакции ИСКЛЮЧИТЕЛЬНО для отображения в UI
    public enum TransactionType
    {
        Unclassified,
        Deposit,
        Withdrawal,
        Transfer,
        CreditPayment,
        CreditIncoming
    }
}
