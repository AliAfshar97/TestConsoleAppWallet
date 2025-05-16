using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestConsoleAppWallet
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string baseUrl = "https://localhost:7145"; // Change if different
            string token = await GetTokenAsync(baseUrl);

            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token acquired.");

                decimal balance = await GetBalanceAsync(baseUrl, token);
                Console.WriteLine($"Current Balance: {balance}");

                var success = await DebitAsync(baseUrl, token, 100.00M, "Test Transaction");
                Console.WriteLine(success ? "Transaction complete." : "Transaction failed.");
            }
            else
            {
                Console.WriteLine("Failed to get token.");
            }
        }
        public class LoginViewModel
        {
            [Display(Name = "ایمیل")]
            [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
            [MaxLength(200, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد .")]
            [EmailAddress(ErrorMessage = "ایمیل وارد شده معتبر نمی باشد")]
            public string Email { get; set; }

            [Display(Name = "کلمه عبور")]
            [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
            [MaxLength(200, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد .")]
            public string Password { get; set; }

            [Display(Name = "مرا به خاطر بسپار")]
            public bool RememberMe { get; set; }
        }

        static async Task<string> GetTokenAsync(string baseUrl)
        {
            using (var client = new HttpClient())
            {
                LoginViewModel requestData = new LoginViewModel()
                {
                    Email = "aliafshar76aa@gmail.com",
                    Password = "12345678",
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{baseUrl}/api/auth/login", content);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(json);
                    return result.tokenString;
                }

                Console.WriteLine("Login failed: " + json);
                return null;
            }
        }

        static async Task<decimal> GetBalanceAsync(string baseUrl, string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync($"{baseUrl}/api/wallet/balance");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(json);
                    return result.balance;
                }

                Console.WriteLine("Error fetching balance: " + json);
                return 0;
            }
        }

        static async Task<bool> DebitAsync(string baseUrl, string token, decimal amount, string description)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var data = new
                {
                    amount = amount,
                    description = description
                };

                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{baseUrl}/api/wallet/debit", content);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(json);
                    Console.WriteLine($"New Balance: {result.newBalance}");
                    return true;
                }

                Console.WriteLine("Transaction failed: " + json);
                return false;
            }
        }
    }
}
