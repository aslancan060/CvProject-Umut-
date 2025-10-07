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
           
            if (!IsValidEmail(contactDto.Email))
                return false; // Geçersiz email — DB’ye yazma, mail gönderme

            
            if (ContainsBannedWords(contactDto.Name) || ContainsBannedWords(contactDto.Message))
                return false; // Uygunsuz içerik varsa DB’ye yazma, mail gönderme

        
            await _dbHelper.InsertContactAsync(contactDto.Name, contactDto.Email, contactDto.Message);

            var smtpSection = _configuration.GetSection("SmtpSettings");
            using var smtp = new SmtpClient(smtpSection["Host"], int.Parse(smtpSection["Port"] ?? "587"))
            {
                Credentials = new NetworkCredential(smtpSection["Username"], smtpSection["Password"]),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtpSection["From"] ?? smtpSection["Username"]),
                Subject = $"Yeni İletişim Mesajı: {contactDto.Name}",
                Body = $"Gönderen: {contactDto.Name}\nEmail: {contactDto.Email}\n\nMesaj:\n{contactDto.Message}"
            };
            mail.To.Add(smtpSection["To"]);

            await smtp.SendMailAsync(mail);

            return true;
        }

    
        /// <summary>
        /// Email formatı doğru mu kontrolü
        /// </summary>
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

        /// <summary>
        /// Yasaklı kelimeleri içeriyor mu kontrolü
        /// </summary>
        private bool ContainsBannedWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.ToLowerInvariant();
            return _bannedWords.Any(bw => input.Contains(bw));
        }
    }
}
