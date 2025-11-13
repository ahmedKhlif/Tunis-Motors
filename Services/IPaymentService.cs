using TP2.Models;

namespace TP2.Services
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentIntent(decimal amount, string currency = "usd");
        Task<bool> ConfirmPayment(string paymentIntentId);
        Task<string> CreateCheckoutSession(decimal amount, string currency = "usd", string successUrl = "", string cancelUrl = "");
    }
}