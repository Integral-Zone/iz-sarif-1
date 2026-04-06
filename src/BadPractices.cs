using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SampleApp.BadPractices
{
    // Naming violation: class should be PascalCase (no underscores)
    public class user_service
    {
        // Public field — breaks encapsulation
        public string AdminEmail;
        public int retryCount;

        // Hardcoded credentials — should be in Key Vault / env vars
        private string password = "SuperSecret123!";
        private string apiKey = "sk-live-abcdef1234567890";
        private string connectionString = "Server=prod-db;Password=hunter2";

        // Public const — fine; naming violation: should be PascalCase not camelCase
        public const string default_role = "admin";
        public const int MAX_RETRIES = 5;
        public const string BASE_URL = "https://api.example.com";

        // Constructor: too many parameters
        public user_service(
            string host,
            int port,
            string username,
            string password2,
            string schema,
            string appName)
        {
            AdminEmail = "admin@example.com";
        }

        // Method naming violation: should be PascalCase
        public void processUser(int id)
        {
            Console.WriteLine("Processing user: " + id);
        }

        // Method naming violation + Console.WriteLine
        public string buildReport(string title, string section, string body, string footer, string author, string version)
        {
            Console.WriteLine("Building report: " + title);
            return title + body;
        }

        // Async void — should return Task
        public async void sendNotification(string userId)
        {
            Console.WriteLine("Sending notification to " + userId);
            await System.Threading.Tasks.Task.Delay(100);
        }

        // Catching generic Exception
        public void loadConfig()
        {
            try
            {
                // do work
                int x = int.Parse("abc");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // empty finally — violation
            }
        }

        // String concat in loop
        public string buildCsv(List<string> items)
        {
            string result = "";
            foreach (var item in items)
            {
                result += item + ",";
            }

            string output = "";
            for (int i = 0; i < items.Count; i++)
            {
                output += items[i].ToString();
            }

            string s = "";
            while (s.Length < 100)
            {
                s += "x";
            }

            return result + output + s;
        }

        // Cyclomatic complexity — many branches
        public string evaluateScore(int score, string grade, bool bonus, bool penalty, string region)
        {
            string result = "";
            if (score > 90)
            {
                result = "A";
            }
            else if (score > 80)
            {
                result = "B";
            }
            else if (score > 70)
            {
                result = "C";
            }
            else if (score > 60)
            {
                result = "D";
            }
            else
            {
                result = "F";
            }

            if (bonus && score > 50)
            {
                result += "+";
            }

            if (penalty || score < 30)
            {
                result += "-";
            }

            switch (grade)
            {
                case "honors":
                    result += "*";
                    break;
                case "merit":
                    result += "^";
                    break;
                case "pass":
                    break;
                default:
                    result += "?";
                    break;
            }

            if (region == "US" || region == "CA")
            {
                result += "(NA)";
            }
            else if (region == "UK" && score > 40)
            {
                result += "(EU)";
            }

            for (int i = 0; i < 3; i++)
            {
                if (i == 1)
                {
                    result += ".";
                }
            }

            return result;
        }

        // Weak hashing algorithms
        public string HashPasswordMd5(string input)
        {
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }

        public string HashPasswordSha1(string input)
        {
            using var sha1 = SHA1.Create();
            var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }

        // Insecure random
        public string GenerateToken()
        {
            var rng = new Random();
            return rng.Next(100000, 999999).ToString();
        }

        public int RollDice()
        {
            var random = new Random(42);
            return random.Next(1, 7);
        }
    }
}
