using Barotrauma;
using System;
using System.Diagnostics;
using System.IO;

namespace MultiplayerCrewManager
{
    public static class McmUtils
    {
        public static void Raw(object? msg)
        {
            try
            {
                ModUtils.Logging.PrintMessage($"[RAW] [MCM] - {msg}");
            }
            catch (System.Exception e)
            {
                ModUtils.Logging.PrintError(e.Message);
            }
        }

        public static void Raw(Exception ex, object? msg)
        {
            try
            {
                ModUtils.Logging.PrintMessage($"[RAW ERROR] [MCM] - {msg}, Exception {ex.Message}");
                StackTrace stackTrace = new StackTrace(0, true);
                var maxTrace = Math.Min(3, stackTrace.FrameCount);

                for (int i = 0; i < maxTrace; i++)
                {
                    var frame = stackTrace.GetFrame(i);
                    if (frame == null) continue;

                    // Get the file name and line number
                    string fileName = frame.GetFileName();
                    int lineNumber = frame.GetFileLineNumber();

                    // Get the method that called the current method
                    var callingMethod = frame.GetMethod().Name;
                    var methodClass = frame.GetMethod().DeclaringType.Name;
                    ModUtils.Logging.PrintError($" --> \"{fileName}\" | \"{methodClass}.{callingMethod}\", Line {lineNumber}");
                }
            }
            catch (System.Exception e)
            {
                ModUtils.Logging.PrintError(e.Message);
            }
        }

        public static void Trace(object? msg)
        {
            try
            {
                if (McmMod.GetConfig.LoggingLevel >= McmLoggingLevel.Trace) //LoggingLevel=4
                    ModUtils.Logging.PrintMessage($"[TRACE] [MCM] - {msg}");
            }
            catch (System.Exception e)
            {
                ModUtils.Logging.PrintError(e.Message);
            }
        }

        public static void Info(object? msg)
        {
            try
            {
                if (McmMod.GetConfig.LoggingLevel >= McmLoggingLevel.Info) //LoggingLevel=3
                    ModUtils.Logging.PrintMessage($"[INFO] [MCM] - {msg}");
            }
            catch (System.Exception e)
            {
                ModUtils.Logging.PrintError(e.Message);
            }
        }

        public static void Warn(object? msg)
        {
            try
            {
                if (McmMod.GetConfig.LoggingLevel >= McmLoggingLevel.Warning) //LoggingLevel=2
                    ModUtils.Logging.PrintWarning($"[WARN] [MCM] - {msg}");
            }
            catch (System.Exception e)
            {
                ModUtils.Logging.PrintError(e.Message);
            }
        }

        public static void Error(object? msg)
        {
            try
            {
                if (McmMod.GetConfig.LoggingLevel >= McmLoggingLevel.Error) //LoggingLevel=1
                {
                    ModUtils.Logging.PrintError($"[ERROR] [MCM] - {msg}");
                }
            }
            catch (System.Exception e)
            {
                ModUtils.Logging.PrintError(e.Message);
            }
        }

        public static void Error(System.Exception e, object? msg)
        {
            try
            {
                // Create a new stack trace, skipping the first frame to get the caller method

                if (McmMod.GetConfig.LoggingLevel >= McmLoggingLevel.Error) //LoggingLevel=1
                {
                    ModUtils.Logging.PrintError($"[ERROR] [MCM] - {msg}, Exception {e.Message}");
                    StackTrace stackTrace = new StackTrace(0, true);
                    var maxTrace = Math.Min(3, stackTrace.FrameCount);

                    for (int i = 0; i < maxTrace; i++)
                    {
                        var frame = stackTrace.GetFrame(i);
                        if (frame == null) continue;

                        // Get the file name and line number
                        string fileName = frame.GetFileName();
                        int lineNumber = frame.GetFileLineNumber();

                        // Get the method that called the current method
                        var callingMethod = frame.GetMethod().Name;
                        var methodClass = frame.GetMethod().DeclaringType.Name;
                        ModUtils.Logging.PrintError($" --> \"{fileName}\" | \"{methodClass}.{callingMethod}\", Line {lineNumber}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ModUtils.Logging.PrintError(ex.Message);
            }
        }

        public static string GetModStoreDirectory()
        {
            var path = System.IO.Path.Combine("LocalMods/.modstore", "MultiplayerCrewManager");
            if (false == Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

    }
}