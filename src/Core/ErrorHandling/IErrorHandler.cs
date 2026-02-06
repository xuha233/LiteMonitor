using System;

namespace LiteMonitor.src.Core.ErrorHandling
{
    /// <summary>
    /// 统一错误处理接口
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// 处理异常
        /// </summary>
        /// <param name="exception">异常对象</param>
        /// <param name="context">错误上下文（可选）</param>
        /// <param name="severity">错误严重级别</param>
        /// <param name="rethrow">是否重新抛出异常</param>
        void HandleException(Exception exception, string? context = null, 
                           ErrorSeverity severity = ErrorSeverity.Error, bool rethrow = false);
        
        /// <summary>
        /// 处理错误消息（无异常对象）
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="context">错误上下文（可选）</param>
        /// <param name="severity">错误严重级别</param>
        void HandleError(string message, string? context = null, 
                        ErrorSeverity severity = ErrorSeverity.Error);
        
        /// <summary>
        /// 尝试执行操作，自动处理异常
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <param name="context">错误上下文（可选）</param>
        /// <param name="onError">错误处理回调（可选）</param>
        /// <returns>操作是否成功</returns>
        bool TryExecute(Action action, string? context = null, Action<Exception>? onError = null);
        
        /// <summary>
        /// 尝试执行操作并返回结果，自动处理异常
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <param name="context">错误上下文（可选）</param>
        /// <param name="defaultValue">失败时返回的默认值</param>
        /// <param name="onError">错误处理回调（可选）</param>
        /// <returns>函数结果或默认值</returns>
        T? TryExecute<T>(Func<T> func, string? context = null, T? defaultValue = default, 
                        Action<Exception>? onError = null);
        
        /// <summary>
        /// 显示用户友好的错误提示
        /// </summary>
        /// <param name="title">提示标题</param>
        /// <param name="message">提示消息</param>
        /// <param name="severity">错误严重级别</param>
        void ShowUserNotification(string title, string message, ErrorSeverity severity);
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象（可选）</param>
        /// <param name="severity">错误严重级别</param>
        void LogError(string message, Exception? exception = null, ErrorSeverity severity = ErrorSeverity.Error);
    }
}