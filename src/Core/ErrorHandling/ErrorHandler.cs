using System;
using System.IO;
using System.Windows.Forms;

namespace LiteMonitor.src.Core.ErrorHandling
{
    /// <summary>
    /// 统一错误处理实现类
    /// </summary>
    public class ErrorHandler : IErrorHandler
    {
        private static ErrorHandler? _instance;
        private static readonly object _lock = new object();
        
        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static ErrorHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ErrorHandler();
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 错误日志文件路径
        /// </summary>
        public string ErrorLogPath { get; set; }
        
        /// <summary>
        /// 是否启用用户通知
        /// </summary>
        public bool EnableUserNotifications { get; set; } = true;
        
        /// <summary>
        /// 是否启用文件日志
        /// </summary>
        public bool EnableFileLogging { get; set; } = true;
        
        /// <summary>
        /// 私有构造函数
        /// </summary>
        private ErrorHandler()
        {
            ErrorLogPath = Path.Combine(AppContext.BaseDirectory, "LiteMonitor_Error.log");
        }
        
        /// <summary>
        /// 处理异常
        /// </summary>
        public void HandleException(Exception exception, string? context = null, 
                                   ErrorSeverity severity = ErrorSeverity.Error, bool rethrow = false)
        {
            if (exception == null) return;
            
            string errorContext = context ?? "Unknown";
            string errorMessage = $"[{errorContext}] {exception.Message}";
            
            // 记录错误日志
            LogError(errorMessage, exception, severity);
            
            // 根据严重级别决定是否显示用户通知
            if (severity >= ErrorSeverity.Error && EnableUserNotifications)
            {
                ShowUserNotification("程序错误", 
                    $"程序遇到错误：{exception.Message}\n\n详细信息已记录到日志文件。", 
                    severity);
            }
            
            // 根据配置决定是否重新抛出
            if (rethrow)
            {
                throw exception;
            }
        }
        
        /// <summary>
        /// 处理错误消息（无异常对象）
        /// </summary>
        public void HandleError(string message, string? context = null, 
                               ErrorSeverity severity = ErrorSeverity.Error)
        {
            string errorContext = context ?? "Unknown";
            string errorMessage = $"[{errorContext}] {message}";
            
            // 记录错误日志
            LogError(errorMessage, null, severity);
            
            // 根据严重级别决定是否显示用户通知
            if (severity >= ErrorSeverity.Error && EnableUserNotifications)
            {
                ShowUserNotification("程序错误", 
                    $"程序遇到错误：{message}\n\n详细信息已记录到日志文件。", 
                    severity);
            }
        }
        
        /// <summary>
        /// 尝试执行操作，自动处理异常
        /// </summary>
        public bool TryExecute(Action action, string? context = null, Action<Exception>? onError = null)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                // 调用自定义错误处理回调
                onError?.Invoke(ex);
                
                // 记录错误但不重新抛出
                HandleException(ex, context, ErrorSeverity.Error, false);
                return false;
            }
        }
        
        /// <summary>
        /// 尝试执行操作并返回结果，自动处理异常
        /// </summary>
        public T? TryExecute<T>(Func<T> func, string? context = null, T? defaultValue = default, 
                               Action<Exception>? onError = null)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                // 调用自定义错误处理回调
                onError?.Invoke(ex);
                
                // 记录错误但不重新抛出
                HandleException(ex, context, ErrorSeverity.Error, false);
                return defaultValue;
            }
        }
        
        /// <summary>
        /// 显示用户友好的错误提示
        /// </summary>
        public void ShowUserNotification(string title, string message, ErrorSeverity severity)
        {
            // 只在UI线程中显示消息框
            if (Application.MessageLoop)
            {
                MessageBoxIcon icon = severity switch
                {
                    ErrorSeverity.Info => MessageBoxIcon.Information,
                    ErrorSeverity.Warning => MessageBoxIcon.Warning,
                    ErrorSeverity.Error => MessageBoxIcon.Error,
                    ErrorSeverity.Critical => MessageBoxIcon.Error,
                    _ => MessageBoxIcon.Information
                };
                
                MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
            }
            else
            {
                // 非UI线程，记录日志但不显示消息框
                LogError($"无法在非UI线程显示用户通知: {title} - {message}", null, severity);
            }
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        public void LogError(string message, Exception? exception = null, ErrorSeverity severity = ErrorSeverity.Error)
        {
            string logLevel = severity switch
            {
                ErrorSeverity.Info => "INFO",
                ErrorSeverity.Warning => "WARN",
                ErrorSeverity.Error => "ERROR",
                ErrorSeverity.Critical => "CRITICAL",
                _ => "INFO"
            };
            
            // 输出到控制台
            switch (severity)
            {
                case ErrorSeverity.Info:
                    Console.WriteLine($"[INFO] {message}");
                    break;
                case ErrorSeverity.Warning:
                    Console.WriteLine($"[WARN] {message}");
                    break;
                case ErrorSeverity.Error:
                case ErrorSeverity.Critical:
                    Console.WriteLine($"[ERROR] {message}");
                    break;
            }
            
            // 同时写入错误日志文件（如果启用）
            if (EnableFileLogging)
            {
                WriteToErrorLog(logLevel, message, exception);
            }
        }
        
        /// <summary>
        /// 写入错误日志文件
        /// </summary>
        private void WriteToErrorLog(string level, string message, Exception? exception)
        {
            try
            {
                string logEntry = "==================================================\n" +
                                 $"[Time]: {DateTime.Now}\n" +
                                 $"[Level]: {level}\n" +
                                 $"[Message]: {message}\n";
                
                if (exception != null)
                {
                    logEntry += $"[Exception]: {exception.GetType().Name}\n" +
                               $"[Stack]:\n{exception.StackTrace}\n";
                }
                
                logEntry += "==================================================\n\n";
                
                File.AppendAllText(ErrorLogPath, logEntry);
            }
            catch
            {
                // 如果日志都写不进去，通常是磁盘满了或权限极度受限，只能忽略
                // 但至少输出到控制台
                Console.WriteLine($"[ERROR] 无法写入错误日志文件: {message}");
            }
        }
        
        /// <summary>
        /// <summary>
        /// 便捷方法：快速日志记录
        /// </summary>
        public static class Log
        {
            public static void Debug(string message) => Console.WriteLine($"[DEBUG] {message}");
            public static void Info(string message) => Console.WriteLine($"[INFO] {message}");
            public static void Warn(string message) => Console.WriteLine($"[WARN] {message}");
            public static void Error(string message) => Console.WriteLine($"[ERROR] {message}");
            
            public static void Error(string message, Exception ex)
            {
                Console.WriteLine($"[ERROR] {message}: {ex.Message}");
                Instance.LogError(message, ex, ErrorSeverity.Error);
            }
        }
    }
}