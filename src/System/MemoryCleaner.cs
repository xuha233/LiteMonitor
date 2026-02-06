using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LiteMonitor.src.SystemServices
{
    /// <summary>
    /// 内存清理结果
    /// </summary>
    public class MemoryCleanResult
    {
        /// <summary>
        /// 清理前的内存使用 (MB)
        /// </summary>
        public long MemoryBeforeMB { get; set; }
        
        /// <summary>
        /// 清理后的内存使用 (MB)
        /// </summary>
        public long MemoryAfterMB { get; set; }
        
        /// <summary>
        /// 释放的内存 (MB)
        /// </summary>
        public long FreedMemoryMB => MemoryBeforeMB - MemoryAfterMB;
        
        /// <summary>
        /// 清理的进程数量
        /// </summary>
        public int ProcessesCleaned { get; set; }
        
        /// <summary>
        /// 失败的进程数量
        /// </summary>
        public int ProcessesFailed { get; set; }
        
        /// <summary>
        /// 清理耗时 (毫秒)
        /// </summary>
        public long ElapsedMilliseconds { get; set; }
        
        /// <summary>
        /// 是否成功完成
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 优化的内存清理器
    /// </summary>
    public static class MemoryCleaner
    {
        // Windows API 声明
        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);

        /// <summary>
        /// 执行优化的内存清理
        /// </summary>
        /// <param name="onProgress">进度回调 (0-100)</param>
        /// <param name="options">清理选项</param>
        /// <returns>清理结果</returns>
        public static MemoryCleanResult Clean(
            Action<int>? onProgress = null, 
            MemoryCleanOptions? options = null)
        {
            options ??= new MemoryCleanOptions();
            var result = new MemoryCleanResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 1. 获取清理前的内存状态
                result.MemoryBeforeMB = GetCurrentMemoryUsageMB();
                onProgress?.Invoke(2);

                // 2. 清理本进程内存
                if (options.CleanSelf)
                {
                    CleanSelfProcess();
                    onProgress?.Invoke(5);
                }

                // 3. 执行GC清理
                if (options.PerformGC)
                {
                    PerformGarbageCollection();
                    onProgress?.Invoke(10);
                }

                // 4. 清理其他进程
                if (options.CleanOtherProcesses)
                {
                    var cleanResult = CleanOtherProcesses(
                        p => onProgress?.Invoke(10 + (int)(p * 0.85)),
                        options);
                    result.ProcessesCleaned = cleanResult.CleanedCount;
                    result.ProcessesFailed = cleanResult.FailedCount;
                }
                else
                {
                    onProgress?.Invoke(95);
                }

                // 5. 清理系统缓存
                if (options.CleanSystemCache)
                {
                    CleanSystemCache();
                }

                // 6. 获取清理后的内存状态
                result.MemoryAfterMB = GetCurrentMemoryUsageMB();
                onProgress?.Invoke(100);

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            }

            return result;
        }

        /// <summary>
        /// 异步执行内存清理
        /// </summary>
        public static async Task<MemoryCleanResult> CleanAsync(
            Action<int>? onProgress = null, 
            MemoryCleanOptions? options = null)
        {
            return await Task.Run(() => Clean(onProgress, options));
        }

        /// <summary>
        /// 获取当前内存使用情况 (MB)
        /// </summary>
        private static long GetCurrentMemoryUsageMB()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                return proc.WorkingSet64 / (1024 * 1024);
            }
        }

        /// <summary>
        /// 清理本进程内存
        /// </summary>
        private static void CleanSelfProcess()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                try
                {
                    // 清空工作集
                    EmptyWorkingSet(proc.Handle);
                    
                    // 设置工作集大小 (最小化内存占用)
                    SetProcessWorkingSetSize(proc.Handle, -1, -1);
                }
                catch
                {
                    // 忽略可能的权限错误
                }
            }
        }

        /// <summary>
        /// 执行垃圾回收
        /// </summary>
        private static void PerformGarbageCollection()
        {
            // 强制完整GC，等待所有代完成
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
        }

        /// <summary>
        /// 清理其他进程
        /// </summary>
        private static (int CleanedCount, int FailedCount) CleanOtherProcesses(
            Action<double>? onProgress, 
            MemoryCleanOptions options)
        {
            int cleanedCount = 0;
            int failedCount = 0;

            try
            {
                var processes = Process.GetProcesses()
                    .Where(p => p.Id != Process.GetCurrentProcess().Id) // 排除本进程
                    .Where(p => !options.ExcludedProcesses.Contains(p.ProcessName.ToLower()))
                    .ToList();

                int total = processes.Count;
                if (total == 0) return (0, 0);

                // 使用并行处理提高性能
                var optionsParallel = new ParallelOptions
                {
                    MaxDegreeOfParallelism = options.MaxParallelism
                };

                int processedCount = 0;
                var lockObj = new object();

                Parallel.ForEach(processes, optionsParallel, proc =>
                {
                    bool success = false;
                    try
                    {
                        using (proc)
                        {
                            if (!proc.HasExited)
                            {
                                // 尝试清空工作集
                                if (EmptyWorkingSet(proc.Handle) == 1)
                                {
                                    success = true;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // 忽略无权限访问的进程
                    }

                    lock (lockObj)
                    {
                        if (success) cleanedCount++;
                        else failedCount++;
                        
                        processedCount++;
                        double progress = (double)processedCount / total;
                        onProgress?.Invoke(progress);
                    }
                });
            }
            catch
            {
                // 忽略异常
            }

            return (cleanedCount, failedCount);
        }

        /// <summary>
        /// 清理系统缓存 (通过触发内存压力)
        /// </summary>
        private static void CleanSystemCache()
        {
            try
            {
                // 通过分配和释放大块内存来触发系统缓存清理
                // 这是一个技巧性的方法，实际效果取决于系统配置
                for (int i = 0; i < 3; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            catch
            {
                // 忽略异常
            }
        }
    }

    /// <summary>
    /// 内存清理选项
    /// </summary>
    public class MemoryCleanOptions
    {
        /// <summary>
        /// 是否清理本进程内存
        /// </summary>
        public bool CleanSelf { get; set; } = true;

        /// <summary>
        /// 是否执行垃圾回收
        /// </summary>
        public bool PerformGC { get; set; } = true;

        /// <summary>
        /// 是否清理其他进程
        /// </summary>
        public bool CleanOtherProcesses { get; set; } = true;

        /// <summary>
        /// 是否清理系统缓存
        /// </summary>
        public bool CleanSystemCache { get; set; } = true;

        /// <summary>
        /// 最大并行度
        /// </summary>
        public int MaxParallelism { get; set; } = 4;

        /// <summary>
        /// 排除的进程名称列表 (小写)
        /// </summary>
        public HashSet<string> ExcludedProcesses { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "system",
            "registry",
            "smss",
            "csrss",
            "services",
            "lsass",
            "winlogon",
            "svchost"
        };
    }
}