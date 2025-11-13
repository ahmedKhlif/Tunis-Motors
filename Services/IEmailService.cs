namespace TP2.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendOrderConfirmationAsync(string to, string customerName, int orderId, decimal totalAmount);
        Task SendOrderStatusUpdateAsync(string to, string customerName, int orderId, string status);
        Task SendCarSoldNotificationAsync(string to, string sellerName, string carName, decimal price);
        Task SendApprovalRequiredNotificationAsync(string to, string adminName, string carName, string sellerName);
        Task SendEmailConfirmationAsync(string to, string userName, string confirmationLink);
        Task SendPasswordResetAsync(string to, string userName, string resetLink);
    }
}