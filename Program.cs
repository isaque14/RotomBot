using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;

namespace RotomBot
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var httpClient = new HttpClient();
            var healthCheckUrl = "https://localhost:7130/health";
            var angularServiceHost = "localhost";
            var angularServicePort = 4200;
            string responseBody = "";

            for(; ;)
            {
                try
                {
                    var response = await httpClient.GetAsync(healthCheckUrl);
                    response.EnsureSuccessStatusCode();

                    responseBody = await response.Content.ReadAsStringAsync();

                    var isAngularServiceOnline = await CheckServiceAvailability(angularServiceHost, angularServicePort);
                    if (!isAngularServiceOnline)
                    {
                        Console.WriteLine("O front-end com Angular está offline.");
                        await SendEmail("O front-end RotomDex está offline.");
                        await Task.Delay(TimeSpan.FromMinutes(10));
                    }

                }
                catch (HttpRequestException ex)
                {
                    SendEmail(ex.Message).Wait();
                    await Task.Delay(TimeSpan.FromMinutes(10));
                }
            }
        }

        static async Task SendEmail(string offlineServices)
        {
            var apiKey = "";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("isaquediniz14@gmail.com", "Rotom Bot");
            var to = new EmailAddress("isaque.silva@ufn.edu.br", "ADM");
            var subject = "Offline applications and services";
            var plainTextContent = "Olá ADM, verifiquei que os seguintes serviços estão fora do ar. Melhor dar uma olhada o quanto antes.";
            var htmlContent = $"<strong>Olá ADM, verifiquei que os serviços estão fora do ar, melhor dar uma olhada o quanto antes. Erro: {offlineServices}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                Console.WriteLine("E-mail enviado com sucesso!");
            }
            else
            {
                Console.WriteLine("Falha ao enviar o e-mail. Status Code: " + response.StatusCode);
            }
        }

        static async Task<bool> CheckServiceAvailability(string host, int port)
        {
            try
            {
                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(host, port);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}