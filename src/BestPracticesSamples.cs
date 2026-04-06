using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApp.BestPractices
{
    // ── Empty Interface (marker interface anti-pattern) ──────────────────────

    // VIOLATION: empty interface (empty body)
    public interface IMarker { }

    // VIOLATION: another empty interface
    public interface IProcessable { }

    // ── Interface Naming Convention ──────────────────────────────────────────

    // VIOLATION: interface does not start with 'I'
    public interface UserRepository
    {
        void Save(object entity);
    }

    // VIOLATION: interface starts with lowercase 'i' not uppercase 'I'
    public interface iLogger
    {
        void Log(string message);
    }

    // ── Equals Without GetHashCode ───────────────────────────────────────────

    // VIOLATION: overrides Equals but not GetHashCode
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Point other)
            {
                return X == other.X && Y == other.Y;
            }
            return false;
        }
        // Missing: public override int GetHashCode() => HashCode.Combine(X, Y);
    }

    // ── Catch-and-Rethrow ────────────────────────────────────────────────────

    public class CatchRethrowExamples
    {
        // VIOLATION: throw ex resets stack trace
        public void ReadFileUnsafe(string path)
        {
            try
            {
                var content = File.ReadAllText(path);
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }

        // VIOLATION: another catch-and-rethrow
        public void ParseIntUnsafe(string value)
        {
            try
            {
                int.Parse(value);
            }
            catch (FormatException ex)
            {
                throw ex;
            }
        }
    }

    // ── Throw Generic Exception ──────────────────────────────────────────────

    public class ThrowGenericExamples
    {
        // VIOLATION: throw new Exception
        public void ValidateInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new Exception("Input cannot be null or empty");
            }
        }

        // VIOLATION: throw new ApplicationException
        public void ProcessOrder(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ApplicationException("Invalid order ID: " + orderId);
            }
        }

        // VIOLATION: throw new SystemException
        public void DoWork()
        {
            throw new SystemException("System error occurred");
        }
    }

    // ── Floating Point Equality ──────────────────────────────────────────────

    public class FloatingPointExamples
    {
        // VIOLATION: float comparison with ==
        public bool IsZeroPrice(double price)
        {
            if (price == 0.0)
            {
                return true;
            }
            return false;
        }

        // VIOLATION: float comparison with == using float literal
        public bool CheckTotal(float total)
        {
            return total == 1.5f;
        }
    }

    // ── Dictionary ContainsKey + Indexer ────────────────────────────────────

    public class DictionaryExamples
    {
        private Dictionary<string, int> _cache = new Dictionary<string, int>();

        // VIOLATION: ContainsKey followed by indexer
        public int GetValueUnsafe(string key)
        {
            if (_cache.ContainsKey(key))
            {
                var value = _cache[key];
                return value;
            }
            return 0;
        }

        // VIOLATION: another ContainsKey + indexer pattern
        public string GetNameUnsafe(Dictionary<string, string> dict, string id)
        {
            if (dict.ContainsKey(id))
            {
                return dict[id];
            }
            return null;
        }
    }

    // ── FirstOrDefault Without Null Check ───────────────────────────────────

    public class LinqExamples
    {
        private List<string> _items = new List<string> { "a", "b", "c" };
        private List<int> _numbers = new List<int> { 1, 2, 3 };

        // VIOLATION: FirstOrDefault without null check
        public int GetFirstMatchingLength(string prefix)
        {
            var item = _items.FirstOrDefault(x => x.StartsWith(prefix));
            return item.Length;
        }

        // VIOLATION: SingleOrDefault without null check
        public string FindExactMatch(string value)
        {
            var match = _items.SingleOrDefault(x => x == value);
            return match.ToUpper();
        }

        // VIOLATION: LastOrDefault without null check
        public string GetLastItem()
        {
            var last = _items.LastOrDefault();
            return last.Trim();
        }

        // VIOLATION: .Count() == 0 instead of !.Any()
        public bool IsEmpty(List<string> list)
        {
            return list.Count() == 0;
        }

        // VIOLATION: .Count() > 0 instead of .Any()
        public bool HasItems(List<string> list)
        {
            return list.Count() > 0;
        }

        // VIOLATION: .ToList() before LINQ
        public IEnumerable<string> FilterUnnecessarily(IEnumerable<string> source)
        {
            return source.ToList().Where(x => x.Length > 3);
        }

        // VIOLATION: .ToList() before Select
        public IEnumerable<int> MapUnnecessarily(IEnumerable<string> source)
        {
            return source.ToList().Select(x => x.Length);
        }
    }

    // ── String.IsNullOrEmpty vs manual check ─────────────────────────────────

    public class StringCheckExamples
    {
        // VIOLATION: manual null check
        public bool IsNameEmpty(string name)
        {
            if (name == null || name == "")
            {
                return true;
            }
            return false;
        }

        // VIOLATION: .Length == 0 check
        public bool HasEmptyValue(string value)
        {
            return value.Length == 0;
        }

        // VIOLATION: == "" check
        public void ProcessName(string name)
        {
            if (name == "")
            {
                Console.WriteLine("Name is empty");
            }
        }
    }

    // ── TODO/FIXME Comments ──────────────────────────────────────────────────

    public class WorkInProgress
    {
        // TODO: implement retry logic for transient failures
        public void ProcessPayment(decimal amount)
        {
            // FIXME: this crashes when amount is negative
            if (amount > 0)
            {
                // HACK: temporary workaround until billing service is ready
                Console.WriteLine("Processing: " + amount);
            }
        }

        // BUG: does not handle concurrent modifications
        public void UpdateCache(string key, string value)
        {
            // TODO: add proper locking
            Console.WriteLine("Cache update: " + key);
        }
    }

    // ── GC.Collect ───────────────────────────────────────────────────────────

    public class GcCollectExamples
    {
        // VIOLATION: explicit GC.Collect
        public void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    // ── Regex in Loop ────────────────────────────────────────────────────────

    public class RegexExamples
    {
        private List<string> _data = new List<string> { "abc123", "xyz789" };

        // VIOLATION: new Regex() inside foreach
        public List<string> FilterWithRegexInLoop(List<string> items)
        {
            var result = new List<string>();
            foreach (var item in items)
            {
                var regex = new Regex(@"\d+");
                if (regex.IsMatch(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        // VIOLATION: new Regex() inside for loop
        public int CountMatchesInLoop(string[] lines)
        {
            int count = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var pattern = new Regex(@"[A-Z][a-z]+");
                count += pattern.Matches(lines[i]).Count;
            }
            return count;
        }
    }

    // ── Thread.Sleep in Async ────────────────────────────────────────────────

    public class AsyncExamples
    {
        // VIOLATION: Thread.Sleep in async method
        public async Task ProcessWithDelayAsync(int milliseconds)
        {
            Thread.Sleep(milliseconds);
            await Task.CompletedTask;
        }

        // VIOLATION: Thread.Sleep in another async method
        public async Task<string> FetchWithRetryAsync(string url)
        {
            Thread.Sleep(1000);
            return await Task.FromResult(url);
        }
    }

    // ── Boxing Value Types ───────────────────────────────────────────────────

    public class BoxingExamples
    {
        // VIOLATION: non-generic ArrayList causes boxing
        public void UseArrayList()
        {
            var list = new ArrayList();
            list.Add(42);
            list.Add(3.14);
            list.Add(true);
        }

        // VIOLATION: non-generic Hashtable causes boxing
        public void UseHashtable()
        {
            var table = new Hashtable();
            table["count"] = 100;
            table["ratio"] = 0.75;
        }
    }

    // ── Static Mutable Collection ────────────────────────────────────────────

    public class CacheManager
    {
        // VIOLATION: public static mutable list
        public static List<string> ActiveSessions = new List<string>();

        // VIOLATION: public static mutable dictionary
        public static Dictionary<string, object> GlobalConfig = new Dictionary<string, object>();

        // VIOLATION: public static mutable HashSet
        public static HashSet<int> BlacklistedUserIds = new HashSet<int>();
    }

    // ── Lock on This or Public Object ───────────────────────────────────────

    public class LockingExamples
    {
        public static object SharedLock = new object();

        // VIOLATION: lock(this)
        public void UpdateUnsafe(string value)
        {
            lock (this)
            {
                Console.WriteLine("Updating: " + value);
            }
        }

        // VIOLATION: lock(SharedLock) - public object
        public void ProcessUnsafe(int id)
        {
            lock (SharedLock)
            {
                Console.WriteLine("Processing: " + id);
            }
        }
    }

    // ── String Format with Interpolation ────────────────────────────────────

    public class StringFormatExamples
    {
        // VIOLATION: String.Format with single placeholder
        public string FormatName(string name)
        {
            var result = String.Format("Hello, {0}!", name);
            return result;
        }

        // VIOLATION: string.Format with single placeholder
        public string FormatCount(int count)
        {
            var msg = string.Format("Total items: {0}", count);
            return msg;
        }
    }

    // ── Fire-and-Forget Async ────────────────────────────────────────────────

    public class FireAndForgetExamples
    {
        // VIOLATION: async method called without await
        public void SendNotification(string userId)
        {
            SendEmailAsync(userId);
        }

        // VIOLATION: async method called without await
        public void BackgroundCleanup()
        {
            CleanupOldRecordsAsync();
        }

        private async Task SendEmailAsync(string userId)
        {
            await Task.Delay(100);
        }

        private async Task CleanupOldRecordsAsync()
        {
            await Task.Delay(500);
        }
    }

    // ── Duplicate Literals ───────────────────────────────────────────────────

    public class HttpClientWrapper
    {
        private readonly HttpClient _client;

        public HttpClientWrapper()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }

        // VIOLATION: "application/json" appears 3+ times in this class
        public async Task<string> PostJsonAsync(string url, string body)
        {
            var content = new System.Net.Http.StringContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _client.PostAsync(url, content);
            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }
    }

    // ── Multiple Return Points ───────────────────────────────────────────────

    public class ClassificationService
    {
        // VIOLATION: 5 return statements (max 3)
        public string ClassifyScore(int score)
        {
            if (score < 0) return "invalid";
            if (score == 0) return "zero";
            if (score < 50) return "low";
            if (score < 80) return "medium";
            return "high";
        }

        // VIOLATION: 4 return points
        public string GetGrade(int score)
        {
            if (score >= 90) return "A";
            if (score >= 80) return "B";
            if (score >= 70) return "C";
            return "F";
        }
    }

    // ── Nesting Depth ────────────────────────────────────────────────────────

    public class NestingExamples
    {
        // VIOLATION: nesting depth exceeds 4
        public void DeepNesting(List<string> items, Dictionary<string, List<int>> lookup)
        {
            foreach (var item in items)
            {
                if (lookup.ContainsKey(item))
                {
                    var values = lookup[item];
                    if (values != null)
                    {
                        foreach (var v in values)
                        {
                            if (v > 0)
                            {
                                if (v < 100)
                                {
                                    Console.WriteLine("Value: " + v);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // ── Parameter Naming Convention ──────────────────────────────────────────

    public class ParameterNamingExamples
    {
        // VIOLATION: PascalCase parameters
        public void ProcessUser(string UserName, int UserId)
        {
            Console.WriteLine(UserName + " " + UserId);
        }

        // VIOLATION: PascalCase parameter
        public string FormatAddress(string StreetAddress, string CityName)
        {
            return StreetAddress + ", " + CityName;
        }
    }

    // ── Local Variable Naming Convention ────────────────────────────────────

    public class LocalVariableNamingExamples
    {
        public void ProcessOrder(int orderId)
        {
            // VIOLATION: PascalCase local variable
            string OrderName = "Test Order";
            int TotalCount = 0;
            double PricePerUnit = 9.99;
            Console.WriteLine(OrderName + TotalCount + PricePerUnit);
        }
    }

    // ── Nested Ternary ───────────────────────────────────────────────────────

    public class TernaryExamples
    {
        // VIOLATION: nested ternary (two ? on same line)
        public string ClassifyNumber(int x)
        {
            var label = x > 0 ? "positive" : x < 0 ? "negative" : "zero";
            return label;
        }

        // VIOLATION: another nested ternary
        public string GetPriority(int level, bool urgent)
        {
            return level > 5 ? "high" : urgent ? "medium" : "low";
        }
    }

    // ── Property Naming Convention ───────────────────────────────────────────

    public class PropertyNamingExamples
    {
        // VIOLATION: camelCase property
        public string firstName { get; set; }

        // VIOLATION: camelCase property
        public int totalCount { get; set; }

        // VIOLATION: underscore-prefixed property
        public bool _isActive { get; set; }
    }

    // ── Too Many Fields ──────────────────────────────────────────────────────

    public class GodClass
    {
        // 16+ fields — VIOLATION
        private string _field1;
        private string _field2;
        private string _field3;
        private int _field4;
        private int _field5;
        private int _field6;
        private bool _field7;
        private bool _field8;
        private double _field9;
        private double _field10;
        private object _field11;
        private object _field12;
        private List<string> _field13;
        private List<int> _field14;
        private Dictionary<string, object> _field15;
        private Dictionary<int, string> _field16;

        public GodClass() { }
    }

    // ── Unsafe Code ──────────────────────────────────────────────────────────

    public class UnsafeExamples
    {
        // VIOLATION: unsafe method
        public unsafe void CopyMemory(byte* src, byte* dst, int count)
        {
            for (int i = 0; i < count; i++)
            {
                dst[i] = src[i];
            }
        }
    }

    // ── LINQ Count vs Any (parenthesized assignment form) ───────────────────

    public class LinqCountParenExamples
    {
        // VIOLATION: .Count() == 0 in parenthesized expression
        public bool CheckEmpty(List<string> items)
        {
            bool isEmpty = (items.Count() == 0);
            return isEmpty;
        }

        // VIOLATION: .Count() > 0 in parenthesized expression
        public bool CheckAny(List<int> numbers)
        {
            bool hasItems = (numbers.Count() > 0);
            return hasItems;
        }

        // VIOLATION: .Count() != 0 in parenthesized expression
        public bool CheckNonEmpty(List<string> items)
        {
            bool notEmpty = (items.Count() != 0);
            return notEmpty;
        }
    }

    // ── String Format as Expression Statement ───────────────────────────────

    public class StringFormatExpressionStatements
    {
        // VIOLATION: String.Format as expression statement (not assigned)
        public void LogMessage(string name)
        {
            Console.WriteLine(string.Format("Hello, {0}!", name));
        }

        // VIOLATION: string.Format as expression statement
        public void LogCount(int count)
        {
            Console.WriteLine(string.Format("Total: {0}", count));
        }
    }

    // ── Finalizer Without Suppress Finalize ──────────────────────────────────

    public class ResourceHolder : IDisposable
    {
        private bool _disposed = false;

        // VIOLATION: finalizer present but SuppressFinalize call is absent from Dispose
        ~ResourceHolder()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // Missing suppress finalize call here
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}
