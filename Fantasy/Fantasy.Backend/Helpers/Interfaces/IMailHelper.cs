using Fantasy.Shared.Responses;

namespace Fantasy.Backend.Helpers.Interfaces
{
    public interface IMailHelper
    {
        ActionResponse<string> SendMail(string toName, string toEmail, string subject, string body, string language);
    }
}