namespace Common.Contracts
{
    public record DepositFundsCommand(Guid UserId, Guid AccountId, decimal Amount, Guid CorrelationId, string Currency);
    public record DepositFundsResponse(bool Success, string? ErrorMessage);

    public record ProcessExternalPaymentCommand(
        Guid CreditId,
        decimal Amount,
        string PaymentId,
        DateTime PaymentDate,
        string SourceCurrency 
    );
    public record ProcessExternalPaymentResponse(
        bool Success,
        string Message,
        Guid? PaymentId,
        decimal AmountInCreditCurrency, 
        string CreditCurrency,        
        decimal? ExchangeRate ); 

    public record PaymentProcessedEvent(
        Guid PaymentId,
        Guid CreditId,
        decimal Amount,
        bool Success,
        string? FailureReason,
        DateTime ProcessedAt
    );
}