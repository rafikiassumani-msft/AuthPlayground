namespace SharedIdentity.Abstractions
{
    public interface ISmsServiceProvider
    {
        Task<MessageResult> SendSmsAnsync(string phoneNumber, string message);
        Task<MessageResult> SendVerificationCodeAsync(string phoneNumber);
        Task<MessageResult> VerifyPhoneNumberAsync(string phoneNumber, string verificationCode);
    }
    public record MessageResult(string Status);
}

