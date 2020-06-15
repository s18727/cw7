using System.Data.SqlClient;
using Classes7.Models;
using Microsoft.AspNetCore.Mvc;

namespace Classes7.Services
{
    public class SqlServerDbService : IStudentDbService
    {
        private const string connecionString = "Data Source=db-mssql.pjwstk.edu.pl;" +
                                               "Initial Catalog=s18727;" +
                                               "Integrated Security=True";

        public Student GetStudent(string index)
        {
            using (var conn = new SqlConnection(connecionString))
            using (var comm = new SqlCommand())
            {
                conn.Open();
                comm.Connection = conn;

                comm.CommandText = "SELECT IndexNumber, FirstName, LastName, Password, Salt " +
                                   "FROM Student WHERE IndexNumber = @index";
                comm.Parameters.AddWithValue("index", index);

                var reader = comm.ExecuteReader();

                return ReadStudent(reader);
            }
        }

        public void SaveRefreshToken(string indexNumber, string refreshToken)
        {
            using (var conn = new SqlConnection(connecionString))
            using (var comm = new SqlCommand())
            {
                conn.Open();
                comm.Connection = conn;

                comm.CommandText = "UPDATE Student SET RefreshToken = @refreshToken " +
                                   "WHERE IndexNumber = @indexNumber";
                comm.Parameters.AddWithValue("refreshToken", refreshToken);
                comm.Parameters.AddWithValue("indexNumber", indexNumber);
                
                comm.ExecuteNonQuery();
            }
        }

        public Student GetStudentByRefreshToken(string refreshToken)
        {
            using (var conn = new SqlConnection(connecionString))
            using (var comm = new SqlCommand())
            {
                conn.Open();
                comm.Connection = conn;
                
                comm.CommandText = "SELECT IndexNumber, FirstName, LastName, Password, Salt " +
                                   "FROM Student WHERE RefreshToken = @refreshToken";
                comm.Parameters.AddWithValue("refreshToken", refreshToken);

                var reader = comm.ExecuteReader();

                return ReadStudent(reader);
            }
        }

        public Student ReadStudent(SqlDataReader reader)
        {
            if (!reader.Read())
            {
                return null;
            }
            
            Student student = new Student();

            student.IndexNumber = reader["IndexNumber"].ToString();
            student.FirstName = reader["FirstName"].ToString();
            student.LastName = reader["LastName"].ToString();
            student.Password = reader["Password"].ToString();
            student.Salt = reader["Salt"].ToString();
            
            return student;
        }
    }
}