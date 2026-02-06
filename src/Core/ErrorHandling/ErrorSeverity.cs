namespace LiteMonitor.src.Core.ErrorHandling
{
    /// <summary>
    /// 错误严重级别枚举
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// 信息级别 - 用于调试和跟踪
        /// </summary>
        Info,
        
        /// <summary>
        /// 警告级别 - 非致命问题，需要关注
        /// </summary>
        Warning,
        
        /// <summary>
        /// 错误级别 - 功能受影响但程序可继续运行
        /// </summary>
        Error,
        
        /// <summary>
        /// 严重级别 - 程序可能崩溃或无法继续运行
        /// </summary>
        Critical
    }
}