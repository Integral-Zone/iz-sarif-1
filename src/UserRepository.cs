using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using Microsoft.Extensions.Logging;

namespace SampleApp.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(string connectionString, ILogger<UserRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        // ─── NON-COMPLIANT: string concatenation ─────────────────────────────

        public DataTable GetUserByNameUnsafe(string username)
        {
            using var conn = new SqlConnection(_connectionString);
            // BAD: directly concatenating user input into SQL
            string query = "SELECT * FROM Users WHERE Username = '" + username + "'";
            var cmd = new SqlCommand(query, conn);
            var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public void DeleteUserUnsafe(string userId)
        {
            using var conn = new SqlConnection(_connectionString);
            // BAD: CommandText built with concatenation
            var cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM Users WHERE Id = " + userId;
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public DataTable SearchUnsafeInterpolated(string role, string department)
        {
            using var conn = new SqlConnection(_connectionString);
            // BAD: string interpolation with SQL keywords
            string sql = $"SELECT Id, Name FROM Users WHERE Role = '{role}' AND Department = '{department}'";
            var cmd = new SqlCommand(sql, conn);
            var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable FindByEmailUnsafeFormat(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            // BAD: String.Format embedding user input into SQL
            string sql = String.Format("SELECT * FROM Users WHERE Email = '{0}'", email);
            var cmd = new SqlCommand(sql, conn);
            var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable SearchWithBuilderUnsafe(string firstName, string lastName)
        {
            using var conn = new SqlConnection(_connectionString);
            // BAD: StringBuilder.Append building a SQL query
            var sb = new StringBuilder();
            sb.Append("SELECT * FROM Users WHERE FirstName = '");
            sb.Append(firstName);
            sb.Append("' AND LastName = '");
            sb.Append(lastName);
            sb.Append("'");
            var cmd = new SqlCommand(sb.ToString(), conn);
            var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        // ─── COMPLIANT: parameterised queries ────────────────────────────────

        public DataTable GetUserByNameSafe(string username)
        {
            using var conn = new SqlConnection(_connectionString);
            // GOOD: parameterised query — user input never touches the SQL string
            const string query = "SELECT * FROM Users WHERE Username = @Username";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Username", username);
            var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public void DeleteUserSafe(string userId)
        {
            using var conn = new SqlConnection(_connectionString);
            // GOOD: parameterised DELETE
            const string sql = "DELETE FROM Users WHERE Id = @UserId";
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public DataTable SearchSafe(string role, string department)
        {
            using var conn = new SqlConnection(_connectionString);
            // GOOD: all dynamic values go through parameters
            const string sql = "SELECT Id, Name FROM Users WHERE Role = @Role AND Department = @Department";
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@Department", department);
            var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        // ─── NON-COMPLIANT: empty catch blocks ───────────────────────────────

        /// <summary>
        /// BAD: IOException is caught and completely ignored.
        /// If the file cannot be read, the method silently returns null
        /// and callers have no idea anything went wrong.
        /// </summary>
        public string ReadConfigFileUnsafe(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (IOException)
            {
                // VIOLATION: empty catch — IOException is swallowed silently
            }
            return null;
        }

        /// <summary>
        /// BAD: SqlException is swallowed. A failed DB operation (deadlock,
        /// constraint violation, connection timeout) disappears without a trace.
        /// </summary>
        public void SaveAuditLogUnsafe(string message)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                const string sql = "INSERT INTO AuditLog (Message) VALUES (@msg)";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@msg", message);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                // VIOLATION: empty catch — DB errors are silently discarded
            }
        }

        /// <summary>
        /// BAD: Bare general catch with no type filter and an empty body.
        /// This is the most dangerous form — it catches every possible exception
        /// (including OutOfMemoryException, StackOverflowException) and does nothing.
        /// </summary>
        public bool PingDatabaseUnsafe()
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                return true;
            }
            catch
            {
                // VIOLATION: bare catch { } — all exceptions silently discarded
            }
            return false;
        }

        /// <summary>
        /// BAD: Multiple typed catch clauses all with empty bodies.
        /// Each exception type is caught but none is logged or handled.
        /// </summary>
        public void UpdateUserProfileUnsafe(int userId, string bio)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                const string sql = "UPDATE Users SET Bio = @bio WHERE Id = @id";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@bio", bio);
                cmd.Parameters.AddWithValue("@id", userId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                // VIOLATION: empty catch for SqlException
            }
            catch (InvalidOperationException)
            {
                // VIOLATION: empty catch for InvalidOperationException
            }
        }

        // ─── COMPLIANT: proper exception handling ────────────────────────────

        /// <summary>
        /// GOOD: Exception is caught, logged, and the method signals failure
        /// by returning null — caller can handle the null case.
        /// </summary>
        public string ReadConfigFileSafe(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Failed to read config file at {Path}", path);
                return null;
            }
        }

        /// <summary>
        /// GOOD: Exception is caught, wrapped in a domain exception, and rethrown
        /// so the caller receives a meaningful error with full stack context.
        /// </summary>
        public void SaveAuditLogSafe(string message)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                const string sql = "INSERT INTO AuditLog (Message) VALUES (@msg)";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@msg", message);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Failed to persist audit log entry: {Message}", message);
                throw new InvalidOperationException("Audit log write failed. See inner exception.", ex);
            }
        }
    }
}
