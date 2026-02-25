using EventOrganizer_ASP.NET.Models;
using Microsoft.Data.SqlClient;

namespace EventOrganizer_ASP.NET.DAL
{
    public class UserDAL
    {
        private readonly DbHelper _dbHelper;

        public UserDAL(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public void InsertUser(User user)
        {
            using (SqlConnection con = _dbHelper.GetConnection())
            {
                string query = @"INSERT INTO Users 
                                (FullName, Email, Password, Phone, Role, CreatedAt)
                                VALUES (@FullName, @Email, @Password, @Phone, @Role, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", user.Password);
                cmd.Parameters.AddWithValue("@Phone", user.Phone ?? "");
                cmd.Parameters.AddWithValue("@Role", user.Role);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public User LoginUser(string email, string password)
        {
            using (SqlConnection con = _dbHelper.GetConnection())
            {
                string query = "SELECT * FROM Users WHERE Email=@Email AND Password=@Password";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new User
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        FullName = reader["FullName"].ToString(),
                        Role = reader["Role"].ToString()
                    };
                }
            }

            return null;
        }
    }
}
