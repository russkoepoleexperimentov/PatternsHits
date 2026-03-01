namespace Common.Contracts
{
    public record DepositFundsCommand(Guid UserId, Guid AccountId, decimal Amount, Guid CorrelationId);
    public record DepositFundsResponse(bool Success, string? ErrorMessage);

    public record ProcessExternalPaymentCommand(
        Guid CreditId,
        decimal Amount,
        string ExternalTransactionId,
        DateTime PaymentDate
    );
    public record ProcessExternalPaymentResponse(
        bool Success,
        string? Message,
        Guid? PaymentId
    );

    public record PaymentProcessedEvent(
        Guid PaymentId,
        Guid CreditId,
        decimal Amount,
        bool Success,
        string? FailureReason,
        DateTime ProcessedAt
    );
}