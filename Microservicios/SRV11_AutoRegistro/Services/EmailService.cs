using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SRV11_AutoRegistro.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarCorreoConfirmacionAsync(
            string destino,
            string enlaceConfirmacion)
        {
            if (string.IsNullOrWhiteSpace(destino))
            {
                throw new ArgumentException(
                    "El correo destino es requerido.",
                    nameof(destino));
            }

            if (string.IsNullOrWhiteSpace(enlaceConfirmacion))
            {
                throw new ArgumentException(
                    "El enlace de confirmación es requerido.",
                    nameof(enlaceConfirmacion));
            }

            var smtpServer =
                _configuration["Email:SmtpServer"];

            var smtpPort =
                _configuration.GetValue<int>("Email:SmtpPort");

            var smtpUser =
                _configuration["Email:SmtpUser"];

            var smtpPassword =
                _configuration["Email:SmtpPassword"];

            if (string.IsNullOrWhiteSpace(smtpServer) ||
                string.IsNullOrWhiteSpace(smtpUser) ||
                string.IsNullOrWhiteSpace(smtpPassword) ||
                smtpPort <= 0)
            {
                throw new InvalidOperationException(
                    "La configuración SMTP está incompleta.");
            }

            var mensaje = new MimeMessage();

            mensaje.From.Add(
                MailboxAddress.Parse(smtpUser));

            mensaje.To.Add(
                MailboxAddress.Parse(destino));

            mensaje.Subject = "Confirmación de cuenta";

            mensaje.Body = new TextPart("html")
            {
                Text =
                $"""
                <h2>Confirmación de cuenta</h2>

                <p>
                    Gracias por registrarse.
                </p>

                <p>
                    Para activar su cuenta haga clic en el siguiente enlace:
                </p>

                <p>
                    <a href="{enlaceConfirmacion}">
                        Confirmar cuenta
                    </a>
                </p>

                <p>
                    Si usted no realizó este registro, puede ignorar este correo.
                </p>
                """
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                smtpServer,
                smtpPort,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                smtpUser,
                smtpPassword);

            await smtp.SendAsync(mensaje);

            await smtp.DisconnectAsync(true);
        }
    }
}