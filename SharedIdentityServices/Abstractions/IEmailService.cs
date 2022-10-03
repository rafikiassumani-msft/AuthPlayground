
using Microsoft.AspNetCore.Identity.UI.Services;

namespace SharedIdentity.Abstractions
{
    public interface IEmailServiceProvider: IEmailSender
    {
        Task Send2faCodeEmailAsync(string email, string code);
    }
}
