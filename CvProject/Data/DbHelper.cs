using Microsoft.Data.SqlClient;

namespace CvProject.Data
{
    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> InsertContactAsync(string name, string email, string message)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "INSERT INTO Contacts (Name, Email, Message, CreatedAt) " +
                "VALUES (@Name, @Email, @Message, @CreatedAt); SELECT SCOPE_IDENTITY();", con);

            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Message", message);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            await con.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }
    }
}
