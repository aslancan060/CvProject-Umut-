using System.Data.SqlClient; // SQL hatalarını yakalamak için ekle
using CvProject.Data;
using CvProject.DTOs;
using CvProject.Interfaces;
using System.Net;
using System.Net.Mail;

namespace CvProject.Services
{
    public class ContactService : IContactService
    {
        private readonly DbHelper _dbHelper;
        private readonly IConfiguration _configuration;

        // Basit kelime filtresi (isteğe bağlı, SQL trigger’dan bağımsız)
        private readonly string[] _bannedWords = new[]
        {
            "sal...", "apt...", "mal", "ger...", "or...", "ya...", "pi...", "hakaret", "küf...", "peze...",
            "argox", "ka...k", "la...", "to..."
        };

        public ContactService(DbHelper dbHelper, IConfiguration configuration)
        {
            _dbHelper = dbHelper;
            _configuration = configuration;
        }

        public async Task<bool> SaveAndSendAsync(ContactDto contactDto)
        {
            // 1️⃣ Email formatı doğru mu?
            if (!IsValidEmail(contactDto.Email))
                throw new Exception("Invalid email format.");

            // 2️⃣ İsteğe bağlı uygulama içi kelime filtresi
            if (ContainsBannedWords(contactDto.Name) || ContainsBannedWords(contactDto.Message))
                throw new Exception("Message contains forbidden words.");

            try
            {
                // 3️⃣ DB insert — SQL trigger burada RAISERROR atabilir
                await _dbHelper.InsertContactAsync(contactDto.Name, contactDto.Email, contactDto.Message);
            }
            catch (SqlException ex)
            {
                // SQL trigger’daki RAISERROR mesajını yakala ve API’ye gönder
                throw new Exception(ex.Message);
            }

            // 4️⃣ Mail gönder
            var smtpSection = _configuration.GetSection("SmtpSettings");
            using var smtp = new SmtpClient(smtpSection["Host"], int.Parse(smtpSection["Port"] ?? "587"))
            {
                Credentials = new NetworkCredential(smtpSection["Username"], smtpSection["Password"]),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtpSection["From"] ?? smtpSection["Username"]),
                Subject = $"New Contact Message: {contactDto.Name}",
                Body = $"Sender: {contactDto.Name}\nEmail: {contactDto.Email}\n\nMessage:\n{contactDto.Message}"
            };
            mail.To.Add(smtpSection["To"]);

            await smtp.SendMailAsync(mail);

            return true;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool ContainsBannedWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.ToLowerInvariant();
            return _bannedWords.Any(bw => input.Contains(bw));
        }
    }
}
