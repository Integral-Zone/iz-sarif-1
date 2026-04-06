using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc;

namespace SampleApp.Security
{
    // ── Insecure Deserialization ─────────────────────────────────────────────

    public class DeserializationExamples
    {
        // VIOLATION: BinaryFormatter is vulnerable to RCE
        public object DeserializeUnsafe(Stream stream)
        {
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }

        // VIOLATION: JavaScriptSerializer with type name handling
        public object DeserializeJson(string json)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.DeserializeObject(json);
        }
    }

    // ── XXE Injection ────────────────────────────────────────────────────────

    public class XxeExamples
    {
        // VIOLATION: XmlDocument without DTD protection
        public XmlDocument LoadXmlUnsafe(string xmlContent)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            return doc;
        }

        // VIOLATION: XmlTextReader without protection
        public void ReadXmlUnsafe(string path)
        {
            var reader = new XmlTextReader(path);
            while (reader.Read()) { }
        }

        // VIOLATION: XmlReaderSettings without DtdProcessing.Prohibit
        public void ReadWithSettingsUnsafe(string path)
        {
            var settings = new XmlReaderSettings();
            using var reader = XmlReader.Create(path, settings);
            while (reader.Read()) { }
        }
    }

    // ── SSL/TLS Verification Disabled ───────────────────────────────────────

    public class SslExamples
    {
        // VIOLATION: ServerCertificateValidationCallback always returns true (single line)
        public void DisableSslVerification()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
        }

        // VIOLATION: SslProtocols.Ssl3 usage
        public void UseWeakProtocol(SslStream ssl, string targetHost)
        {
            var protocols = System.Security.Authentication.SslProtocols.Ssl3;
            ssl.AuthenticateAsClient(targetHost, null, protocols, false);
        }
    }

    // ── Command Injection ────────────────────────────────────────────────────

    public class CommandInjectionExamples
    {
        // VIOLATION: Process.Start with shell interpreter
        public void RunShellUnsafe(string userInput)
        {
            System.Diagnostics.Process.Start("cmd.exe", "/c " + userInput);
        }

        // VIOLATION: ProcessStartInfo with dynamic arguments
        public void RunProcessUnsafe(string fileName, string arguments)
        {
            var psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = fileName;
            psi.Arguments = arguments;
            System.Diagnostics.Process.Start(psi);
        }
    }

    // ── LDAP Injection ───────────────────────────────────────────────────────

    public class LdapInjectionExamples
    {
        // VIOLATION: LDAP filter built with string concatenation
        public void SearchLdapUnsafe(string username)
        {
            var entry = new DirectoryEntry("LDAP://domain.example.com");
            var searcher = new DirectorySearcher(entry);
            searcher.Filter = "(&(objectClass=user)(sAMAccountName=" + username + "))";
            searcher.FindOne();
        }

        // VIOLATION: DirectoryEntry with dynamic path
        public void ConnectLdapUnsafe(string ldapPath)
        {
            var entry = new DirectoryEntry(ldapPath);
            entry.RefreshCache();
        }
    }

    // ── Path Traversal ───────────────────────────────────────────────────────

    public class PathTraversalExamples
    {
        // VIOLATION: File.ReadAllText with non-literal path
        public string ReadFileUnsafe(string userPath)
        {
            return File.ReadAllText(userPath);
        }

        // VIOLATION: File.WriteAllText with variable path
        public void WriteFileUnsafe(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        // VIOLATION: new FileStream with variable path
        public Stream OpenFileUnsafe(string filePath)
        {
            return new FileStream(filePath, FileMode.Open);
        }
    }

    // ── XSS Output ───────────────────────────────────────────────────────────

    public class XssOutputExamples
    {
        // VIOLATION: Response.Write with user-controlled input (capital R)
        public void WriteResponseUnsafe(HttpResponse Response, string userInput)
        {
            Response.Write(userInput);
        }

        // VIOLATION: Html.Raw with variable
        public string RenderUnsafe(string content)
        {
            // Html.Raw(content) produces unencoded output
            var output = Html.Raw(content);
            return output.ToString();
        }
    }

    // ── Open Redirect ────────────────────────────────────────────────────────

    public class OpenRedirectExamples : Controller
    {
        // VIOLATION: Redirect with variable URL
        public ActionResult Login(string returnUrl)
        {
            // VIOLATION: redirecting to unvalidated URL
            Response.Redirect(returnUrl);
            return null;
        }

        // VIOLATION: Redirect(returnUrl) without validation
        public IActionResult Callback(string returnUrl)
        {
            return Redirect(returnUrl);
        }
    }

    // ── Cookie Security ──────────────────────────────────────────────────────

    public class CookieSecurityExamples
    {
        // VIOLATION: HttpCookie without HttpOnly flag
        public void SetCookieNoHttpOnly(HttpResponse response, string sessionId)
        {
            var cookie = new HttpCookie("session", sessionId);
            response.Cookies.Add(cookie);
        }

        // VIOLATION: HttpCookie without Secure flag
        public void SetCookieNoSecure(HttpResponse response, string value)
        {
            var cookie = new HttpCookie("auth", value);
            cookie.HttpOnly = true;
            // Missing: cookie.Secure = true
            response.Cookies.Add(cookie);
        }
    }

    // ── CSRF Protection Missing ───────────────────────────────────────────────

    public class AccountController : Controller
    {
        // VIOLATION: [HttpPost] without [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ChangePassword(string newPassword)
        {
            return View();
        }

        // VIOLATION: [HttpDelete] without [ValidateAntiForgeryToken]
        [HttpDelete]
        public ActionResult DeleteAccount(int userId)
        {
            return View();
        }
    }

    // ── Missing Authorization ─────────────────────────────────────────────────

    public class AdminController : Controller
    {
        // VIOLATION: [HttpGet] without [Authorize] or [AllowAnonymous]
        [HttpGet]
        public ActionResult GetAdminDashboard()
        {
            return View();
        }

        // VIOLATION: [HttpPost] without [Authorize] or [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PromoteUser(int userId)
        {
            return View();
        }
    }

    // ── Timing Attack ────────────────────────────────────────────────────────

    public class AuthenticationService
    {
        // VIOLATION: comparing password with == (timing attack)
        public bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (inputPassword == storedHash)
            {
                return true;
            }
            return false;
        }

        // VIOLATION: comparing token with ==
        public bool ValidateToken(string token, string expectedToken)
        {
            return token == expectedToken;
        }
    }

    // ── Reflection with User Input ───────────────────────────────────────────

    public class ReflectionExamples
    {
        // VIOLATION: Type.GetType with user-controlled type name
        public Type GetTypeFromUserInput(string typeName)
        {
            Type.GetType(typeName);
            return null;
        }

        // VIOLATION: Activator.CreateInstance with variable
        public object CreateInstanceFromUserInput(string typeName)
        {
            Activator.CreateInstance(typeName, true);
            return null;
        }

        // VIOLATION: Assembly.Load with variable
        public void LoadAssemblyFromInput(string assemblyName)
        {
            System.Reflection.Assembly.Load(assemblyName);
        }
    }

    // ── ECB Cipher Mode ──────────────────────────────────────────────────────

    public class EncryptionExamples
    {
        // VIOLATION: CipherMode.ECB
        public byte[] EncryptWithEcb(byte[] data, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Key = key;
            using var encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        // VIOLATION: TripleDES weak symmetric encryption
        public byte[] EncryptWithTripleDes(byte[] data, byte[] key)
        {
            using var des = TripleDES.Create();
            des.Key = key;
            using var encryptor = des.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        // VIOLATION: DES weak symmetric encryption (as expression statement)
        public void CreateDesCipher()
        {
            DES.Create();
        }

        // VIOLATION: RC2 weak symmetric encryption (as expression statement)
        public void CreateRc2Cipher()
        {
            RC2.Create();
        }
    }

    // ── Hardcoded IP Address ─────────────────────────────────────────────────

    public class NetworkConfiguration
    {
        // VIOLATION: hardcoded IP address
        private string _databaseHost = "192.168.1.100";
        private string _cacheHost = "10.0.0.5";

        public void Connect()
        {
            string backupServer = "172.16.0.1";
            var client = new TcpClient();
            client.Connect(backupServer, 5432);
        }
    }

    // ── Sensitive Data in Log ────────────────────────────────────────────────

    public class LoggingExamples
    {
        private readonly Microsoft.Extensions.Logging.ILogger<LoggingExamples> _logger;

        public LoggingExamples(Microsoft.Extensions.Logging.ILogger<LoggingExamples> logger)
        {
            _logger = logger;
        }

        // VIOLATION: logging password
        public void LogSensitiveData(string password, string token)
        {
            _logger.LogInformation("User auth with password: " + password);
            _logger.LogDebug("API token used: " + token);
        }

        // VIOLATION: logging apikey
        public void LogApiKey(string apikey)
        {
            _logger.LogWarning("Request made with apikey: " + apikey);
        }

        // VIOLATION: logging secret
        public void LogSecret(string secret)
        {
            _logger.LogError("Credential secret: " + secret);
        }
    }
}
